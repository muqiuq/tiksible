using System;
using System.Collections.Generic;
using System.Linq;
using Tiksible.Models.CfgEntities;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Tiksible.Services
{
    public static class VlanConfigParser
    {
        public static VlanHostConfigCfgEntity? Parse(HostCfgEntity host, IEnumerable<HostCfgEntity>? allHosts = null)
        {
            return ParseInternal(host, allHosts, []);
        }

        private static VlanHostConfigCfgEntity? ParseInternal(
            HostCfgEntity host,
            IEnumerable<HostCfgEntity>? allHosts,
            HashSet<string> visited)
        {
            if (host.Params == null || !host.Params.ContainsKey("vlan"))
                return null;

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yaml = serializer.Serialize(host.Params["vlan"]);
            var local = deserializer.Deserialize<VlanHostConfigCfgEntity>(yaml);

            if (local.Inherit == null || allHosts == null)
                return local;

            var inheritName = local.Inherit;
            if (!visited.Add(host.Name))
                throw new InvalidOperationException(
                    $"Circular VLAN inheritance detected at host '{host.Name}'.");

            var baseHost = allHosts.FirstOrDefault(h => h.Name == inheritName)
                ?? throw new InvalidOperationException(
                    $"Host '{host.Name}' inherits VLAN config from '{inheritName}', but that host was not found.");

            var baseConfig = ParseInternal(baseHost, allHosts, visited)
                ?? throw new InvalidOperationException(
                    $"Host '{host.Name}' inherits VLAN config from '{inheritName}', but that host has no vlan config.");

            return Merge(baseConfig, local);
        }

        /// <summary>
        /// Merges <paramref name="local"/> on top of <paramref name="inherited"/>.
        /// Profiles and bridges are merged by name (local wins on conflicts).
        /// Assignments are: inherited first, then local appended (so local port assignments take precedence).
        /// Scalar fields (DefaultPrefix, NumberOfInterfaces) keep the local value when explicitly set.
        /// </summary>
        private static VlanHostConfigCfgEntity Merge(VlanHostConfigCfgEntity inherited, VlanHostConfigCfgEntity local)
        {
            // Profiles: start with inherited, override/add local by name
            var profiles = inherited.Profiles
                .Where(p => local.Profiles.All(lp => lp.Name != p.Name))
                .Concat(local.Profiles)
                .ToList();

            // Bridges: same pattern
            var bridges = inherited.Bridges
                .Where(b => local.Bridges.All(lb => lb.Name != b.Name))
                .Concat(local.Bridges)
                .ToList();

            // Assignments: inherited first so local entries (appended later) win when the
            // calculator builds its portToProfile dictionary (last writer wins per port).
            var assignments = inherited.Assignments.Concat(local.Assignments).ToList();

            return new VlanHostConfigCfgEntity
            {
                Inherit = null,
                DefaultPrefix = local.DefaultPrefix,          // local keeps its default "ether" unless overridden
                NumberOfInterfaces = local.NumberOfInterfaces ?? inherited.NumberOfInterfaces,
                Profiles = profiles,
                Bridges = bridges,
                Assignments = assignments,
            };
        }
    }
}
