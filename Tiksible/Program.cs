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

            var handlers = new BaseHandler[]
            {
                new InitHandler(configStorage),
                new BackupHandler(configStorage),
                new RunHandler(configStorage),
                new RunsHandler(configStorage),
                new DiffHandler(configStorage),
                new ApplyHandler(configStorage),
                new InstallSshPubKeyHandler(configStorage),
                new UpdateHandler(configStorage),
                new TestHandler(configStorage),
                new VlanHandler(configStorage),
                new VersionHandler(configStorage),
            };

            try
            {
                var rootCommand = new RootCommand("Tiksible - Automation for MikroTik");
                foreach (var h in handlers)
                    rootCommand.Add(((IHandler)h).GetCommand());

                return rootCommand.InvokeAsync(args).Result;
            }
            catch (UserArgumentErrorException ex)
            {
                Console.WriteLine(ex.Message);
                return 254;
            }
            finally
            {
                foreach (var h in handlers)
                    h.Dispose();
            }
        }
    }
}
