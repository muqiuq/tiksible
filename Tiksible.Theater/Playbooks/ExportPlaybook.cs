using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible.Theater.Playbooks
{
    public class ExportPlaybook : IPlaybook
    {
        List<FullExecutionOrder> orderList = new List<FullExecutionOrder>();

        public ExportPlaybook()
        {
            orderList.Add(ExecutionOrderHelper.Export("config"));
        }

        public List<FullExecutionOrder> GetExecutionOrders()
        {
            return orderList;
        }
    }
}
