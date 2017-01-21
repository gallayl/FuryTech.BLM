using System.Threading.Tasks;

namespace BLM.Interfaces.Authorize
{
    internal interface IAuthorizeModify<in T> : IBlmEntry
    {
        /// <summary>
        /// Authorizes an update / modify operation
        /// </summary>
        /// <param name="originalEntity">The original entity</param>
        /// <param name="modifiedEntity">The modified entity</param>
        /// <param name="ctx">The insertion context info</param>
        /// <returns>If the entity can be updated / modified</returns>
        Task<AuthorizationResult> CanModifyAsync(T originalEntity, T modifiedEntity, IContextInfo ctx);
    }
}
