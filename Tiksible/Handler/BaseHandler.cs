using GNS3aaS.CLI.Services;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiksible;
using Tiksible.Exceptions;
using Tiksible.Models.CfgEntities;
using Tiksible.Models.CfgEntities.Extensions;
using Tiksible.Theater;
using Tiksible.Theater.SshHelpers;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GNS3aaS.CLI.Handler
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

        public void AddCredHostsDefaultArgument(Command initCommand, out Option<string> credOption, out Option<string> hostsOption, out Option<bool> debugOption)
        {
            credOption = new Option<string>(new string[]{"-c", "--credentials"},
                () => GlobalConstants.DefaultCredentialsFilename, "path to credentials yaml file");
            hostsOption = new Option<string>(new string[] { "-h", "--hosts" },
                () => GlobalConstants.DefaultHostsFileName, "path to hosts yaml file");

            debugOption = new Option<bool>(new string[] { "-v", "--verbose" },
                () => false, "debug");

            initCommand.AddOption(credOption);
            initCommand.AddOption(hostsOption);
            initCommand.AddOption(debugOption);

        }

        public void LoadHostsAndCredentials(string credentialsFileName, string hostsFileName)
        {
            Hosts = LoadYaml<HostsCfgEntity>(hostsFileName);
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
