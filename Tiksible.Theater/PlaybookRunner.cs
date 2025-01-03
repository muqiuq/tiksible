using Microsoft.Extensions.Logging;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiksible.Lib.Logging;
using Tiksible.Theater.SshHelpers;

namespace Tiksible.Theater
{
    public class PlaybookRunner
    {
        private readonly ISshConnectionInfo sshConnectionInfo;
        private readonly IPlaybook playbook;

        private bool? wasSuccessfull = null;

        private static readonly ILogger<PlaybookRunner> logger = StaticLoggerFactory.GetStaticLogger<PlaybookRunner>();

        public Dictionary<string, byte[]> Files = new Dictionary<string, byte[]>();

        public Dictionary<string, string> Artifacts = new Dictionary<string, string>();

        public PlaybookRunner(ISshConnectionInfo sshConnection, IPlaybook playbook)
        {
            this.sshConnectionInfo = sshConnection;
            this.playbook = playbook;
        }

        public void Run()
        {
            var connectionInfo = sshConnectionInfo.GetConnectionInfo();   

            using (var client = new SshClient(connectionInfo))
            using (var sftpClient = new SftpClient(connectionInfo))
            {
                client.Connect();
                if(!sshConnectionInfo.SshOnly)
                {
                    sftpClient.Connect();
                }
                logger.LogDebug($"Executing Playbook {playbook.GetType().Name} on {sshConnectionInfo}");

                foreach (var order in playbook.GetExecutionOrders())
                {
                    
                    if (order.ExecutionCondition != null && !Execute(client, sftpClient, order.ExecutionCondition, "Condition")) continue;

                    wasSuccessfull = false;
                    if (order.PreCheck != null && !Execute(client, sftpClient, order.PreCheck, "PreCheck")) break;
                    if (order.PreCheck != null) order.PreCheck.Executed = true;
                    
                    if (!Execute(client, sftpClient, order.ExecutionOrder, "ExecutionOrder")) break;
                    order.ExecutionOrder.Executed = true;

                    if (order.PostCheck != null && !Execute(client, sftpClient, order.PostCheck, "PostCheck")) break;
                    if (order.PostCheck != null) order.PostCheck.Executed = true;
                    
                    wasSuccessfull = true;
                }
            }
        }

        public bool IsSuccess()
        {
            if (!wasSuccessfull.HasValue) throw new Exception("Not runned yet");
            return wasSuccessfull!.Value;
        }

        private bool Execute(SshClient client, SftpClient sftpClient, IExecutionOrder order, string state)
        {
            if (order.GetOrderType() == ExecutionOrderType.Command) return ExecuteCommand(client, order, state);
            if (order.GetOrderType() == ExecutionOrderType.FileUpload) return ExecuteFileUpload(client, sftpClient, order, state);
            if (order.GetOrderType() == ExecutionOrderType.FileDownload) return ExecuteFileDownload(client, sftpClient, order, state);
            if (order.GetOrderType() == ExecutionOrderType.DeleteFile) return ExecuteFileDelete(client, sftpClient, order, state);
            if (order.GetOrderType() == ExecutionOrderType.Sleep) return ExecuteSleep(client, sftpClient, order, state);
            return false;
        }

        private bool ExecuteSleep(SshClient client, SftpClient sftpClient, IExecutionOrder order, string state)
        {
            int sleepInMillis = 1000;
            if(int.TryParse(order.Command(), out var parsedDuration))
            {
                sleepInMillis = parsedDuration;
            }
            else
            {
                logger.LogWarning($"{state} could not parse duration {order.Command()}. Using default duration of 1000ms.");
            }
            Thread.Sleep(sleepInMillis);
            return true;
        }

        private bool ExecuteCommand(SshClient client, IExecutionOrder order, string state)
        {
            var command = client.CreateCommand(order.Command());
            command.Execute();
            if(order.HasArtifact())
            {
                Artifacts[order.GetArtifactName()] = command.Result;
            }
            logger.LogDebug($"{order.Command()} => ({command.ExitStatus}) {command.Result.Replace("\n", " ")}");
            if (!order.Verify(command.ExitStatus, command.Result))
            {
                logger.LogWarning($"{state} {order.Command()}. Returned ({command.ExitStatus}) {command.Result.Replace("\n", " ")}");
                return false;
            }
            return true;
        }

        private bool ExecuteFileUpload(SshClient client, SftpClient sftpClient, IExecutionOrder order, string state)
        {
            var filePath = order.Command();
            if(!Files.ContainsKey(filePath))
            {
                logger.LogWarning($"File not found in playbook runner context: {filePath}");
                return false;
            }
            var fileStream = new MemoryStream(Files[filePath]);
            sftpClient.UploadFile(fileStream, filePath, canOverride: true);
            return true;
        }

        private bool ExecuteFileDownload(SshClient client, SftpClient sftpClient, IExecutionOrder order, string state)
        {
            var filePath = order.Command();
            var inboundFile = new MemoryStream();
            sftpClient.DownloadFile(filePath, inboundFile);
            Files[filePath] = inboundFile.ToArray();
            return true;
        }

        private bool ExecuteFileDelete(SshClient client, SftpClient sftpClient, IExecutionOrder order, string state)
        {
            var filePath = order.Command();
            sftpClient.DeleteFile(filePath);
            return true;
        }
    }
}
