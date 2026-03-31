using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible.Theater.Playbooks
{
    public class RunRscScriptPlaybook : IPlaybook
    {
        List<FullExecutionOrder> orderList = new List<FullExecutionOrder>();

        public const string DefaultFileName = "tmp-tiksible-script.rsc";
        public const int DefaultSleepMs = 1000;
        public const string Output = "output";

        public string FileName { get; }

        public RunRscScriptPlaybook() : this(DefaultFileName, DefaultSleepMs)
        {
        }

        public RunRscScriptPlaybook(string fileName, int sleepMs = DefaultSleepMs)
        {
            FileName = fileName;
            orderList.Add(ExecutionOrderHelper.UploadFile(fileName));
            orderList.Add(ExecutionOrderHelper.Sleep(sleepMs));
            orderList.Add(ExecutionOrderHelper.CmdWithOutput($"/import file-name={fileName}", Output));
            orderList.Add(ExecutionOrderHelper.DeleteFileSFTP(fileName));
        }

        public List<FullExecutionOrder> GetExecutionOrders()
        {
            return orderList;
        }
    }
}
