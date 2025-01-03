using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;

namespace Tiksible.Theater.SshHelpers
{
    internal class SshConnectionInfoPubKey : ISshConnectionInfo
    {
        public string HostName { get; set; }
        public int SshPort { get; set; }
        public string Username { get; set; }

        public string PrivateKey { get; set; }

        public bool SshOnly { get; set; }

        internal SshConnectionInfoPubKey(string hostName, string username, string privateKey, int sshPort, bool sshOnly)
        {
            HostName = hostName;
            Username = username;
            PrivateKey = privateKey;
            SshPort = sshPort;
            SshOnly = sshOnly;
        }

        public ConnectionInfo GetConnectionInfo()
        {
            var privateKeyFile = new PrivateKeyFile(new MemoryStream(Encoding.UTF8.GetBytes(PrivateKey)));

            var connectionInfo = new ConnectionInfo(HostName,
                SshPort,
                Username,
                new PrivateKeyAuthenticationMethod(Username, privateKeyFile));

            return connectionInfo;
        }
    }
}
