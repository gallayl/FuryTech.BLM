using System.Collections.Generic;

namespace BLM.NetStandard
{
    public class AuthorizationResult
    {
        private AuthorizationResult() { }

        public static AuthorizationResult Success(string message = null)
        {
            return new AuthorizationResult()
            {
                HasSucceed = true,
                Message = message
            };
        }

        public static AuthorizationResult Fail<T>(string message, T entity)
        {
            return new AuthorizationResult()
            {
                HasSucceed = false,
                Message = message
            };
        }

        public string Message { get; private set; }

        public bool HasSucceed { get; private set; }

        public List<AuthorizationResult> InnerResult = new List<AuthorizationResult>();
    }
}
