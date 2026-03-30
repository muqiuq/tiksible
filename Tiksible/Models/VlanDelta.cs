using System.Collections.Generic;
using System.Linq;

namespace Tiksible.Models
{
    public class VlanDelta
    {
        public List<string> BridgeAdd { get; } = new();
        public List<string> BridgeSets { get; } = new();
        public List<string> BridgeVlanAdds { get; } = new();
        public List<string> BridgeVlanSets { get; } = new();
        public List<string> BridgePortAdds { get; } = new();
        public List<string> BridgePortSets { get; } = new();
        public List<string> BridgePortRemoves { get; } = new();

        public bool HasChanges =>
            BridgeAdd.Count > 0 || BridgeSets.Count > 0 ||
            BridgeVlanAdds.Count > 0 || BridgeVlanSets.Count > 0 ||
            BridgePortAdds.Count > 0 || BridgePortSets.Count > 0 ||
            BridgePortRemoves.Count > 0;

        public IEnumerable<string> AllCommands() =>
            BridgeAdd
                .Concat(BridgeSets)
                .Concat(BridgeVlanAdds)
                .Concat(BridgeVlanSets)
                .Concat(BridgePortAdds)
                .Concat(BridgePortSets)
                .Concat(BridgePortRemoves);
    }
}
