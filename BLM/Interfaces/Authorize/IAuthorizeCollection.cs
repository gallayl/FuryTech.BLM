using System.Linq;
using System.Threading.Tasks;

namespace BLM.Interfaces.Authorize
{
    internal interface IAuthorizeCollection : IBlmEntry
    {
        /// <summary>
        /// Authorizes a collection to be read
        /// </summary>
        /// <param name="entities">The full entity set</param>
        /// <param name="ctx">The collection context info</param>
        /// <returns>The authorized entities</returns>
        Task<IQueryable> AuthorizeCollectionAsync(IQueryable entities, IContextInfo ctx);
    }

    internal interface IAuthorizeCollection<in TInput, TOutput> : IAuthorizeCollection where TInput : class where TOutput : class
    {
        /// <summary>
        /// Authorizes a collection to be read
        /// </summary>
        /// <param name="entities">The full entity set</param>
        /// <param name="ctx">The collection context info</param>
        /// <returns>The authorized entities</returns>
        Task<IQueryable<TOutput>> AuthorizeCollectionAsync(IQueryable<TInput> entities, IContextInfo ctx);
    }
}
