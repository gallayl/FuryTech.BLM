using System.Threading.Tasks;
using BLM.Interfaces.Authorize;

namespace BLM
{
    public abstract class AuthorizeModify<T> : IAuthorizeModify<T>
    {
        public abstract Task<AuthorizationResult> CanModifyAsync(T originalEntity, T modifiedEntity, IContextInfo ctx);
    }
}