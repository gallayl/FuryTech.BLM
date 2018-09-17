using System.Threading.Tasks;

namespace BLM.NetStandard.Interfaces.Listen
{
    public interface IListenModified<in T> : IBlmEntry
    {
        /// <summary>
        /// Triggered after an already existing entity is modified, validated and saved succesfully in the DB.
        /// </summary>
        /// <param name="originalEntity">The DbEntityEntry for the update.</param>
        /// <param name="modifiedEntity">The succesfully modified entity</param>
        /// <param name="context">The modification context</param>
        Task OnModifiedAsync(T originalEntity, T modifiedEntity, IContextInfo context);
    }
}
