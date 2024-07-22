using System.CommandLine;
using System.Text;
using System.Text.RegularExpressions;
using GNS3aaS.CLI.Services;
using Renci.SshNet;
using tik4net;
using tik4net.Objects;
using tik4net.Objects.User;
using Tiksible.Exceptions;
using Tiksible.Handler;
using Tiksible.Models;
using Tiksible.Services;
using Tiksible.Theater;
using Tiksible.Theater.Playbooks;
using Tiksible.Theater.SshHelpers;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.NodeDeserializers;

namespace Tiksible
{
    internal class Program
    {
        static int Main(string[] args)
        {
            var configStorage = ConfigStorage.CreateOrLoad();

            try
            {
                var rootCommand = new RootCommand("Tiksible - Automation for MikroTik")
                {
                    (new InitHandler(configStorage)).GetCommand(),
                    new BackupHandler(configStorage).GetCommand(),
                    new RunHandler(configStorage).GetCommand(),
                    new DiffHandler(configStorage).GetCommand(),
                    new ApplyHandler(configStorage).GetCommand()
                };

                return rootCommand.InvokeAsync(args).Result;
            }
            catch (UserArgumentErrorException ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
        }
    }
}
