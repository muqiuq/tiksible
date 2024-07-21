using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible.Theater
{
    public interface IPlaybook
    {

        public List<FullExecutionOrder> GetExecutionOrders();

    }
}
