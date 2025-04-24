using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible.Models.CfgEntities
{
    public class PerVlanCfgEntity
    {

        public int Vid { get; set; }

        public List<string> Untagged { get; set; }

        public List<string> Tagged { get; set; }

        public string Comment { get; set; }

    }
}
