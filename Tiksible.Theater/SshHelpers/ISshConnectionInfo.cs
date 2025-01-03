using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;

namespace Tiksible.Theater.SshHelpers
{
    public interface ISshConnectionInfo
    {
        public string HostName { get; set; }
        
        public int SshPort { get; set; }

        public string Username { get; set; }

        public bool SshOnly { get; set; }

        public ConnectionInfo GetConnectionInfo();

    }
}
