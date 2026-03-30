using System.Collections.Generic;

namespace Tiksible.Models.CfgEntities
{
    public class VlanAssignmentCfgEntity
    {
        /// <summary>
        /// Port references: a List&lt;object&gt; of port refs (int, string, "X-Y" range),
        /// or a single string range "X-Y".
        /// </summary>
        public object Ports { get; set; }

        /// <summary>Name of the profile to assign to these ports.</summary>
        public string Profile { get; set; }
    }
}
