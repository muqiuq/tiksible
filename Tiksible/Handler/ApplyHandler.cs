using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;
using Tiksible.Exceptions;
using Tiksible.Extensions;
using Tiksible.Helpers;
using Tiksible.Models.CfgEntities.Extensions;
using Tiksible.Services;
using Tiksible.Theater;
using Tiksible.Theater.Playbooks;

namespace Tiksible.Handler
{
    public class ApplyHandler : BaseHandler, IHandler
    {
        public ApplyHandler(ConfigStorage configStorage) : base(configStorage)
        {

        }


        public Command GetCommand()
        {
            var applyCommand = new Command("apply", "render template and apply to hosts");

            var rscFilenameArg = new Argument<string>("rsc-filename", "path to the rsc file");
            applyCommand.AddArgument(rscFilenameArg);

            var writeOption = new Option<bool>(
                "--write",
                "executes commands on remote system"
            );
            applyCommand.AddOption(writeOption);

            AddCredHostsDefaultArgument(applyCommand, out var credOption, out var hostsOption, out var debugOption, out var filterOption);

            applyCommand.SetHandler(async (string rscFilename, bool write, string credentialsFileName, string hostsFileName, bool debug, string hostFilter) =>
            {
                await HandleApply(rscFilename, write, credentialsFileName, hostsFileName, debug, hostFilter);
            }, rscFilenameArg, writeOption, credOption, hostsOption, debugOption, filterOption);

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
                Console.WriteLine(ConsoleOutputHelper.MakeDeviderLine($"APPLY @ {host.Name}"));

                var templateContext = new TemplateContext();
                templateContext.TemplateLoader = new ScibanTemplateLoader();
                templateContext.PushSourceFile(Path.GetFullPath(rscFilename));
                var so = new ScriptObject();
                so.Import(new { Host = host, Credentials = host.GetCredentials(Credentials)});
                templateContext.PushGlobal((so));
                
                var goalRscFileRaw = template.Render(templateContext);

                if (debug)
                {
                    Console.WriteLine(ConsoleOutputHelper.MakeDeviderLine($"RENDERED RSC FILE for {host.Name}"));
                    Console.WriteLine(goalRscFileRaw);
                    Console.WriteLine(ConsoleOutputHelper.MakeDeviderLine($"END"));
                }

                if (write)
                {
                    var conInfo = host.GetCredentials(Credentials)!.GetSshConnectionInfo(host);

                    var playbookRunner = new PlaybookRunner(conInfo, new RunRscScriptPlaybook());

                    playbookRunner.Files.Add(RunRscScriptPlaybook.FileName, Encoding.UTF8.GetBytes(goalRscFileRaw));

                    playbookRunner.Run();

                    ConsoleOutputHelper.PrintStatusLine($"Apply {rscFilename} @ {host.Name}", playbookRunner.IsSuccess());

                    Console.WriteLine(ConsoleOutputHelper.MakeDeviderLine($"OUTPUT", '-'));
                    Console.WriteLine(playbookRunner.Artifacts[RunRscScriptPlaybook.Output]);
                }

                Console.WriteLine(ConsoleOutputHelper.MakeDeviderLine($"END APPLY @ {host.Name}"));
            }
        }
    }
}
