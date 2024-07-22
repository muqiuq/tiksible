using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using GNS3aaS.Theater;

namespace Tiksible.Theater
{
    public static class ExecutionOrderHelper
    {
        public static Func<string, bool> CompareStringFunc(string shouldBe)
        {
            return (outputResult) =>
            {
                return outputResult.Trim() == shouldBe.Trim();
            };
        }

        public static Func<string, bool> CompareStringListFunc(params string[] canBe)
        {
            return (outputResult) =>
            {
                return canBe.Contains(outputResult.Trim());
            };
        }

        public static FullExecutionOrder UploadFile(string fileName)
        {
            return new FullExecutionOrder()
                .AddOrder(new FileUploadOrder(fileName));
        }

        public static FullExecutionOrder DownloadFile(string fileName)
        {
            return new FullExecutionOrder()
                .AddPreCheck(new CommandExectionOrder($":if ([:len [/file find name=\"tmp1.rsc\"]] > 0) do={{:put \"1\"}} else={{:put \"0\"}}", CompareStringFunc("1")))
                .AddOrder(new FileDownloadOrder(fileName));
                
        }

        internal static FullExecutionOrder Export(string artifactName)
        {
            return new FullExecutionOrder()
                .AddOrder(new CommandExectionOrder($"/export show-sensitive", artifactName: artifactName));
        }

        internal static FullExecutionOrder Cmd(string cmd)
        {
            return new FullExecutionOrder()
                .AddOrder(new CommandExectionOrder(cmd));
        }

        internal static FullExecutionOrder DeleteFile(string path)
        {
            return new FullExecutionOrder()
                .AddOrder(new CommandExectionOrder($"/file/remove {path}"))
                .AddPostCheck(new CommandExectionOrder(
                    $":if ([:len [/file find name=\"{path}\"]] > 0) do={{:put \"1\"}} else={{:put \"0\"}}",
                    CompareStringFunc("0")));
        }

        internal static FullExecutionOrder DeleteFileSFTP(string path)
        {
            return new FullExecutionOrder()
                .AddOrder(new FileDeleteOrder(path));
        }

        internal static FullExecutionOrder CmdWithOutput(string cmd, string artifactName)
        {
            return new FullExecutionOrder()
                .AddOrder(new CommandExectionOrder(cmd, artifactName: artifactName));
        }


        internal static FullExecutionOrder Sleep(int v)
        {
            return new FullExecutionOrder()
                .AddOrder(new SleepExecutionOrder(v));

        }

    }
}
