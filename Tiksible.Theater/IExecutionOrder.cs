using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible.Theater
{
    public interface IExecutionOrder
    {

        public bool Executed { get; internal set; }

        public ExecutionOrderType GetOrderType();

        public string Command();

        public bool Verify(int? exitStatus, string output);

        public bool HasArtifact();

        public String GetArtifactName();

    }
}
