using System.Collections.Generic;

namespace Tiksible.Theater.Playbooks
{
    public class FetchBridgePlaybook : IPlaybook
    {
        public const string ArtifactName = "bridge";

        private readonly List<FullExecutionOrder> orderList;

        public FetchBridgePlaybook()
        {
            orderList = new List<FullExecutionOrder>
            {
                ExecutionOrderHelper.CmdWithOutput("/interface bridge export show-sensitive", ArtifactName)
            };
        }

        public List<FullExecutionOrder> GetExecutionOrders() => orderList;
    }
}
