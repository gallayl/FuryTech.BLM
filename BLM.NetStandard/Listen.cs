using System.Threading.Tasks;
using BLM.NetStandard.Interfaces;
using BLM.NetStandard.Interfaces.Listen;

namespace BLM.NetStandard
{
    public static class Listen
    {
        public static async Task CreatedAsync<T>(T entity, IContextInfo context)
        {
            var createListeners = Loader.GetEntriesFor<IListenCreated<T>>();
            foreach (var createListener in createListeners)
            {
                await ((IListenCreated<T>)createListener).OnCreatedAsync(entity, context);
            }
        }

        public static void Created<T>(T entity, IContextInfo context)
        {
            CreatedAsync<T>(entity, context).Wait();
        }

        public static async Task CreateFailedAsync<T>(T entity, IContextInfo context)
        {
            var createFailListeners = Loader.GetEntriesFor<IListenCreateFailed<T>>();
            foreach (var listener in createFailListeners)
            {
                await ((IListenCreateFailed<T>)listener).OnCreateFailedAsync(entity, context);
            }
        }

        public static void CreateFailed<T>(T entity, IContextInfo context)
        {
            CreateFailedAsync(entity, context).Wait();
        }

        public static async Task ModifiedAsync<T>(T original, T modified, IContextInfo context)
        {

            var modifyListeners = Loader.GetEntriesFor<IListenModified<T>>();
            foreach (var listener in modifyListeners)
            {
                await ((IListenModified<T>)listener).OnModifiedAsync(original, modified, context);
            }
        }

        public static void Modified<T>(T original, T modified, IContextInfo context)
        {
            ModifiedAsync(original, modified, context).Wait();
        }

        public static async Task ModificationFailedAsync<T>(T original, T modified, IContextInfo context)
        {
            var modifyListeners = Loader.GetEntriesFor<IListenModificationFailed<T>>();
            foreach (var listener in modifyListeners)
            {
                await ((IListenModificationFailed<T>)listener).OnModificationFailedAsync(original, modified, context);
            }
        }

        public static void ModificationFailed<T>(T original, T modified, IContextInfo context)
        {
            ModificationFailedAsync(original, modified, context).Wait();
        }

        public static async Task RemovedAsync<T>(T entity, IContextInfo context)
        {
            var modifyListeners = Loader.GetEntriesFor<IListenRemoved<T>>();
            foreach (var listener in modifyListeners)
            {
                await ((IListenRemoved<T>)listener).OnRemovedAsync(entity, context);
            }
        }

        public static void Removed<T>(T entity, IContextInfo context)
        {
            RemovedAsync(entity, context).Wait();
        }

        public static async Task RemoveFailedAsync<T>(T entity, IContextInfo context)
        {
            var modifyListeners = Loader.GetEntriesFor<IListenRemoveFailed<T>>();
            foreach (var listener in modifyListeners)
            {
                await ((IListenRemoveFailed<T>)listener).OnRemoveFailedAsync(entity, context);
            }
        }

        public static void RemoveFailed<T>(T entity, IContextInfo context)
        {
            RemoveFailedAsync(entity, context).Wait();
        }
    }
}
