using System.Threading.Tasks;

namespace BLM.NetStandard.Interfaces.Listen
{
    public interface IListenRemoveFailed<in T> : IBlmEntry
    {
        /// <summary>
        /// Triggered when an entity remove failed due validation in the Business Layer
        /// </summary>
        /// <param name="entity">The entity which has been failed to remove</param>
        /// <param name="context">The removal context</param>
        Task OnRemoveFailedAsync(T entity, IContextInfo context);
    }
}
