﻿using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GNS3aaS.CLI.Handler;
using GNS3aaS.CLI.Services;
using Tiksible.Models.CfgEntities;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Tiksible.Handler
{
    public class InitHandler : BaseHandler, IHandler
    {
        public InitHandler(ConfigStorage configStorage) : base(configStorage)
        {
        }

        public Command GetCommand()
        {
            var initCommand = new Command("init", "init project");

            AddCredHostsDefaultArgument(initCommand, out var credOption, out var hostsOption, out var debugOption);

            initCommand.SetHandler(async (credentialsFileName, hostsFileName, debug) =>
            {
                await handle(credentialsFileName, hostsFileName, debug);
            }, credOption, hostsOption, debugOption);

            return initCommand;
        }

        private async Task handle(string credentialsFileName, string hostsFileName, bool debug)
        {
            var hosts = new HostsCfgEntity();
            hosts.Hosts.Add(new HostCfgEntity()
            {
                Name = "host1",
                Address = "dns or ip",
                CredentialsAlias = "default",
                Params = new Dictionary<string, string>()
                {
                    {"custom-param1", "1234"}
                }
            });
            WriteYamlFileIfNotExists(hostsFileName, hosts);

            var creds = new CredentialsCfgEntity();
            creds.Credentials.Add(new CredentialCfgEntity()
            {
                Name = "default",
                Password = "1234",
                PrivateKey = "privateKey"
            });
            WriteYamlFileIfNotExists(credentialsFileName, creds);
        }

        private void WriteYamlFileIfNotExists(string filename, object obj)
        {
            var yamlSerializer = new SerializerBuilder()
                .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
                
                .Build();

            if (!File.Exists(filename))
            {
                using (TextWriter writer = new StreamWriter(filename))
                {
                    yamlSerializer.Serialize(writer, obj);
                }
                Console.WriteLine($"Created {filename}");
            }
            else
            {
                Console.WriteLine($"{filename} Already exists. skipping.");
            }
        }
    }
}
