using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiksible.Theater.SshHelpers;

namespace Tiksible.Models.CfgEntities.Extensions
{
    public static class CredentialCfgEntityExtensions
    {
        public static ISshConnectionInfo GetSshConnectionInfo(this CredentialCfgEntity credential, HostCfgEntity host)
        {
            if (!string.IsNullOrEmpty(credential.PrivateKey) && !string.IsNullOrEmpty(credential.Password))
            {
                return SshConnectionInfoFactory.CreateCombinedConnectionInfo(host.Address, credential.Username, credential.Password,credential.PrivateKey, host.SshPort);
            }

            if (!string.IsNullOrEmpty(credential.PrivateKey))
            {
                return SshConnectionInfoFactory.CreatePubKeyConnectionInfo(host.Address, credential.Username, credential.PrivateKey, host.SshPort);
            }
            else if (!string.IsNullOrEmpty(credential.Password))
            {
                return SshConnectionInfoFactory.CreateUserNamePasswordConnectionInfo(host.Address, credential.Username, credential.Password, host.SshPort);
            }

            throw new InvalidOperationException("Credential must have either a Password or a PrivateKey or both.");
        }
    }
}
