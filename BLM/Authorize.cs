using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLM.Interfaces;
using BLM.Interfaces.Authorize;

namespace BLM
{
    public static class Authorize
    {
        private static AuthorizationResult _elevatedResult = AuthorizationResult.Success("Elevated context");

        public static async Task<IQueryable<T>> CollectionAsync<T>(IQueryable<T> entities, IContextInfo context) where T : class
        {
            var collectionAuthorizers = Loader.GetEntriesFor<IAuthorizeCollection<T, T>>();
            foreach (var collectionAuthorizer in collectionAuthorizers)
            {

                var auth = (IAuthorizeCollection)collectionAuthorizer;
                entities = (await auth.AuthorizeCollectionAsync(entities, context)).Cast<T>();
            }

            return entities;
        }

        public static IQueryable<T> Collection<T>(IQueryable<T> entities, IContextInfo context) where T : class
        {
            return CollectionAsync<T>(entities, context).Result;
        }

        public static async Task<IEnumerable<AuthorizationResult>> CreateAsync<T>(T entity, IContextInfo context)
        {
            var createAuthorizers = Loader.GetEntriesFor<IAuthorizeCreate<T>>();
            List<AuthorizationResult> results = new List<AuthorizationResult>();
            foreach (var authorizer in createAuthorizers)
            {
                var auth = (IAuthorizeCreate<T>)authorizer;
                results.Add(await auth.CanCreateAsync(entity, context));
            }
            return results;
        }

        public static async Task<IEnumerable<AuthorizationResult>> ModifyAsync<T>(T originalEntity, T modifiedEntity, IContextInfo context)
        {

            var modifyAuthorizers = Loader.GetEntriesFor<IAuthorizeModify<T>>();
            List<AuthorizationResult> results = new List<AuthorizationResult>();

            foreach (var authorizer in modifyAuthorizers)
            {
                var auth = (IAuthorizeModify<T>)authorizer;
                results.Add(await auth.CanModifyAsync(originalEntity, modifiedEntity, context));
            }
            return results;
        }

        public static async Task<IEnumerable<AuthorizationResult>> RemoveAsync<T>(T entity, IContextInfo context)
        {
            var removeAuthorizers = Loader.GetEntriesFor<IAuthorizeRemove<T>>();

            List<AuthorizationResult> results = new List<AuthorizationResult>();

            foreach (var authorizer in removeAuthorizers)
            {
                var auth = (IAuthorizeRemove<T>)authorizer;
                results.Add(await auth.CanRemoveAsync(entity, context));
            }
            return results;
        }
    }
}
