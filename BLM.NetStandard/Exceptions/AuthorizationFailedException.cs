namespace BLM.NetStandard.Exceptions
{
    public class AuthorizationFailedException : BLMException
    {
        public AuthorizationResult AuthorizationResult { get; }

        public AuthorizationFailedException(AuthorizationResult authResult)
        {
            AuthorizationResult = authResult;
        }
    }
}
