using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible.Theater
{
    public interface IWorkflow
    {

        public void Run();

        public bool IsSuccessfull { get; }

        public string Message { get; }

        public bool IsFinished { get; }

    }
}
