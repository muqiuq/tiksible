using Tiksible;
using Tiksible.Models;
using Tiksible.Theater;
using Tiksible.Theater.Playbooks;
using Tiksible.Theater.SshHelpers;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DevConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var connectionInfo = new ConnectionInfo("192.168.23.162",
            //    "admin", new PasswordAuthenticationMethod("admin", "admin"));

            var sshConInfo =
                SshConnectionInfoFactory.CreateUserNamePasswordConnectionInfo("192.168.23.162", "admin", "admin");

            var playbookExport = new PlaybookRunner(sshConInfo, new ExportPlaybook());

            playbookExport.Run();

            if (!playbookExport.IsSuccess())
            {
                Console.WriteLine("export failed");
                return;
            }


            File.WriteAllText($"{sshConInfo.HostName}.rsc", playbookExport.Artifacts["config"]);

            var playbookBackup = new PlaybookRunner(sshConInfo, new BackupPlaybook());

            playbookBackup.Run();

            if (!playbookExport.IsSuccess())
            {
                Console.WriteLine("backup failed");
                return;
            }

            File.WriteAllBytes($"{sshConInfo.HostName}.backup", playbookBackup.Files[BackupPlaybook.TmpBackupFileName]);



            //using (var client = new SshClient(connectionInfo))
            //using (var sftpClient = new SftpClient(connectionInfo))
            //{
            //    client.Connect();
            //    var command = client.CreateCommand("/export show-sensitive file=tmp1");
            //    command.Execute();
            //    var inboundStream = new MemoryStream();
            //    sftpClient.DownloadFile("/tmp1", new MemoryStream());
            //    Console.WriteLine(Encoding.UTF8.GetString(inboundStream.ToArray()));
            //}



            //using (var conn = ConnectionFactory.CreateConnection(TikConnectionType.Api))
            //{
            //    conn.Open("192.168.23.162", "admin", "admin");
            //    var cmdExport = conn.CreateCommand("/export");
            //    cmdExport.AddParameter("file", "tmp1");
            //    var result = cmdExport.ExecuteList();
            //    Console.WriteLine(result);

            //}
        }

        static void YamlTest()
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            var exampleConfig = File.ReadAllText("example.yaml");

            var configTasks = deserializer.Deserialize<List<ConfigTask>>(exampleConfig);
            //var configTask = deserializer.Deserialize(exampleConfig);

            foreach (var configTask in configTasks)
            {
                var tikEntries = TikEntityHelper.GetTikEntities(configTask);
                Console.WriteLine(tikEntries);
            }
        }
    }
}
