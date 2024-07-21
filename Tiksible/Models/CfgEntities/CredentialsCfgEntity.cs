using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible.Models.CfgEntities
{
    public class CredentialsCfgEntity
    {
        public List<CredentialCfgEntity> Credentials { get; set; } = new List<CredentialCfgEntity>();

    }
}
