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

        internal SshConnectionInfoPassword(string hostName, string username, string password, int sshPort)
        {
            HostName = hostName;
            Username = username;
            Password = password;
            SshPort = sshPort;
        }

        public ConnectionInfo GetConnectionInfo()
        {
            return new ConnectionInfo(HostName, SshPort, Username, new PasswordAuthenticationMethod(Username, Password));
        }
    }
}
