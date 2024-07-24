using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible.Theater.SshHelpers
{
    public static class SshConnectionInfoFactory
    {

        public static ISshConnectionInfo CreateUserNamePasswordConnectionInfo(string hostname, string username,
            string password, int sshPort)
        {
            return new SshConnectionInfoPassword(hostname, username, password, sshPort);
        }

        public static ISshConnectionInfo CreatePubKeyConnectionInfo(string hostname, string username, string privateKey, int sshPort)
        {
            return new SshConnectionInfoPubKey(hostname, username, privateKey, sshPort);
        }

        public static ISshConnectionInfo CreateCombinedConnectionInfo(string hostname, string username, string password, string privateKey, int sshPort)
        {
            return new SshConnectionInfoPubKeyPassword(hostname, username, privateKey, password, sshPort);
        }
    }
}
