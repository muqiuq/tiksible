using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible.Exceptions
{
    public class TiksibleCmdExecutionException : Exception
    {
        public TiksibleCmdExecutionException()
        {
        }

        protected TiksibleCmdExecutionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public TiksibleCmdExecutionException(string? message) : base(message)
        {
        }

        public TiksibleCmdExecutionException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
