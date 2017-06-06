using System.Threading.Tasks;

namespace BLM.NetStandard.Interfaces.Listen
{
    public interface IListenCreated<in T> : IBlmEntry
    {
        /// <summary>
        /// Triggered after the entity is validated, created and saved succesfully in the DB.
        /// </summary>
        /// <param name="entity">The created entity</param>
        /// <param name="context">The creation context</param>
        Task OnCreatedAsync(T entity, IContextInfo context);
    }
}
