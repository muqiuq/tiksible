﻿using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tiksible;
using Tiksible.Exceptions;
using Tiksible.Models.CfgEntities;
using Tiksible.Models.CfgEntities.Extensions;
using Tiksible.Services;
using Tiksible.Theater;
using Tiksible.Theater.SshHelpers;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Tiksible.Handler
{
    public abstract class BaseHandler
    {
        protected readonly ConfigStorage configStorage;

        protected CredentialsCfgEntity Credentials;

        protected HostsCfgEntity Hosts;

        public BaseHandler(ConfigStorage configStorage)
        {
            this.configStorage = configStorage;
        }

        public void AddOutputDefaultOptions(Command initCommand, out Option<string> outputOption,
            out Option<bool> overwriteOutputOption)
        {
            outputOption = new Option<string>(
                ["-o", "--output"],
                description: "Path to save the command output"
            );
            
            overwriteOutputOption = new Option<bool>(["-w", "--overwrite"],
                () => false, "force overwrite output file");
            
            initCommand.AddOption(outputOption);
            initCommand.AddOption(overwriteOutputOption);
        }
        
        public void AddCredHostsDefaultArgument(Command initCommand, out Option<string> credOption
            , out Option<string> hostsOption, out Option<bool> debugOption, out Option<string> filterOption )
        {
            var credentialsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), GlobalConstants.DefaultCredentialsFilename);

            credOption = new Option<string>(["-c", "--credentials"],
                () => credentialsPath, "path to credentials yaml file");
            hostsOption = new Option<string>(["-h", "--hosts"],
                () => GlobalConstants.DefaultHostsFileName, "path to hosts yaml file");

            filterOption = new Option<string>(["-f", "--filter"],
                () => "", "filter hosts using regex");

            debugOption = new Option<bool>(["-v", "--verbose"],
                () => false, "debug");
            

            
            initCommand.AddOption(credOption);
            initCommand.AddOption(hostsOption);
            initCommand.AddOption(debugOption);
            initCommand.AddOption(filterOption);
            

        }

        public void LoadHostsAndCredentials(string credentialsFileName, string hostsFileName, string filter)
        {
            Hosts = LoadYaml<HostsCfgEntity>(hostsFileName);
            if (!string.IsNullOrWhiteSpace(filter))
            {
                var hostFilterRegex = new Regex(filter);
                var countBefore = Hosts.Hosts.Count;
                Hosts.Hosts = Hosts.Hosts.Where(h => hostFilterRegex.IsMatch(h.Name)).ToList();
                Console.WriteLine($"filter matched {Hosts.Hosts.Count} of {countBefore} hosts");
            }
            Credentials = LoadYaml<CredentialsCfgEntity>(credentialsFileName);
        }

        private T LoadYaml<T>(string filename)
        {
            if (!File.Exists(filename))
            {
                throw new UserArgumentErrorException($"{filename} does not exists");
            }
            
            var yamlDeseri = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            using (TextReader tr = new StreamReader(filename))
            {
                return yamlDeseri.Deserialize<T>(tr);
            }
        }

        public PlaybookRunner RunPlaybook(ISshConnectionInfo conInfo, IPlaybook playbook, bool hardFail = false) 
        {
            var playbookRunner = new PlaybookRunner(conInfo, playbook);

            playbookRunner.Run();

            if (!playbookRunner.IsSuccess())
            {
                var errMsg = $"Failed to run {playbook.GetType().Name}";
                if (hardFail)
                {
                    throw new UserArgumentErrorException(errMsg);
                }
                else
                {
                    Console.WriteLine(errMsg);
                }
            }

            return playbookRunner;
        }

        public void CheckCredentialsForAllHosts()
        {
            foreach (var host in Hosts.Hosts)
            {
                if (host.GetCredentials(Credentials) == null)
                {
                    throw new UserArgumentErrorException($"No credentials found for {host.Name}");
                }
            }
        }
    }
}
