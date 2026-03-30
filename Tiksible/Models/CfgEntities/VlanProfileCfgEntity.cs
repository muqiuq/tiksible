using System.Collections.Generic;

namespace Tiksible.Models.CfgEntities
{
    public class VlanProfileCfgEntity
    {
        public string Name { get; set; }
        public string Comment { get; set; }
        public string Edge { get; set; } = "auto";
        public string BpduGuard { get; set; } = "no";
        /// <summary>The VLAN ID that is native (untagged/pvid) on assigned ports.</summary>
        public int Untagged { get; set; }
        /// <summary>
        /// Either the string "all", a single int VLAN ID, or a List&lt;object&gt; of VLAN IDs.
        /// </summary>
        public object Tagged { get; set; }
        /// <summary>Extra RouterOS properties appended to /interface bridge port add/set for ports assigned this profile. Only compared when set.</summary>
        public Dictionary<string, string> Properties { get; set; } = new();
    }
}
