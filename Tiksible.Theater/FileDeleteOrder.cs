using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible.Theater
{
    public class FileDeleteOrder : IExecutionOrder
    {
        public bool Executed { get; set; }

        public FileDeleteOrder(string filename)
        {
            Filename = filename;
        }

        public string Filename { get; }

        public string Command()
        {
            return Filename;
        }

        public ExecutionOrderType GetOrderType()
        {
            return ExecutionOrderType.DeleteFile;
        }

        public bool Verify(int? exitStatus, string output)
        {
            return true;
        }

        public bool HasArtifact()
        {
            return false;
        }

        public string GetArtifactName()
        {
            return Filename;
        }
    }
}
