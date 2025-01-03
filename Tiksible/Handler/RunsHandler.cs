using Scriban;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiksible.Exceptions;
using Tiksible.Helpers;
using Tiksible.Models.CfgEntities.Extensions;
using Tiksible.Services;
using Tiksible.Theater.Playbooks;

namespace Tiksible.Handler
{
    public class RunsHandler : BaseHandler, IHandler
    {
        public RunsHandler(ConfigStorage configStorage) : base(configStorage)
        {
        }

        public Command GetCommand()
        {
            var runCommand = new Command("runs", "run multiple command on multiple hosts");

            var rscFilenameArg = new Argument<string>("filename", "path to the command file");
            runCommand.AddArgument(rscFilenameArg);

            AddCredHostsDefaultArgument(runCommand, out var credOption, out var hostsOption, out var debugOption, out var hostFilterOption);

            runCommand.SetHandler(async (string rscFilenameArg, string credentialsFileName, string hostsFileName, bool debug, string hostFilter) =>
            {
                await HandleRuns(rscFilenameArg, credentialsFileName, hostsFileName, debug, hostFilter);
            }, rscFilenameArg, credOption, hostsOption, debugOption, hostFilterOption);

            return runCommand;
        }

        private async Task HandleRuns(string rscFilename, string credentialsFileName, string hostsFileName, bool debug, string hostFilter)
        {
            LoadHostsAndCredentials(credentialsFileName, hostsFileName, hostFilter);
            CheckCredentialsForAllHosts();

            if (!File.Exists(rscFilename))
            {
                throw new UserArgumentErrorException($"command file not found @ {Path.GetFullPath(rscFilename)}");
            }

            var templateFileContent = await File.ReadAllTextAsync(rscFilename);

            var template = Template.Parse(templateFileContent);

            foreach (var host in Hosts.Hosts)
            {
                Console.WriteLine(ConsoleOutputHelper.MakeDeviderLine($"RUNS @ {host.Name}"));

                var goalRscFileRaw = template.Render(new { Host = host, Credentials = host.GetCredentials(Credentials) });

                var goalRscFileLines = goalRscFileRaw.Replace("\r", "").Split("\n");

                var conInfo = host.GetCredentials(Credentials)!.GetSshConnectionInfo(host);

                var runPlaybook = RunPlaybook(conInfo, new RunMultipleCmdPlaybook(goalRscFileLines));

                foreach (var artifact in runPlaybook.Artifacts)
                {
                    Console.WriteLine($"# {artifact.Key}");
                    Console.WriteLine(artifact.Value);
                }
                
                Console.WriteLine();

                Console.WriteLine(ConsoleOutputHelper.MakeDeviderLine($"FINISHED RUNS @ {host.Name}"));
            }
        }
    }
}
