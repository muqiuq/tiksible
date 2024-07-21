using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible.Theater.Playbooks
{
    public class RunSingleCmdPlaybook : IPlaybook
    {
        List<FullExecutionOrder> orderList = new List<FullExecutionOrder>();

        public const string OutputArtifactName = "output";

        public RunSingleCmdPlaybook(string cmd)
        {
            orderList.Add(ExecutionOrderHelper.CmdWithOutput(cmd, OutputArtifactName));
        }

        public List<FullExecutionOrder> GetExecutionOrders()
        {
            return orderList;
        }
    }
}
