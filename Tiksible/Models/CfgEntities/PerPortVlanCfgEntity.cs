using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible.Models.CfgEntities
{
    public class PerPortVlanCfgEntity
    {
        public int? PortNumber { get; set; }

        public string? PortName { get; set; }

        public int Untagged { get; set; }

        public List<int> Tagged { get; set; }

    }
}
