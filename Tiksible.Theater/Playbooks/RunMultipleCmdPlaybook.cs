using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible.Theater.Playbooks
{
    public class RunMultipleCmdPlaybook : IPlaybook
    {
        List<FullExecutionOrder> orderList = new List<FullExecutionOrder>();

        public RunMultipleCmdPlaybook(string[] cmds)
        {
            foreach (var cmd in cmds)
            {
                orderList.Add(ExecutionOrderHelper.CmdWithOutput(cmd, cmd));
            }
        }

        public List<FullExecutionOrder> GetExecutionOrders()
        {
            return orderList;
        }
    }
}