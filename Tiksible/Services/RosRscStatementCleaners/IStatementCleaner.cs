using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiksible.Models;

namespace Tiksible.Services.RosRscStatementCleaners
{
    public interface IStatementCleaner
    {

        public bool Match(string path);

        public void Clean(RosStatement statement);

    }
}
