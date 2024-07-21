using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible.Theater
{
    internal class CommandExectionOrder : IExecutionOrder
    {
        private readonly string command;
        private readonly Func<string, bool> verifyOutputFunc;
        private readonly bool verifyExitStatus;
        private readonly string? artifactName;

        public CommandExectionOrder(string command, Func<string, bool> verifyOutputFunc = null, bool verifyExitStatus = true, string? artifactName = null)
        {
            this.command = command;
            this.verifyOutputFunc = verifyOutputFunc;
            this.verifyExitStatus = verifyExitStatus;
            this.artifactName = artifactName;
        }

        public bool Executed { get; set; }

        public string Command()
        {
            return this.command;
        }

        public string GetArtifactName()
        {
            return artifactName!;
        }

        public ExecutionOrderType GetOrderType()
        {
            return ExecutionOrderType.Command;
        }

        public bool HasArtifact()
        {
            return artifactName != null;
        }


        public bool Verify(int? exitStatus, string output)
        {
            if (verifyExitStatus && exitStatus != 0) return false;
            if (verifyOutputFunc == null) return true;
            return verifyOutputFunc.Invoke(output);
        }

    }
}
