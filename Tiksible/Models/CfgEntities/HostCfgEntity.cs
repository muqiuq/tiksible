using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible.Models.CfgEntities
{
    public class HostCfgEntity
    {

        public string Name { get; set; }

        public string Address { get; set; }

        public string CredentialsAlias { get; set; }

        public Dictionary<string, object> Params { get; set; }
    }
}
