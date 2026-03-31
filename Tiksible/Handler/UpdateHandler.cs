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
using System.Text.RegularExpressions;
using Renci.SshNet.Common;
using Tiksible.Theater.SshHelpers;

namespace Tiksible.Handler
{
    public class UpdateHandler : BaseHandler, IHandler
    {
        public UpdateHandler(ConfigStorage configStorage) : base(configStorage)
        {

        }


        public Command GetCommand()
        {
            var cmd = new Command("update", "update packages on host");

            var writeOption = new Option<bool>(
                "--write",
                "apply update if available"
            );
            cmd.AddOption(writeOption);

            var retryCountOption = new Option<int>(
                "--retry-count",
                () => 40,
                "maximum number of connection attempts after reboot during update"
            );
            cmd.AddOption(retryCountOption);

            var retryIntervalOption = new Option<int>(
                "--retry-interval",
                () => 2000,
                "interval in milliseconds between connection retries"
            );
            cmd.AddOption(retryIntervalOption);

            AddCredHostsDefaultArgument(cmd, out var credOption, out var hostsOption, out var debugOption, out var filterOption);

            cmd.SetHandler(async (bool write, int retryCount, int retryInterval, string credentialsFileName, string hostsFileName, bool debug, string hostFilter) =>
            {
                await HandleApply(write, retryCount, retryInterval, credentialsFileName, hostsFileName, debug, hostFilter);
            }, writeOption, retryCountOption, retryIntervalOption, credOption, hostsOption, debugOption, filterOption);

            return cmd;
        }

        private bool GetInstalledAndLatestVersion(ISshConnectionInfo conInfo, out string installedVersion, out string? latestVersion)
        {
            installedVersion = null;
            latestVersion = null;
            var playbookRunnerCheckVersion = new PlaybookRunner(conInfo,
                new RunSingleCmdPlaybook($"/system/package/update/check-for-updates"), Pool);

            playbookRunnerCheckVersion.Run();

            if (!playbookRunnerCheckVersion.IsSuccess())
            {
                throw new TiksibleCmdExecutionException("Could not check version");
            }

            var outputCmd = playbookRunnerCheckVersion.Artifacts[RunSingleCmdPlaybook.OutputArtifactName].Trim();

            Match matchInstalledVersion = Regex.Match(outputCmd, @"installed-version:\s*(\d+\.\d+(?:\.\d+)?)");
            Match matchLatestVersion = Regex.Match(outputCmd, @"latest-version:\s*(\d+\.\d+(?:\.\d+)?)");

            if (!matchInstalledVersion.Success)
            {
                return false;
            }
            installedVersion = matchInstalledVersion.Groups[1].Value.Trim();
            if (matchLatestVersion.Success)
            {
                latestVersion = matchLatestVersion.Groups[1].Value.Trim();
            }

            return true;
        }

        private async Task HandleApply(bool write, int retryCount, int retryInterval, string credentialsFileName, string hostsFileName, bool debug, string hostFilter)
        {
            LoadHostsAndCredentials(credentialsFileName, hostsFileName, hostFilter);
            CheckCredentialsForAllHosts();

            foreach (var host in Hosts.Hosts)
            {
                Console.WriteLine(ConsoleOutputHelper.MakeDeviderLine($"UPDATE @ {host.Name}"));

                var conInfo = host.GetCredentials(Credentials)!.GetSshConnectionInfo(host);

                if (!GetInstalledAndLatestVersion(conInfo, out var installedVersion, out var latestVersion))
                {
                    Console.WriteLine("Could not retrieve installed-version");
                }else{

                    Console.WriteLine($"Installed Version: {installedVersion}");

                    if (latestVersion == null)
                    {
                        Console.WriteLine("Could not retrieve latest-version. Is the host available to reach the mikrotik update servers?");
                    }
                    else
                    {
                        Console.WriteLine($"Latest Version: {latestVersion}");

                        if (latestVersion != installedVersion && write)
                        {
                            var playbookRunner = new PlaybookRunner(conInfo, new RunSingleCmdPlaybook($"/system/package/update/install"), Pool);
                            try
                            {
                                playbookRunner.Run();
                            }
                            catch (SshConnectionException ex)
                            {
                                // The Router reboots after success download and installation and aborts the connection. 
                            }

                            Console.WriteLine($"Triggered Install Update @ {host.Name}");

                            bool connectionTestSuccess = false;
                            bool updateSuccess = false;

                            for (int a = 0; a < retryCount; a++)
                            {
                                Thread.Sleep(retryInterval);
                                if (SshConnectionTestService.TestConnection(conInfo))
                                {
                                    connectionTestSuccess = true;
                                    break;
                                }
                                Console.Write(".");
                                await Console.Out.FlushAsync();
                            }

                            if (connectionTestSuccess)
                            {
                                if(GetInstalledAndLatestVersion(conInfo, out var installedVersionFinal, out var latestVersionFinal))
                                {
                                    Console.WriteLine($"Version after update: {installedVersionFinal}");
                                    updateSuccess = installedVersionFinal == latestVersion;
                                }
                            }

                            ConsoleOutputHelper.PrintStatusLine($"Update installed @ {host.Name}", updateSuccess);
                        }
                        else
                        {
                            ConsoleOutputHelper.PrintStatusLine($"Newest version @ {host.Name}", true);
                        }
                    }
                }
                
                Console.WriteLine(ConsoleOutputHelper.MakeDeviderLine($"END UPDATE @ {host.Name}"));
            }
        }
    }
}
