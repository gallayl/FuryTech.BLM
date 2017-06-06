using System.Threading.Tasks;
using BLM.NetStandard.Interfaces;
using BLM.NetStandard.Interfaces.Authorize;

namespace BLM.NetStandard
{
    public abstract class AuthorizeCreate<T> : IAuthorizeCreate<T>
    {
        public abstract Task<AuthorizationResult> CanCreateAsync(T entity, IContextInfo ctx);
    }
}