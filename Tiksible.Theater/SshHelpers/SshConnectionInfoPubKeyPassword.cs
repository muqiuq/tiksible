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
        public int SshPort { get; set; }
        public string Username { get; set; }

        public string PrivateKey { get; set; }

        public string Password { get; set; }

        public bool SshOnly { get; set; }

        internal SshConnectionInfoPubKeyPassword(string hostName, string username, string privateKey, string password, int sshPort, bool sshOnly)
        {
            HostName = hostName;
            Username = username;
            PrivateKey = privateKey;
            Password = password;
            SshPort = sshPort;
            SshOnly = sshOnly;
        }

        public ConnectionInfo GetConnectionInfo()
        {
            PrivateKeyFile privateKeyFile;
            if (!PrivateKey.StartsWith("-----BEGIN OPENSSH PRIVATE KEY-----"))
            {
                var privateKeyPath = PrivateKey.Trim();
                if (privateKeyPath.StartsWith("~/"))
                {
                    privateKeyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),privateKeyPath.Substring(2));
                }

                privateKeyFile = new PrivateKeyFile(File.OpenRead(Path.GetFullPath(privateKeyPath)));
            }
            else
            {
                privateKeyFile = new PrivateKeyFile(new MemoryStream(Encoding.UTF8.GetBytes(PrivateKey)));
            }
            
            var connectionInfo = new ConnectionInfo(HostName,
                SshPort,
                Username,
                new PrivateKeyAuthenticationMethod(Username, privateKeyFile), new PasswordAuthenticationMethod(Username, Password));

            return connectionInfo;
        }
    }
}
