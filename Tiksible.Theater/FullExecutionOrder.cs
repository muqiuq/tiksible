using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible.Theater
{
    public class FullExecutionOrder
    {
        public IExecutionOrder PreCheck { get; private set; }

        public IExecutionOrder ExecutionCondition { get; private set; }

        public IExecutionOrder PostCheck { get; private set; }

        public IExecutionOrder ExecutionOrder { get; private set; }

        public FullExecutionOrder()
        {
        }

        public FullExecutionOrder AddOrder(IExecutionOrder order)
        {
            ExecutionOrder = order;
            return this;
        }

        public FullExecutionOrder AddCondition(IExecutionOrder order)
        {
            ExecutionCondition = order;
            return this;
        }

        public FullExecutionOrder AddPreCheck(IExecutionOrder preCheck)
        {
            PreCheck = preCheck;
            return this;
        }

        public FullExecutionOrder AddPostCheck(IExecutionOrder postCheck)
        { 
            PostCheck = postCheck;
            return this;
        }
    }
}
