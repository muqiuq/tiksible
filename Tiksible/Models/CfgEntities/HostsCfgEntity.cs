using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible.Models.CfgEntities
{
    public class HostsCfgEntity
    {
        public List<HostCfgEntity> Hosts { get; set; } = new List<HostCfgEntity>();
    }
}
