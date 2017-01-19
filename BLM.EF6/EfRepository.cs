using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;

using BLM.Extensions;
using BLM.Exceptions;
using BLM.Attributes;
using BLM.Interfaces;

namespace BLM.EF6
{
    public class EfRepository<T> : IRepository<T>, IEfRepository where T : class, new()
    {

        private readonly DbContext _dbcontext;
        private readonly DbSet<T> _dbset;
        private readonly Type _type;
        private readonly bool _disposeDbContextOnDispose;

        private readonly Dictionary<string, IEfRepository> _childRepositories = new Dictionary<string, IEfRepository>();

        /// <summary>
        /// If there are inherited objets, which have LogicalDeleteAttribue-s on it, we throw an exception
        /// </summary>
        public bool IgnoreLogicalDeleteError { get; set; } = false;

        public EfRepository(DbContext db, bool disposeDbContextOnDispose = true)
        {
            _dbcontext = db;
            _dbset = db.Set<T>();
            _type = typeof(T);
            _disposeDbContextOnDispose = disposeDbContextOnDispose;
            // TODO: What about the inherited classes? 
        }

        private EfContextInfo GetContextInfo(IIdentity user)
        {
            return new EfContextInfo(user, _dbcontext);
        }

        private IEfRepository GetChildRepositoryFor(DbEntityEntry entry)
        {
            var repoType = entry.Entity.GetType();
            return GetChildRepositoryFor(repoType);
        }

        private IEfRepository GetChildRepositoryFor(Type type)
        {
            var repoKey = type.FullName;
            if (_childRepositories.ContainsKey(repoKey))
            {
                return _childRepositories[repoKey];
            }
            var childRepositoryType = typeof(EfRepository<>).MakeGenericType(type);
            var childRepo = Activator.CreateInstance(childRepositoryType, _dbcontext, false) as IEfRepository;
            return childRepo;
        }

        #region Static things
        /// <summary>
        /// Check the given type if it has an LogicalDeleteAttribute on any property, and returns with the first property it founds (or null)
        /// </summary>
        /// <param name="type">Checked type</param>
        /// <returns></returns>
        public static PropertyInfo GetLogicalDeleteProperty(Type type)
        {
            if (!LogicalDeleteCache.ContainsKey(type.FullName))
            {
                LogicalDeleteCache.Add(type.FullName, type.GetProperties().FirstOrDefault(x => x.GetCustomAttributes<LogicalDeleteAttribute>().Any()));
            }
            return LogicalDeleteCache[type.FullName];
        }

        private static readonly Dictionary<string, PropertyInfo> LogicalDeleteCache;

        static EfRepository()
        {
            LogicalDeleteCache = new Dictionary<string, PropertyInfo>();
        }
        #endregion

        private async Task<AuthorizationResult> AuthorizeAddAsync(IIdentity usr, T newEntity)
        {
            var authResult = (await Authorize.CreateAsync(newEntity, GetContextInfo(usr))).CreateAggregateResult();
            if (!authResult.HasSucceed)
            {
                await Listen.CreateFailedAsync(newEntity, GetContextInfo(usr));
                _dbcontext.Entry(newEntity).State = EntityState.Detached;
            }

            return authResult;
        }

        public void Add(IIdentity user, T newItem)
        {
            AddAsync(user, newItem).Wait();
        }

        public async Task AddAsync(IIdentity user, T newItem)
        {
            await Task.Factory.StartNew(() => {
                _dbset.Add(newItem);
            });
        }

        public void AddRange(IIdentity user, IEnumerable<T> newItems)
        {
            AddRangeAsync(user, newItems).Wait();
        }

        public async Task AddRangeAsync(IIdentity user, IEnumerable<T> newItems)
        {
            await Task.Factory.StartNew(() => {
                _dbset.AddRange(newItems);
            });
        }


        public void Dispose()
        {
            foreach (KeyValuePair<string, IEfRepository> childRepo in _childRepositories)
            {
                childRepo.Value?.Dispose();
            }
            if (_disposeDbContextOnDispose)
            {
                _dbcontext?.Dispose();
            }
        }

        public IQueryable<T> Entities(IIdentity user)
        {
            return Authorize.Collection(_dbset, GetContextInfo(user));
        }

        public async Task<IQueryable<T>> EntitiesAsync(IIdentity user)
        {
            return await Authorize.CollectionAsync(_dbset, GetContextInfo(user));
        }

        public void Remove(IIdentity usr, T item)
        {
            RemoveAsync(usr, item).Wait();
        }

        public async Task RemoveAsync(IIdentity user, T item)
        {
            await Task.Factory.StartNew(() => {
                _dbset.Remove(item);
            });
        }

        public void RemoveRange(IIdentity usr, IEnumerable<T> items)
        {
            RemoveRangeAsync(usr, items).Wait();
        }

        public async Task RemoveRangeAsync(IIdentity user, IEnumerable<T> items)
        {
            await Task.Factory.StartNew(() => {
                _dbset.RemoveRange(items);
            });
        }

        private AuthorizationResult AuthorizeEntityChange(IIdentity user, DbEntityEntry ent)
        {
            return AuthorizeEntityChangeAsync(user, ent).Result;
        }

        public async Task<AuthorizationResult> AuthorizeEntityChangeAsync(IIdentity user, DbEntityEntry ent)
        {

            if (ent.State == EntityState.Unchanged || ent.State == EntityState.Detached)
                return AuthorizationResult.Success();

            if (ent.Entity is T)
            {
                var casted = ent.Cast<T>();
                switch (ent.State)
                {
                    case EntityState.Added:
                        T interpreted = Interpret.BeforeCreate(casted.Entity, GetContextInfo(user));
                        return (await Authorize.CreateAsync(interpreted, GetContextInfo(user))).CreateAggregateResult();

                    case EntityState.Modified:
                        var original = CreateWithValues(casted.OriginalValues);
                        var modified = CreateWithValues(casted.CurrentValues);
                        var modifiedInterpreted = Interpret.BeforeModify(original, modified, GetContextInfo(user));
                        foreach (var field in ent.CurrentValues.PropertyNames)
                        {
                            ent.CurrentValues[field] = modifiedInterpreted.GetType().GetProperty(field).GetValue(modifiedInterpreted, null);
                        }
                        return (await Authorize.ModifyAsync(original, modifiedInterpreted, GetContextInfo(user))).CreateAggregateResult();
                    case EntityState.Deleted:
                        return (await Authorize.RemoveAsync(CreateWithValues(casted.OriginalValues, casted.Entity.GetType()), GetContextInfo(user))).CreateAggregateResult();
                    default:
                        return AuthorizationResult.Fail("The entity state is invalid", casted.Entity);
                }
            }
            else
            {
                return await GetChildRepositoryFor(ent).AuthorizeEntityChangeAsync(user, ent);
            }
        }
        private static T CreateWithValues(DbPropertyValues values, Type type = null)
        {
                if (type == null)
                {
                    type = typeof(T);
                }

                T entity = (T)Activator.CreateInstance(type);

                foreach (string name in values.PropertyNames)
                {
                    var value = values.GetValue<object>(name);
                    var property = type.GetProperty(name);

                    if (value != null)
                    {
                        property.SetValue(entity, Convert.ChangeType(value, property.PropertyType), null);
                    }
                }
                return entity;
        }

        public void SaveChanges(IIdentity user)
        {
            SaveChangesAsync(user).Wait();
        }

        public async Task SaveChangesAsync(IIdentity user)
        {
            var contextInfo = GetContextInfo(user);

            _dbcontext.ChangeTracker.DetectChanges();
            var entries = _dbcontext.ChangeTracker.Entries().ToList();

            foreach (var entityChange in _dbcontext.ChangeTracker.Entries())
            {
                var authResult = await AuthorizeEntityChangeAsync(user, entityChange);
                if (!authResult.HasSucceed)
                {
                    if (entityChange.State == EntityState.Modified)
                    {
                        await Listen.ModificationFailedAsync(CreateWithValues(entityChange.OriginalValues), entityChange.Entity as T, GetContextInfo(user));
                    }
                    else if (entityChange.State == EntityState.Deleted)
                    {
                        await Listen.RemoveFailedAsync(CreateWithValues(entityChange.OriginalValues), contextInfo);
                    }
                    throw new AuthorizationFailedException(authResult);
                }
            }

            // Added should be updated after saving changes for get the ID of the newly created entity
            var added = entries.Where(a => a.State == EntityState.Added).Select(a => a.Entity).ToList();
            var modified = entries.Where(a => a.State == EntityState.Modified).Select(SelectBoth).ToList();
            var removed = entries.Where(a => a.State == EntityState.Deleted).Select(a => SelectOriginal(a)).ToList();

            //var tasks = new List<Task>(removed);
            //tasks.AddRange(modified);

            //await Task.WhenAll(tasks.ToArray());

            if (removed.Any())
            {
                if (GetLogicalDeleteProperty(_type) == null)
                {
                    if (!IgnoreLogicalDeleteError &&
                        removed.Any(entry => GetLogicalDeleteProperty(entry.GetType()) != null))
                    {
                        throw new LogicalSecurityRiskException(
                            $"There are derived types in the deleted entries which have LogicalDeleteAttribute, but the base type does not use logical delete.");
                    }
                }
                else
                {
                    var logicalRemoved = entries.Where(a => a.State == EntityState.Deleted).ToList();
                    logicalRemoved.ForEach(entry =>
                    {
                        //await entry.ReloadAsync();
                        entry.State = EntityState.Modified;
                        entry.Property(GetLogicalDeleteProperty(_type).Name).CurrentValue = true;
                    });
                }
            }
            await _dbcontext.SaveChangesAsync();
            await DistributeToListenersAsync(added, contextInfo, modified, removed);
        }

        public async Task DistributeToListenersAsync(List<object> added, EfContextInfo contextInfo, List<Tuple<object, object>> modified, List<object> removed, bool isChildRepository = false)
        {

            if (!isChildRepository)
            {
                List<Type> otherTypes = added.Where(a => !(a is T)).Select(a => a.GetType()).ToList();
                otherTypes.AddRange(modified.Where(a => !(a.Item1 is T)).Select(a => a.Item1.GetType()));
                otherTypes.AddRange(removed.Where(a => !(a is T)).Select(a => a.GetType()));
                foreach (var otherType in otherTypes.Distinct())
                {
                    var repo = GetChildRepositoryFor(otherType);
                    await repo.DistributeToListenersAsync(added, contextInfo, modified, removed, true);
                }
            }


            /* from the same type */
            //added.Where(a=>(a) is T).Cast<T>().Select(async a => await Listen.CreatedAsync(a, contextInfo));
            foreach (var addedEntry in added.Where(a => (a) is T).Cast<T>())
            {
                await Listen.CreatedAsync(addedEntry, contextInfo);
            }
            //var t2 = modified.Where(a => a is Tuple<T,T>).Cast<Tuple<T,T>>().Select(async a =>await Listen.ModifiedAsync((a).Item1, (a).Item2, contextInfo));
            foreach (var modifiedEntry in modified.Where(a => a is Tuple<T, T>).Cast<Tuple<T, T>>())
            {
                await Listen.ModifiedAsync(modifiedEntry.Item1, modifiedEntry.Item2, contextInfo);
            }

            //var t3 = removed.Where(a => a is T).Cast<T>().Select(async a => await Listen.RemovedAsync((a), contextInfo));
            foreach (var removedEntry in removed.Where(a => a is T).Cast<T>())
            {
                await Listen.RemovedAsync(removedEntry, contextInfo);
            }

        }

        public void SetEntityState(T entity, EntityState newState)
        {
            _dbcontext.Entry(entity).State = newState;
        }

        private static T SelectCurrent(DbEntityEntry a, Type type = null)
        {
            if (type == null)
            {
                type = a.Entity.GetType();
            }
            return CreateWithValues(a.CurrentValues.Clone(), type);
        }
        private static object SelectOriginal(DbEntityEntry a, Type type = null)
        {
            if (type == null)
            {
                type = a.Entity.GetType();
            }
            return CreateWithValues(a.OriginalValues.Clone(), type);
        }

        private static Tuple<object, object> SelectBoth(DbEntityEntry a)
        {
            var type = a.Entity.GetType();
            return (new Tuple<object, object>(SelectOriginal(a, type), SelectCurrent(a, type)));
        }

        public EntityState GetEntityState(T entity)
        {
            return _dbcontext.Entry(entity).State;
        }
    }
}
