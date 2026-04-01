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
                var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
                Console.WriteLine($"tiksible {version}");
                Console.WriteLine("Repository: https://github.com/muqiuq/tiksible");
                Console.WriteLine("Author:     Philipp M. Albrecht <philipp@uisa.ch>");
            });
            return cmd;
        }
    }
}
