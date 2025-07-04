﻿using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiksible.Helpers;
using Tiksible.Models.CfgEntities;
using Tiksible.Models.CfgEntities.Extensions;
using Tiksible.Services;
using Tiksible.Theater;
using Tiksible.Theater.Playbooks;
using Tiksible.Theater.SshHelpers;

namespace Tiksible.Handler
{
    public class BackupHandler : BaseHandler, IHandler
    {
        public BackupHandler(ConfigStorage configStorage) : base(configStorage)
        {
        }

        public Command GetCommand()
        {
            var backupCommand = new Command("backup", "Backup project");

            var outputFolderOption = new Option<string>(new string[]{"-o", "--output"},
                () => "backups", "path to output folder");
            backupCommand.AddOption(outputFolderOption);

            AddCredHostsDefaultArgument(backupCommand, out var credOption, out var hostsOption, out var debugOption, out var hostFilterOption);

            backupCommand.SetHandler(HandleBackup, outputFolderOption, credOption, hostsOption, debugOption,
                hostFilterOption);
            
            return backupCommand;
        }

        private async Task HandleBackup(string outputFolder, string credentialsFileName, string hostsFileName, bool debug, string hostFilter)
        {
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
                Console.WriteLine($"Created output folder {outputFolder}");
            }
            LoadHostsAndCredentials(credentialsFileName, hostsFileName, hostFilter);
            var timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");

            CheckCredentialsForAllHosts();

            #region Execution
            foreach (var host in Hosts.Hosts)
            {
                var finalOutputPath = Path.Combine(outputFolder, timeStamp);
                if (!Directory.Exists(finalOutputPath))
                {
                    Directory.CreateDirectory(finalOutputPath);
                }
                
                var conInfo = host.GetCredentials(Credentials)!.GetSshConnectionInfo(host);

                Console.WriteLine($"Backing up {host.Name}...");

                var backupSuccess = false;

                try
                {
                    await RunBackup(host, conInfo, finalOutputPath);
                    backupSuccess = true;
                }
                catch (Exception ex)
                {
                    if (debug)
                    {
                        Console.WriteLine(ex);
                    }
                    else
                    {
                        Console.WriteLine(ex.Message);
                    }
                    backupSuccess = false;
                }

                ConsoleOutputHelper.PrintStatusLine($"Backup {host.Name}", backupSuccess);
            }
            #endregion
        }

        private async Task RunBackup(HostCfgEntity host, ISshConnectionInfo conInfo, string finalOutputPath)
        {
            var backupPlaybook = RunPlaybook(conInfo, new BackupPlaybook());

            await File.WriteAllBytesAsync(Path.Combine(finalOutputPath, $"{host.Name}.backup"), backupPlaybook.Files[BackupPlaybook.TmpBackupFileName]);

            var exportPlaybook = RunPlaybook(conInfo, new ExportPlaybook());

            await File.WriteAllTextAsync(Path.Combine(finalOutputPath, $"{host.Name}.rsc"), exportPlaybook.Artifacts["config"]);
        }
    }
}
