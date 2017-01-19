using System.Threading.Tasks;
using BLM.Interfaces.Authorize;

namespace BLM
{
    public abstract class AuthorizeCreate<T> : IAuthorizeCreate<T>
    {
        public abstract Task<AuthorizationResult> CanCreateAsync(T entity, IContextInfo ctx);
    }
}