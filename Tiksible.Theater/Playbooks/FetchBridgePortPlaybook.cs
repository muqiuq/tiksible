using System.Collections.Generic;

namespace Tiksible.Theater.Playbooks
{
    public class FetchBridgePortPlaybook : IPlaybook
    {
        public const string ArtifactName = "bridgePort";

        private readonly List<FullExecutionOrder> orderList;

        public FetchBridgePortPlaybook()
        {
            orderList = new List<FullExecutionOrder>
            {
                ExecutionOrderHelper.CmdWithOutput("/interface bridge port export show-sensitive", ArtifactName)
            };
        }

        public List<FullExecutionOrder> GetExecutionOrders() => orderList;
    }
}
