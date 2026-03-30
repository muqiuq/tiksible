using System;
using System.Collections.Generic;
using System.Linq;
using Tiksible.Models;
using Tiksible.Models.CfgEntities;

namespace Tiksible.Services
{
    public static class VlanDeltaCalculator
    {
        public static VlanDelta Calculate(
            VlanHostConfigCfgEntity config,
            RosStatementList currentBridges,
            RosStatementList currentBridgeVlans,
            RosStatementList currentBridgePorts)
        {
            var delta = new VlanDelta();
            var bridgeCfg = config.Bridges.FirstOrDefault()
                ?? throw new InvalidOperationException("No bridge defined in vlan config.");
            var bridge = bridgeCfg.Name;

            // Step 0 — Check bridge exists; create or update properties
            var currentBridge = currentBridges.FirstOrDefault(s =>
                s.Properties.TryGetValue("name", out var n) && n == bridge);
            if (currentBridge == null)
            {
                var propsStr = string.Concat((bridgeCfg.Properties ?? new()).Select(kv => $" {kv.Key}={QuoteValue(kv.Value)}"));
                delta.BridgeAdd.Add($"/interface bridge add name={bridge} vlan-filtering=yes{propsStr}");
            }
            else if ((bridgeCfg.Properties ?? new()).Any(kv =>
                currentBridge.Properties.GetValueOrDefault(kv.Key, "") != kv.Value))
            {
                var propsStr = string.Concat(bridgeCfg.Properties.Select(kv => $" {kv.Key}={QuoteValue(kv.Value)}"));
                delta.BridgeSets.Add($"/interface bridge set [find name={bridge}]{propsStr}");
            }

            // Step 1 — Expand assignments into port → profile map
            var portToProfile = new Dictionary<string, VlanProfileCfgEntity>();
            foreach (var assignment in config.Assignments)
            {
                var profile = config.Profiles.FirstOrDefault(p => p.Name == assignment.Profile)
                    ?? throw new InvalidOperationException($"Profile '{assignment.Profile}' not found.");
                foreach (var port in ExpandPorts(assignment.Ports, config.DefaultPrefix))
                    portToProfile[port] = profile;
            }

            // Step 2 — Collect all VLAN IDs referenced in the config
            var allVlanIds = new HashSet<int>();
            foreach (var profile in config.Profiles)
            {
                allVlanIds.Add(profile.Untagged);
                if (!IsAll(profile.Tagged))
                {
                    foreach (var id in NormaliseTaggedToList(profile.Tagged))
                        allVlanIds.Add(id);
                }
            }

            // Step 3 — Derive desired bridge vlan table
            var desiredBridgeVlans = new Dictionary<int, (List<string> Tagged, List<string> Untagged)>();
            foreach (var vlanId in allVlanIds)
            {
                var taggedPorts = new List<string>();
                var untaggedPorts = new List<string>();

                foreach (var (port, profile) in portToProfile)
                {
                    if (profile.Untagged == vlanId)
                        untaggedPorts.Add(port);

                    var resolvedTagged = IsAll(profile.Tagged)
                        ? allVlanIds
                        : NormaliseTaggedToList(profile.Tagged).ToHashSet();

                    if (resolvedTagged.Contains(vlanId) && profile.Untagged != vlanId)
                        taggedPorts.Add(port);
                }

                // Always include the bridge interface: untagged for its own PVID, tagged for everything else
                if (vlanId == bridgeCfg.Pvid)
                    untaggedPorts.Add(bridge);
                else
                    taggedPorts.Add(bridge);

                taggedPorts.Sort(StringComparer.Ordinal);
                untaggedPorts.Sort(StringComparer.Ordinal);
                desiredBridgeVlans[vlanId] = (taggedPorts, untaggedPorts);
            }

            // Step 4 — Compare and emit bridge VLAN commands
            foreach (var (vlanId, desired) in desiredBridgeVlans)
            {
                var taggedStr   = string.Join(",", desired.Tagged);
                var untaggedStr = string.Join(",", desired.Untagged);

                var current = currentBridgeVlans.FirstOrDefault(s =>
                    s.Properties.TryGetValue("vlan-ids", out var v) && v == vlanId.ToString());

                if (current == null)
                {
                    delta.BridgeVlanAdds.Add(BuildVlanAddCmd(bridge, vlanId, taggedStr, untaggedStr));
                }
                else
                {
                    var currentTagged   = SortPortList(NormalizeRosListValue(current.Properties.GetValueOrDefault("tagged", "")));
                    var currentUntagged = SortPortList(NormalizeRosListValue(current.Properties.GetValueOrDefault("untagged", "")));
                    if (currentTagged != taggedStr || currentUntagged != untaggedStr)
                        delta.BridgeVlanSets.Add(BuildVlanSetCmd(bridge, vlanId, taggedStr, untaggedStr));
                }
            }

            // Step 5 — Compare and emit bridge port commands
            foreach (var (port, profile) in portToProfile)
            {
                var pvid    = profile.Untagged.ToString();
                var edge    = profile.Edge    ?? "auto";
                var bpduGrd = profile.BpduGuard ?? "no";
                var extraPort = profile.Properties ?? new Dictionary<string, string>();
                var extraPortStr = string.Concat(extraPort.Select(kv => $" {kv.Key}={QuoteValue(kv.Value)}"));

                var current = currentBridgePorts.FirstOrDefault(s =>
                    s.Properties.TryGetValue("interface", out var iface) && iface == port);

                if (current == null)
                {
                    delta.BridgePortAdds.Add(
                        $"/interface bridge port add bridge={bridge} interface={port} pvid={pvid} edge={edge} bpdu-guard={bpduGrd}{extraPortStr}");
                }
                else
                {
                    var curPvid    = current.Properties.GetValueOrDefault("pvid", "1");
                    var curEdge    = current.Properties.GetValueOrDefault("edge", "auto");
                    var curBpdu    = current.Properties.GetValueOrDefault("bpdu-guard", "no");
                    var extraPortChanged = extraPort.Any(kv => current.Properties.GetValueOrDefault(kv.Key, "") != kv.Value);
                    if (curPvid != pvid || curEdge != edge || curBpdu != bpduGrd || extraPortChanged)
                        delta.BridgePortSets.Add(
                            $"/interface bridge port set [find interface={port}] pvid={pvid} edge={edge} bpdu-guard={bpduGrd}{extraPortStr}");
                }
            }

            // Step 6 — Detect unexpected bridge ports (only when NumberOfInterfaces is set)
            if (config.NumberOfInterfaces.HasValue)
            {
                for (int i = 1; i <= config.NumberOfInterfaces.Value; i++)
                {
                    var portName = config.DefaultPrefix + i;
                    if (!portToProfile.ContainsKey(portName))
                    {
                        var exists = currentBridgePorts.Any(s =>
                            s.Properties.TryGetValue("interface", out var iface) && iface == portName);
                        if (exists)
                            delta.BridgePortRemoves.Add($"/interface bridge port remove [find interface={portName}]");
                    }
                }
            }

            return delta;
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static string QuoteValue(string value) =>
            value.Contains(' ') ? $"\"{value}\"" : value;

        private static bool IsAll(object tagged) =>
            tagged is string s && s.Trim().Equals("all", StringComparison.OrdinalIgnoreCase);

        private static HashSet<int> NormaliseTaggedToList(object tagged)
        {
            if (tagged == null) return new HashSet<int>();
            if (tagged is int i) return new HashSet<int> { i };
            if (tagged is string s) return new HashSet<int> { int.Parse(s.Trim()) };
            if (tagged is List<object> list)
                return list.Select(item => item switch
                {
                    int id => id,
                    string str => int.Parse(str.Trim()),
                    _ => throw new InvalidOperationException($"Unexpected tagged item type: {item?.GetType()}")
                }).ToHashSet();
            throw new InvalidOperationException($"Unexpected tagged type: {tagged?.GetType()}");
        }

        private static IEnumerable<string> ExpandPorts(object ports, string defaultPrefix)
        {
            if (ports is string rangeStr)
                return ExpandRange(rangeStr, defaultPrefix);

            if (ports is List<object> list)
                return list.SelectMany(item => item switch
                {
                    string s when s.Contains('-') => ExpandRange(s, defaultPrefix),
                    _ => new[] { ResolvePortName(item, defaultPrefix) }
                });

            return Enumerable.Empty<string>();
        }

        private static IEnumerable<string> ExpandRange(string range, string defaultPrefix)
        {
            var parts = range.Split('-');
            if (parts.Length == 2 && int.TryParse(parts[0].Trim(), out var from) && int.TryParse(parts[1].Trim(), out var to))
                return Enumerable.Range(from, to - from + 1).Select(i => defaultPrefix + i);
            return new[] { range }; // not a numeric range — treat as literal port name
        }

        private static string ResolvePortName(object item, string defaultPrefix) => item switch
        {
            int id => defaultPrefix + id,
            string s when int.TryParse(s.Trim(), out _) => defaultPrefix + s.Trim(),
            string s => s,
            _ => throw new InvalidOperationException($"Cannot resolve port from {item?.GetType()}")
        };

        private static string SortPortList(string portsCsv)
        {
            if (string.IsNullOrWhiteSpace(portsCsv)) return "";
            var parts = portsCsv.Split(',').Select(p => p.Trim()).Where(p => p.Length > 0).ToList();
            parts.Sort(StringComparer.Ordinal);
            return string.Join(",", parts);
        }

        private static string BuildVlanAddCmd(string bridge, int vlanId, string tagged, string untagged)
        {
            var sb = $"/interface bridge vlan add bridge={bridge} vlan-ids={vlanId}";
            if (!string.IsNullOrEmpty(tagged))   sb += $" tagged={tagged}";
            if (!string.IsNullOrEmpty(untagged)) sb += $" untagged={untagged}";
            return sb;
        }

        private static string BuildVlanSetCmd(string bridge, int vlanId, string tagged, string untagged)
        {
            var sb = $"/interface bridge vlan set [find bridge={bridge} vlan-ids={vlanId} !dynamic]";
            // Always include both fields. Use quoted empty string to clear a list in RouterOS.
            sb += $" tagged={RosListValue(tagged)}";
            sb += $" untagged={RosListValue(untagged)}";
            return sb;
        }

        /// Returns the value suitable for a RouterOS port-list assignment.
        /// Empty list must be written as \"\" so RouterOS actually clears it.
        private static string RosListValue(string csv) =>
            string.IsNullOrEmpty(csv) ? "\"\"" : csv;

        /// Normalises a value read back from a RouterOS export: strips surrounding quotes
        /// so that \"\" (which RouterOS may export for a cleared list) compares equal to \"\".
        private static string NormalizeRosListValue(string v) =>
            v.Length >= 2 && v.StartsWith('"') && v.EndsWith('"') ? v[1..^1] : v;
    }
}
