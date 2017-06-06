using System.Threading.Tasks;

namespace BLM.NetStandard.Interfaces.Authorize
{
    internal interface IAuthorizeCreate<in T> : IBlmEntry
    {
        /// <summary>
        /// Authorizes an Insert operation
        /// </summary>
        /// <param name="entity">The entity to be inserted</param>
        /// <param name="ctx">The insertion context info</param>
        /// <returns>If the entity can be inserted</returns>
        Task<AuthorizationResult> CanCreateAsync(T entity, IContextInfo ctx);

    }
}
