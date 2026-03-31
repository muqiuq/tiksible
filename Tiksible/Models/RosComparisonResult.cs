using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible.Models
{
    public class RosComparisonResult
    {

        public RosStatementList MissingStatementsOwn { get; set; } = new RosStatementList();

        public RosStatementList MissingStatementsOther { get; set; } = new RosStatementList();

    }
}
