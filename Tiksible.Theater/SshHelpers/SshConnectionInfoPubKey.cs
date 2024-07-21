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
        public string Username { get; set; }

        public string PrivateKey { get; set; }

        internal SshConnectionInfoPubKey(string hostName, string username, string privateKey)
        {
            HostName = hostName;
            Username = username;
            PrivateKey = privateKey;
        }

        public ConnectionInfo GetConnectionInfo()
        {
            var privateKeyFile = new PrivateKeyFile(new MemoryStream(Encoding.UTF8.GetBytes(PrivateKey)));

            var connectionInfo = new ConnectionInfo(HostName,
                Username,
                new PrivateKeyAuthenticationMethod(Username, privateKeyFile));

            return connectionInfo;
        }
    }
}
