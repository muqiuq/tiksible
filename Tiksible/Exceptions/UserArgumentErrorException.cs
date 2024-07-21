using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Tiksible.Exceptions
{
    public class UserArgumentErrorException : Exception
    {
        public UserArgumentErrorException()
        {
        }

        protected UserArgumentErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public UserArgumentErrorException(string? message) : base(message)
        {
        }

        public UserArgumentErrorException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
