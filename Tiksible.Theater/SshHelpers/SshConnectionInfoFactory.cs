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
            string password, int sshPort, bool sshOnly)
        {
            return new SshConnectionInfoPassword(hostname, username, password, sshPort, sshOnly);
        }

        public static ISshConnectionInfo CreatePubKeyConnectionInfo(string hostname, string username, string privateKey, int sshPort, bool sshOnly)
        {
            return new SshConnectionInfoPubKey(hostname, username, privateKey, sshPort, sshOnly);
        }

        public static ISshConnectionInfo CreateCombinedConnectionInfo(string hostname, string username, string password, string privateKey, int sshPort, bool sshOnly)
        {
            return new SshConnectionInfoPubKeyPassword(hostname, username, privateKey, password, sshPort, sshOnly);
        }
    }
}
