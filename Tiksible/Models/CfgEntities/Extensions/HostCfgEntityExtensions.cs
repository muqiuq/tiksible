using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible.Models.CfgEntities.Extensions
{
    public static class HostCfgEntityExtensions
    {
        public static CredentialCfgEntity? GetCredentials(this HostCfgEntity host, CredentialsCfgEntity credentialsConfig)
        {
            return credentialsConfig.Credentials
                .FirstOrDefault(cred => cred.Name.Equals(host.CredentialsAlias, StringComparison.OrdinalIgnoreCase));
        }
    }
}
