using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GNS3aaS.CLI.Handler;
using GNS3aaS.CLI.Services;
using Tiksible.Helpers;
using Tiksible.Models.CfgEntities.Extensions;
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

            AddCredHostsDefaultArgument(runCommand, out var credOption, out var hostsOption, out var debugOption);

            runCommand.SetHandler(async (string cmd, string credentialsFileName, string hostsFileName, bool debug) =>
            {
                await HandleRun(cmd, credentialsFileName, hostsFileName, debug);
            }, cmdArg, credOption, hostsOption, debugOption);

            return runCommand;
        }

        private async Task HandleRun(string cmd, string credentialsFileName, string hostsFileName, bool debug)
        {
            LoadHostsAndCredentials(credentialsFileName, hostsFileName);
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
