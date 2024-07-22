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

        public const string FileName = "tmp-tiksible-script.rsc";
        public const string Output = "output";

        public RunRscScriptPlaybook()
        {
            orderList.Add(ExecutionOrderHelper.UploadFile(FileName));
            orderList.Add(ExecutionOrderHelper.Sleep(1000));
            orderList.Add(ExecutionOrderHelper.CmdWithOutput($"/import file-name={FileName}", Output));
            orderList.Add(ExecutionOrderHelper.DeleteFileSFTP(FileName));
        }

        public List<FullExecutionOrder> GetExecutionOrders()
        {
            return orderList;
        }
    }
}
