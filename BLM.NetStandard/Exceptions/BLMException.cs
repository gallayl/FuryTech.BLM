using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLM.NetStandard.Exceptions
{
    public class BLMException : Exception
    {
        public BLMException(string message = null, Exception innerException = null) : base(message, innerException)
        {
        }
    }
}
