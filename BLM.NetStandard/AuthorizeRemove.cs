using System.Threading.Tasks;
using BLM.NetStandard.Interfaces;
using BLM.NetStandard.Interfaces.Authorize;

namespace BLM.NetStandard
{
    public abstract class AuthorizeRemove<T> : IAuthorizeRemove<T>
    {
        public abstract Task<AuthorizationResult> CanRemoveAsync(T entity, IContextInfo ctx);
    }
}