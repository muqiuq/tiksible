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
        public static ISshConnectionInfo GetSshConnectionInfo(this CredentialCfgEntity credential, string hostname)
        {
            if (!string.IsNullOrEmpty(credential.PrivateKey) && !string.IsNullOrEmpty(credential.Password))
            {
                return SshConnectionInfoFactory.CreateCombinedConnectionInfo(hostname, credential.Username, credential.Password,credential.PrivateKey);
            }

            if (!string.IsNullOrEmpty(credential.PrivateKey))
            {
                return SshConnectionInfoFactory.CreatePubKeyConnectionInfo(hostname, credential.Username, credential.PrivateKey);
            }
            else if (!string.IsNullOrEmpty(credential.Password))
            {
                return SshConnectionInfoFactory.CreateUserNamePasswordConnectionInfo(hostname, credential.Username, credential.Password);
            }

            throw new InvalidOperationException("Credential must have either a Password or a PrivateKey or both.");
        }
    }
}
