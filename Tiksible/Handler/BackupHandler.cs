﻿using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GNS3aaS.CLI.Handler;
using GNS3aaS.CLI.Services;
using Tiksible.Models.CfgEntities.Extensions;
using Tiksible.Theater;
using Tiksible.Theater.Playbooks;

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

            AddCredHostsDefaultArgument(backupCommand, out var credOption, out var hostsOption, out var debugOption);

            backupCommand.SetHandler(async (outputFolder, credentialsFileName, hostsFileName, debug) =>
            {
                await HandleBackup(outputFolder, credentialsFileName, hostsFileName, debug);
            }, outputFolderOption, credOption, hostsOption, debugOption);

            return backupCommand;
        }

        private async Task HandleBackup(string outputFolder, string credentialsFileName, string hostsFileName, bool debug)
        {
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
                Console.WriteLine($"Created output folder {outputFolder}");
            }
            LoadHostsAndCredentials(credentialsFileName, hostsFileName);
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
                
                var conInfo = host.GetCredentials(Credentials)!.GetSshConnectionInfo(host.Address);

                var backupPlaybook = RunPlaybook(conInfo, new BackupPlaybook());

                await File.WriteAllBytesAsync(Path.Combine(finalOutputPath, $"{host.Name}.backup"), backupPlaybook.Files[BackupPlaybook.TmpBackupFileName]);

                var exportPlaybook = RunPlaybook(conInfo, new ExportPlaybook());

                await File.WriteAllTextAsync(Path.Combine(finalOutputPath, $"{host.Name}.rsc"), exportPlaybook.Artifacts["config"]);

                Console.WriteLine($"Backup completed for {host.Name}");
            }
            #endregion
        }
    }
}