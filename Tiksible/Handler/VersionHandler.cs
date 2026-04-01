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
                var asm = Assembly.GetExecutingAssembly();
                var version = asm.GetName().Version?.ToString() ?? "unknown";
                var metadata = asm.GetCustomAttributes<AssemblyMetadataAttribute>();
                var repoUrl = metadata.FirstOrDefault(a => a.Key == "RepositoryUrl")?.Value ?? "";
                var authors = metadata.FirstOrDefault(a => a.Key == "Authors")?.Value ?? "";
                Console.WriteLine($"tiksible {version}");
                Console.WriteLine($"Repository: {repoUrl}");
                Console.WriteLine($"Author:     {authors}");
            });
            return cmd;
        }
    }
}
