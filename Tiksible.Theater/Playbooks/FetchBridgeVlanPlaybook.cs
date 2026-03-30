using System.Collections.Generic;

namespace Tiksible.Theater.Playbooks
{
    public class FetchBridgeVlanPlaybook : IPlaybook
    {
        public const string ArtifactName = "bridgeVlan";

        private readonly List<FullExecutionOrder> orderList;

        public FetchBridgeVlanPlaybook()
        {
            orderList = new List<FullExecutionOrder>
            {
                ExecutionOrderHelper.CmdWithOutput("/interface bridge vlan export show-sensitive", ArtifactName)
            };
        }

        public List<FullExecutionOrder> GetExecutionOrders() => orderList;
    }
}
