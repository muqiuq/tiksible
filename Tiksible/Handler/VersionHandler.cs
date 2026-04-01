using System.CommandLine;
using System.Reflection;
using Tiksible.Services;

namespace Tiksible.Handler
{
    public class VersionHandler : BaseHandler, IHandler
    {
        public VersionHandler(ConfigStorage configStorage) : base(configStorage) { }

        public Command GetCommand()
        {
            var cmd = new Command("version", "show tiksible version");
            cmd.SetHandler(() =>
            {
                Console.WriteLine(Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown");
            });
            return cmd;
        }
    }
}
