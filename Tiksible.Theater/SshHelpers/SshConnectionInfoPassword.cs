using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;

namespace Tiksible.Theater.SshHelpers
{
    public class SshConnectionInfoPassword : ISshConnectionInfo
    {
        public string HostName { get; set; }
        public int SshPort { get; set; }
        public string Username { get; set; }

        public string Password { get; set; }
        public bool SshOnly { get; set; }

        internal SshConnectionInfoPassword(string hostName, string username, string password, int sshPort, bool sshOnly)
        {
            HostName = hostName;
            Username = username;
            Password = password;
            SshPort = sshPort;
            SshOnly = sshOnly;
        }

        public ConnectionInfo GetConnectionInfo()
        {
            return new ConnectionInfo(HostName, SshPort, Username, new PasswordAuthenticationMethod(Username, Password));
        }
    }
}
