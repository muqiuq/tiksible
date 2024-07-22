using System.CommandLine;
using Tiksible.Exceptions;
using Tiksible.Handler;
using Tiksible.Services;

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
                    new ApplyHandler(configStorage).GetCommand(),
                    new InstallSshPubKeyHandler(configStorage).GetCommand(),
                    new UpdateHandler(configStorage).GetCommand(),
                    new TestHandler(configStorage).GetCommand()
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
