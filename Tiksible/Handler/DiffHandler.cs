using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GNS3aaS.CLI.Handler;
using GNS3aaS.CLI.Services;
using Scriban;
using Tiksible.Exceptions;
using Tiksible.Helpers;
using Tiksible.Models.CfgEntities.Extensions;
using Tiksible.Services;
using Tiksible.Theater.Playbooks;

namespace Tiksible.Handler
{
    public class DiffHandler : BaseHandler, IHandler
    {
        public DiffHandler(ConfigStorage configStorage) : base(configStorage)
        {
        }

        public Command GetCommand()
        {
            var applyCommand = new Command("diff", "compare remote config with template and apply diff");

            var rscFilenameArg = new Argument<string>("rsc-filename", "path to the rsc file");
            applyCommand.AddArgument(rscFilenameArg);

            var writeOption = new Option<bool>(
                "--write",
                "executes commands on remote system"
            );
            applyCommand.AddOption(writeOption);

            AddCredHostsDefaultArgument(applyCommand, out var credOption, out var hostsOption, out var debugOption, out var hostFilterOption);

            applyCommand.SetHandler(async (string rscFilename, bool write, string credentialsFileName, string hostsFileName, bool debug, string hostFilter) =>
            {
                await HandleApply(rscFilename, write, credentialsFileName, hostsFileName, debug, hostFilter);
            }, rscFilenameArg, writeOption, credOption, hostsOption, debugOption, hostFilterOption);

            return applyCommand;
        }


        private async Task HandleApply(string rscFilename, bool write, string credentialsFileName, string hostsFileName, bool debug, string hostFilter)
        {
            LoadHostsAndCredentials(credentialsFileName, hostsFileName, hostFilter);
            CheckCredentialsForAllHosts();

            if (!File.Exists(rscFilename))
            {
                throw new UserArgumentErrorException($"rsc file not found @ {Path.GetFullPath(rscFilename)}");
            }

            var templateFileContent = await File.ReadAllTextAsync(rscFilename);

            var template = Template.Parse(templateFileContent);



            foreach (var host in Hosts.Hosts)
            {
                Console.WriteLine(ConsoleOutputHelper.MakeDeviderLine($"DIFF @ {host.Name}"));

                var goalRscFileRaw = template.Render(new { Host = host, Credentials = host.GetCredentials(Credentials) });

                if (debug)
                {
                    Console.WriteLine(ConsoleOutputHelper.MakeDeviderLine($"RENDERED RSC FILE for {host.Name}"));
                    Console.WriteLine(goalRscFileRaw);
                    Console.WriteLine(ConsoleOutputHelper.MakeDeviderLine($"END"));
                }
                

                var conInfo = host.GetCredentials(Credentials)!.GetSshConnectionInfo(host.Address);

                var exportPlaybook = RunPlaybook(conInfo, new ExportPlaybook());

                var isRscConfigString = exportPlaybook.Artifacts[ExportPlaybook.ArtifactName];

                var isRscConfig = RosRscParser.Parse(isRscConfigString);
                var shouldRscConfig = RosRscParser.Parse(goalRscFileRaw);

                RosRscStatementCleaner.RemoveNotRequiredParameters(isRscConfig);
                RosRscStatementCleaner.RemoveNotRequiredParameters(shouldRscConfig);

                var comparisonResult = isRscConfig.Compare(shouldRscConfig);

                foreach (var statement in comparisonResult.MissingStatemenetsOwn)
                {
                    var statementStr = statement.Export();
                    Console.WriteLine(statementStr);
                    if (write)
                    {
                        var runPlaybook = RunPlaybook(conInfo, new RunSingleCmdPlaybook(statementStr));
                        ConsoleOutputHelper.PrintStatusLine(statementStr, runPlaybook.IsSuccess());
                    }
                }

                

                Console.WriteLine(ConsoleOutputHelper.MakeDeviderLine($"FINISHED DIFF @ {host.Name}"));
            }
        }
    }
}

