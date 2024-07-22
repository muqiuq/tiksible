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
using Tiksible.Theater;

namespace Tiksible.Handler
{
    public class InstallSshPubKeyHandler : BaseHandler, IHandler
    {
        public InstallSshPubKeyHandler(ConfigStorage configStorage) : base(configStorage)
        {

        }


        public Command GetCommand()
        {
            var cmd = new Command("pubkey", "install public key for a user");

            var pubKeyFileNameArg = new Argument<string>("pubkey", "path to the public key file");
            var usernameArg = new Argument<string>("username", "username for authentication");

            cmd.AddArgument(pubKeyFileNameArg);
            cmd.AddArgument(usernameArg);

            AddCredHostsDefaultArgument(cmd, out var credOption, out var hostsOption, out var debugOption, out var filterOption);

            cmd.SetHandler(async (string pubKeyFileName, string username, string credentialsFileName, string hostsFileName, bool debug, string hostFilter) =>
            {
                await HandleApply(pubKeyFileName, username, credentialsFileName, hostsFileName, debug, hostFilter);
            }, pubKeyFileNameArg, usernameArg, credOption, hostsOption, debugOption, filterOption);

            return cmd;
        }


        private async Task HandleApply(string pubKeyFileName, string username, string credentialsFileName, string hostsFileName, bool debug, string hostFilter)
        {
            LoadHostsAndCredentials(credentialsFileName, hostsFileName, hostFilter);
            CheckCredentialsForAllHosts();

            if (!File.Exists(pubKeyFileName))
            {
                throw new UserArgumentErrorException($"public file not found @ {Path.GetFullPath(pubKeyFileName)}");
            }

            var publicKeyFile = await File.ReadAllTextAsync(pubKeyFileName);

            var publicKeyParts = publicKeyFile.Trim().Split(" ");
            if (publicKeyParts.Length != 3)
            {
                throw new UserArgumentErrorException("Invalid public key (maybe missing user at the end?)");
            }

            var publicKeyName = publicKeyParts.Last();

            foreach (var host in Hosts.Hosts)
            {
                Console.WriteLine(ConsoleOutputHelper.MakeDeviderLine($"PUBKEY @ {host.Name}"));

                var conInfo = host.GetCredentials(Credentials)!.GetSshConnectionInfo(host.Address);

                var playbookRunnerCheckKey = new PlaybookRunner(conInfo,
                    new RunSingleCmdPlaybook($":put [:len [/user ssh-keys find key-owner=\"{publicKeyName}\"]]"));

                playbookRunnerCheckKey.Run();

                if (!playbookRunnerCheckKey.IsSuccess())
                {
                    throw new TiksibleCmdExecutionException("Could not check if key is already installed");
                }

                if (playbookRunnerCheckKey.Artifacts[RunSingleCmdPlaybook.OutputArtifactName].Trim() != "0")
                {
                    Console.WriteLine("SSH key already present: Skipping");
                }
                else
                {
                    var playbookRunner = new PlaybookRunner(conInfo, new InstallSshPubKeyPlaybook(username));

                    playbookRunner.Files.Add(InstallSshPubKeyPlaybook.FileName, Encoding.UTF8.GetBytes(publicKeyFile));

                    playbookRunner.Run();

                    ConsoleOutputHelper.PrintStatusLine($"Install SSH key for {username} @ {host.Name}", playbookRunner.IsSuccess());

                    Console.WriteLine(ConsoleOutputHelper.MakeDeviderLine($"OUTPUT", '-'));
                    Console.WriteLine(playbookRunner.Artifacts[RunRscScriptPlaybook.Output]);
                }
                Console.WriteLine(ConsoleOutputHelper.MakeDeviderLine($"END PUBKEY @ {host.Name}"));
            }
        }
    }
}
