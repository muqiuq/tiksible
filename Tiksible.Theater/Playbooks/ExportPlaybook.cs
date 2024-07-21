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

        public const string ArtifactName = "config";

        public ExportPlaybook()
        {
            orderList.Add(ExecutionOrderHelper.Export(ArtifactName));
        }

        public List<FullExecutionOrder> GetExecutionOrders()
        {
            return orderList;
        }
    }
}
