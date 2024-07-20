using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible
{
    public class Helper
    {
        public static Assembly GetTikAssembly()
        {
            return typeof(tik4net.Objects.Ip.IpAddress).Assembly;
        }

    }
}
