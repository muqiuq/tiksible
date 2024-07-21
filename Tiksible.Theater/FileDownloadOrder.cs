using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible.Theater
{
    public class FileDownloadOrder : IExecutionOrder
    {
        public bool Executed { get; set; }

        public FileDownloadOrder(string filename)
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
            return ExecutionOrderType.FileDownload;
        }

        public bool Verify(int? exitStatus, string output)
        {
            return true;
        }

        public bool HasArtifact()
        {
            return true;
        }

        public string GetArtifactName()
        {
            return Filename;
        }
    }
}
