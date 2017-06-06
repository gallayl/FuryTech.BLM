using System.Threading.Tasks;

namespace BLM.NetStandard.Interfaces.Authorize
{
    internal interface IAuthorizeRemove<in T> : IBlmEntry
    {
        /// <summary>
        /// Authorizes a remove operation
        /// </summary>
        /// <param name="entity">The entity to be removed</param>
        /// <param name="ctx">The remove context info</param>
        /// <returns>If the entity can be removed or not</returns>
        Task<AuthorizationResult> CanRemoveAsync(T entity, IContextInfo ctx);
    }
}
