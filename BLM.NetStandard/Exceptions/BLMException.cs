using System;

namespace BLM.NetStandard.Exceptions
{
    public class BLMException : Exception
    {
        public BLMException(string message = null, Exception innerException = null) : base(message, innerException)
        {
        }
    }
}
