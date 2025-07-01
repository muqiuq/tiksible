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


            AddOutputDefaultOptions(runCommand, out var outputOption, out var overwriteOutputOption);

            AddCredHostsDefaultArgument(runCommand, out var credOption, out var hostsOption, out var debugOption, out var hostFilterOption);

            runCommand.SetHandler(async (string cmd, string? output, bool overwriteOutput, string credentialsFileName, string hostsFileName, bool debug, string hostFilter) =>
            {
                await HandleRun(cmd, output, overwriteOutput,  credentialsFileName, hostsFileName, debug, hostFilter);
            }, cmdArg, outputOption, overwriteOutputOption, credOption, hostsOption, debugOption, hostFilterOption);

            return runCommand;
        }

        private async Task HandleRun(string cmd, string? outputPath, bool overwriteOutput, string credentialsFileName, string hostsFileName, bool debug, string hostFilter)
        {
            LoadHostsAndCredentials(credentialsFileName, hostsFileName, hostFilter);
            CheckCredentialsForAllHosts();

            foreach (var host in Hosts.Hosts)
            {
                Console.WriteLine(ConsoleOutputHelper.MakeDeviderLine($"RUN @ {host.Name}"));
        
                var conInfo = host.GetCredentials(Credentials)!.GetSshConnectionInfo(host);

                var runPlaybook = RunPlaybook(conInfo, new RunSingleCmdPlaybook(cmd));
        
                ConsoleOutputHelper.PrintStatusLine($"{cmd}", runPlaybook.IsSuccess());
                Console.WriteLine();
        
                var output = runPlaybook.Artifacts[RunSingleCmdPlaybook.OutputArtifactName];
                Console.WriteLine(output);

                if (!string.IsNullOrEmpty(outputPath))
                {
                    var originalOutputPath = outputPath;
                    int counter = 0;
                    while (File.Exists(outputPath) && !overwriteOutput)
                    {
                        outputPath = $"{originalOutputPath}.{counter}";
                        counter++;
                        if (counter > 10000) throw new InvalidDataException("Cannot find non existent output file name");
                    }
                    await File.WriteAllTextAsync(outputPath, output);
                    Console.WriteLine(ConsoleOutputHelper.MakeDeviderLine($"Wrote output to {outputPath}"));
                }

                Console.WriteLine(ConsoleOutputHelper.MakeDeviderLine($"FINISHED RUN @ {host.Name}"));
            }
        }
    }
}