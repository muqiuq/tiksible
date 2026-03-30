using System.Collections.Generic;

namespace Tiksible.Models.CfgEntities
{
    public class VlanHostConfigCfgEntity
    {
        public string DefaultPrefix { get; set; } = "ether";
        public int? NumberOfInterfaces { get; set; }
        public List<VlanBridgeCfgEntity> Bridges { get; set; } = new();
        public List<VlanProfileCfgEntity> Profiles { get; set; } = new();
        public List<VlanAssignmentCfgEntity> Assignments { get; set; } = new();
    }
}
