using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiksible.Helpers;
using Tiksible.Models.CfgEntities.Extensions;
using Tiksible.Services;
using Tiksible.Theater.Playbooks;

namespace Tiksible.Handler
{
    public class RunHandler: BaseHandler, IHandler
    {
        public RunHandler(ConfigStorage configStorage) : base(configStorage)
        {
        }

        public Command GetCommand()
        {
            var runCommand = new Command("run", "run single command on multiple hosts");

            var cmdArg = new Argument<string>("cmd", "command to run");
            runCommand.AddArgument(cmdArg);

            AddCredHostsDefaultArgument(runCommand, out var credOption, out var hostsOption, out var debugOption, out var hostFilterOption);

            runCommand.SetHandler(async (string cmd, string credentialsFileName, string hostsFileName, bool debug, string hostFilter) =>
            {
                await HandleRun(cmd, credentialsFileName, hostsFileName, debug, hostFilter);
            }, cmdArg, credOption, hostsOption, debugOption, hostFilterOption);

            return runCommand;
        }

        private async Task HandleRun(string cmd, string credentialsFileName, string hostsFileName, bool debug, string hostFilter)
        {
            LoadHostsAndCredentials(credentialsFileName, hostsFileName, hostFilter);
            CheckCredentialsForAllHosts();

            foreach (var host in Hosts.Hosts)
            {
                Console.WriteLine(ConsoleOutputHelper.MakeDeviderLine($"RUN @ {host.Name}"));
                
                var conInfo = host.GetCredentials(Credentials)!.GetSshConnectionInfo(host.Address);

                var runPlaybook = RunPlaybook(conInfo, new RunSingleCmdPlaybook(cmd));
                
                ConsoleOutputHelper.PrintStatusLine($"{cmd}", runPlaybook.IsSuccess());
                Console.WriteLine();
                Console.WriteLine(runPlaybook.Artifacts[RunSingleCmdPlaybook.OutputArtifactName]);

                Console.WriteLine(ConsoleOutputHelper.MakeDeviderLine($"FINISHED RUN @ {host.Name}"));
            }
        }
    }
}
