using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet.Common;
using Tiksible.Theater.SshHelpers;

namespace Tiksible.Theater
{
    public class SshConnectionTestService
    {
        public const int DefaultTimeoutSeconds = 5;

        public static bool TestConnection(ISshConnectionInfo sshConnectionInfo, TimeSpan? timeout = null)
        {
            var connectionInfo = sshConnectionInfo.GetConnectionInfo();

            connectionInfo.Timeout = timeout ?? TimeSpan.FromSeconds(DefaultTimeoutSeconds);
            try
            {
                using (var client = new SshClient(connectionInfo))
                {
                    client.Connect();
                    return true;
                }
            }
            catch (Exception ex) when (ex is SocketException || ex is SshException || ex is ProxyException)
            {
                return false;
            }
        }
    }
}
