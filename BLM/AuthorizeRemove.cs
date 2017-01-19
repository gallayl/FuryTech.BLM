using System.Threading.Tasks;
using BLM.Interfaces;
using BLM.Interfaces.Authorize;

namespace BLM
{
    public abstract class AuthorizeRemove<T> : IAuthorizeRemove<T>
    {
        public abstract Task<AuthorizationResult> CanRemoveAsync(T entity, IContextInfo ctx);
    }
}