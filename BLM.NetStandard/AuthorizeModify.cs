using System.Threading.Tasks;
using BLM.NetStandard.Interfaces;
using BLM.NetStandard.Interfaces.Authorize;

namespace BLM.NetStandard
{
    public abstract class AuthorizeModify<T> : IAuthorizeModify<T>
    {
        public abstract Task<AuthorizationResult> CanModifyAsync(T originalEntity, T modifiedEntity, IContextInfo ctx);
    }
}