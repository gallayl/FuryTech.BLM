using System.Linq;
using System.Threading.Tasks;
using BLM.Interfaces;
using BLM.Interfaces.Authorize;

namespace BLM
{
    public abstract class AuthorizeCollection<T> : IAuthorizeCollection<T, T>
    {
        /// <summary>
        /// Authorizes a collection to be read
        /// </summary>
        /// <param name="entities">The full entity set</param>
        /// <param name="ctx">The collection context info</param>
        /// <returns>The authorized entities</returns>
        public abstract Task<IQueryable<T>> AuthorizeCollectionAsync(IQueryable<T> entities, IContextInfo ctx);

        /// <summary>
        /// Authorizes a collection to be read
        /// </summary>
        /// <param name="entities">The full entity set</param>
        /// <param name="ctx">The collection context info</param>
        /// <returns>The authorized entities</returns>
        public async Task<IQueryable> AuthorizeCollectionAsync(IQueryable entities, IContextInfo ctx)
        {
            return await AuthorizeCollectionAsync(entities.Cast<T>(), ctx);
        }
    }
}