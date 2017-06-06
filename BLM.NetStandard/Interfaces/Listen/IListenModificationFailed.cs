using System.Threading.Tasks;

namespace BLM.NetStandard.Interfaces.Listen
{
    public interface IListenModificationFailed<in T> : IBlmEntry
    {
        /// <summary>
        /// Triggered when an entity modification has been failed due validation in the Business Layer
        /// </summary>
        /// <param name="originalEntity">The DbEntityEntry for the update.</param>
        /// <param name="modifiedEntity">The entity which has been failed to modify</param>
        /// <param name="context">The modification context</param>

        Task OnModificationFailedAsync(T originalEntity, T modifiedEntity, IContextInfo context);
    }
}
