using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiksible.Theater;

namespace GNS3aaS.Theater
{
    public class SleepExecutionOrder : IExecutionOrder
    {
        public SleepExecutionOrder(int durationInMilliseconds)
        {
            DurationInMilliseconds = durationInMilliseconds;
        }

        public int DurationInMilliseconds { get; }

        bool IExecutionOrder.Executed { get; set; }

        public string Command()
        {
            return DurationInMilliseconds.ToString();
        }

        public string GetArtifactName()
        {
            return "";
        }

        public ExecutionOrderType GetOrderType()
        {
            return ExecutionOrderType.Sleep;
        }

        public bool HasArtifact()
        {
            return false;
        }

        public bool Verify(int? exitStatus, string output)
        {
            return true;
        }
    }
}
