using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible.Models
{
    public class RosStatementList : List<RosStatement>
    {

        public RosComparisonResult Compare(RosStatementList other)
        {
            RosComparisonResult result = new RosComparisonResult();
            result.MissingStatemenetsOther = getMissingStatements(this, other);
            result.MissingStatemenetsOwn = getMissingStatements(other, this);
            return result;
        }

        private static RosStatementList getMissingStatements(RosStatementList left, RosStatementList right)
        {
            RosStatementList statements = new RosStatementList();

            foreach (var l in left)
            {
                if(!right.Contains(l))
                    statements.Add(l);
            }

            return statements;
        }

        public string[] Export()
        {
            return this.Select(x => x.Export()).ToArray();
        }
    }
}
