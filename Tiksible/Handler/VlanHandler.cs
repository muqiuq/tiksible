using System;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;
using Tiksible.Helpers;
using Tiksible.Models.CfgEntities.Extensions;
using Tiksible.Services;
using Tiksible.Theater.Playbooks;

namespace Tiksible.Handler
{
    public class VlanHandler : BaseHandler, IHandler
    {
        public VlanHandler(ConfigStorage configStorage) : base(configStorage)
        {
        }

        public Command GetCommand()
        {
            var vlanCommand = new Command("vlan", "calculate and optionally apply VLAN configuration delta");

            var executeOption = new Option<bool>(
                ["-e", "--execute"],
                () => false,
                "apply changes to the device (default: dry-run, print only)"
            );
            vlanCommand.AddOption(executeOption);

            AddCredHostsDefaultArgument(vlanCommand, out var credOption, out var hostsOption, out var debugOption, out var filterOption);

            vlanCommand.SetHandler(
                async (bool execute, string credFile, string hostsFile, bool debug, string hostFilter) =>
                {
                    await HandleVlan(execute, credFile, hostsFile, debug, hostFilter);
                },
                executeOption, credOption, hostsOption, debugOption, filterOption);

            return vlanCommand;
        }

        private async Task HandleVlan(bool execute, string credFile, string hostsFile, bool debug, string hostFilter)
        {
            LoadHostsAndCredentials(credFile, hostsFile, hostFilter);
            CheckCredentialsForAllHosts();

            foreach (var host in Hosts.Hosts)
            {
                Console.WriteLine(ConsoleOutputHelper.MakeDividerLine($"VLAN @ {host.Name}"));

                var vlanConfig = VlanConfigParser.Parse(host, Hosts.Hosts);
                if (vlanConfig == null)
                {
                    Console.WriteLine("  No vlan config, skipping.");
                    Console.WriteLine(ConsoleOutputHelper.MakeDividerLine($"END VLAN @ {host.Name}"));
                    continue;
                }

                var conInfo = host.GetCredentials(Credentials)!.GetSshConnectionInfo(host);

                var bridgeRunner     = RunPlaybook(conInfo, new FetchBridgePlaybook());
                var bridgeVlanRunner = RunPlaybook(conInfo, new FetchBridgeVlanPlaybook());
                var bridgePortRunner = RunPlaybook(conInfo, new FetchBridgePortPlaybook());

                var currentBridges = RosRscParser.Parse(bridgeRunner.Artifacts[FetchBridgePlaybook.ArtifactName]);
                var currentVlans   = RosRscParser.Parse(bridgeVlanRunner.Artifacts[FetchBridgeVlanPlaybook.ArtifactName]);
                var currentPorts   = RosRscParser.Parse(bridgePortRunner.Artifacts[FetchBridgePortPlaybook.ArtifactName]);

                var delta = VlanDeltaCalculator.Calculate(vlanConfig, currentBridges, currentVlans, currentPorts);

                if (!delta.HasChanges)
                {
                    Console.WriteLine("  No changes needed.");
                }
                else
                {
                    var commands = delta.AllCommands().ToList();
                    Console.WriteLine($"  {(execute ? "Applying" : "[DRY RUN]")} {commands.Count} change(s):");
                    foreach (var cmd in commands)
                    {
                        Console.WriteLine($"  {cmd}");
                        if (execute)
                        {
                            var result = RunPlaybook(conInfo, new RunSingleCmdPlaybook(cmd));
                            var output = result.Artifacts.TryGetValue(RunSingleCmdPlaybook.OutputArtifactName, out var o) ? o : "";
                            var rosSuccess = result.IsSuccess() && !output.TrimStart().StartsWith("failure:", StringComparison.OrdinalIgnoreCase);
                            ConsoleOutputHelper.PrintStatusLine(cmd, rosSuccess);
                        }
                    }
                }

                Console.WriteLine(ConsoleOutputHelper.MakeDividerLine($"END VLAN @ {host.Name}"));
            }

            await Task.CompletedTask;
        }
    }
}
