using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;

namespace Tiksible.Theater.SshHelpers
{
    internal class SshConnectionInfoPubKeyPassword : ISshConnectionInfo
    {
        public string HostName { get; set; }
        public string Username { get; set; }

        public string PrivateKey { get; set; }

        public string Password { get; set; }

        internal SshConnectionInfoPubKeyPassword(string hostName, string username, string privateKey, string password)
        {
            HostName = hostName;
            Username = username;
            PrivateKey = privateKey;
            Password = password;
        }

        public ConnectionInfo GetConnectionInfo()
        {
            var privateKeyFile = new PrivateKeyFile(new MemoryStream(Encoding.UTF8.GetBytes(PrivateKey)));

            var connectionInfo = new ConnectionInfo(HostName,
                Username,
                new PrivateKeyAuthenticationMethod(Username, privateKeyFile), new PasswordAuthenticationMethod(Username, Password));

            return connectionInfo;
        }
    }
}
