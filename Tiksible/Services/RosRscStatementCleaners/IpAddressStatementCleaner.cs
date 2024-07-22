using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiksible.Models;

namespace Tiksible.Services.RosRscStatementCleaners
{
    public class IpAddressStatementCleaner : IStatementCleaner
    {
        public bool Match(string path)
        {
            return path == "ip address";
        }

        public void Clean(RosStatement statement)
        {
            if (statement.Properties.ContainsKey("network"))
            {
                statement.Properties.Remove("network");
            }
        }
    }
}
