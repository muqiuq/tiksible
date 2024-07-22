using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiksible.Helpers;
using Tiksible.Models.CfgEntities.Extensions;
using Tiksible.Models.CfgEntities;
using Tiksible.Services;
using Tiksible.Theater.Playbooks;
using Tiksible.Theater.SshHelpers;
using Renci.SshNet.Common;
using System.ComponentModel;
using Tiksible.Theater;

namespace Tiksible.Handler
{
    public class TestHandler : BaseHandler, IHandler
    {
        public TestHandler(ConfigStorage configStorage) : base(configStorage)
        {
        }

        public Command GetCommand()
        {
            var cmd = new Command("test", "test ssh connection to hosts");

            AddCredHostsDefaultArgument(cmd, out var credOption, out var hostsOption, out var debugOption, out var hostFilterOption);

            cmd.SetHandler(async (credentialsFileName, hostsFileName, debug, hostFilter) =>
            {
                await HandleTest(credentialsFileName, hostsFileName, debug, hostFilter);
            }, credOption, hostsOption, debugOption, hostFilterOption);

            return cmd;
        }

        private async Task HandleTest(string credentialsFileName, string hostsFileName, bool debug, string hostFilter)
        {
            LoadHostsAndCredentials(credentialsFileName, hostsFileName, hostFilter);
            CheckCredentialsForAllHosts();

            foreach (var host in Hosts.Hosts)
            {
                var conInfo = host.GetCredentials(Credentials)!.GetSshConnectionInfo(host.Address);
                
                ConsoleOutputHelper.PrintStatusLine($"{host.Name}", SshConnectionTestService.TestConnection(conInfo));
            }
        }
    }
}
