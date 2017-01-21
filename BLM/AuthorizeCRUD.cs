using System.Threading.Tasks;
using BLM.Interfaces;
using BLM.Interfaces.Authorize;

namespace BLM
{
    public abstract class AuthorizeCRUD<T> : AuthorizeCollection<T>, IAuthorizeCreate<T>, IAuthorizeModify<T>, IAuthorizeRemove<T>
    {
        public abstract Task<AuthorizationResult> CanCreateAsync(T entity, IContextInfo ctx);
        public abstract Task<AuthorizationResult> CanModifyAsync(T originalEntity, T modifiedEntity, IContextInfo ctx);

        public abstract Task<AuthorizationResult> CanRemoveAsync(T entity, IContextInfo ctx);

    }
}
