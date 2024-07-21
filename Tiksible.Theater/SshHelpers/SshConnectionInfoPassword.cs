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
        public string Username { get; set; }

        public string Password { get; set; }

        internal SshConnectionInfoPassword(string hostName, string username, string password)
        {
            HostName = hostName;
            Username = username;
            Password = password;
        }

        public ConnectionInfo GetConnectionInfo()
        {
            return new ConnectionInfo(HostName, Username, new PasswordAuthenticationMethod(Username, Password));
        }
    }
}
