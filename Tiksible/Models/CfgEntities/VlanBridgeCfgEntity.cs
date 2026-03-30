using System.Collections.Generic;

namespace Tiksible.Models.CfgEntities
{
    public class VlanBridgeCfgEntity
    {
        public string Name { get; set; } = "bridge";
        /// <summary>Native VLAN ID of the bridge interface itself. Bridge appears untagged for this VLAN and tagged for all others.</summary>
        public int Pvid { get; set; } = 1;
        /// <summary>Extra RouterOS properties applied to the bridge (e.g. vlan-filtering, protocol-mode). Compared when set.</summary>
        public Dictionary<string, string> Properties { get; set; } = new();
    }
}
