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
            var repoKey = entry.Entity.GetType().FullName;
            if (_childRepositories.ContainsKey(repoKey))
            {
                return _childRepositories[repoKey];
            }
            var childRepositoryType = typeof(EfRepository<>).MakeGenericType(entry.Entity.GetType());
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
            var result = await AuthorizeAddAsync(user, newItem);

            if (!result.HasSucceed)
            {
                throw new AuthorizationFailedException(result);
            }
            _dbset.Add(newItem);
        }

        public void AddRange(IIdentity user, IEnumerable<T> newItems)
        {
            AddRangeAsync(user, newItems).Wait();
        }

        public async Task AddRangeAsync(IIdentity user, IEnumerable<T> newItems)
        {
            var fails = new List<AuthorizationResult>();

            var newItemlist = newItems.ToList();
            foreach (var item in newItemlist)
            {
                var result = await AuthorizeAddAsync(user, item);
                if (!result.HasSucceed)
                {
                    fails.Add(result);
                }
            }

            if (fails.Any())
            {
                throw new AuthorizationFailedException(fails.CreateAggregateResult());
            }

            _dbset.AddRange(newItemlist);
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
            var result = (await Authorize.RemoveAsync(item, GetContextInfo(user))).CreateAggregateResult();
            if (!result.HasSucceed)
            {
                throw new AuthorizationFailedException(result);
            }
            _dbset.Remove(item);
        }

        public void RemoveRange(IIdentity usr, IEnumerable<T> items)
        {
            RemoveRangeAsync(usr, items).Wait();
        }

        public async Task RemoveRangeAsync(IIdentity user, IEnumerable<T> items)
        {
            var fails = new List<AuthorizationResult>();
            var entityList = items.ToList();
            foreach (var entity in entityList)
            {
                var result = (await Authorize.RemoveAsync(entity, GetContextInfo(user))).CreateAggregateResult();
                if (!result.HasSucceed)
                {
                    fails.Add(result);
                }
            }

            if (fails.Any())
            {
                var aggregated = fails.CreateAggregateResult();
                throw new AuthorizationFailedException(aggregated);
            }

            _dbset.RemoveRange(entityList);
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
                        var original = await CreateWithValuesAsync(casted.OriginalValues);
                        var modified = await CreateWithValuesAsync(casted.CurrentValues);
                        var modifiedInterpreted = Interpret.BeforeModify(original, modified, GetContextInfo(user));
                        foreach (var field in ent.CurrentValues.PropertyNames)
                        {
                            ent.CurrentValues[field] = modifiedInterpreted.GetType().GetProperty(field).GetValue(modifiedInterpreted, null);
                        }
                        return (await Authorize.ModifyAsync(original, modifiedInterpreted, GetContextInfo(user))).CreateAggregateResult();
                    case EntityState.Deleted:
                        return (await Authorize.RemoveAsync(await CreateWithValuesAsync(casted.OriginalValues, casted.Entity.GetType()), GetContextInfo(user))).CreateAggregateResult();
                    default:
                        return AuthorizationResult.Fail("The entity state is invalid", casted.Entity);
                }
            }
            else
            {
                var childRepositoryType = typeof(EfRepository<>).MakeGenericType(ent.Entity.GetType());
                var childRepo = Activator.CreateInstance(childRepositoryType, _dbcontext, false) as IEfRepository;
                var authResult = await childRepo.AuthorizeEntityChangeAsync(user, ent);
                childRepo.Dispose();
                return authResult;
            }
        }
        private static async Task<T> CreateWithValuesAsync(DbPropertyValues values, Type type = null)
        {
            return await Task.Factory.StartNew(() =>
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
            });
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
                        await Listen.ModificationFailedAsync(await CreateWithValuesAsync(entityChange.OriginalValues), entityChange.Entity as T, GetContextInfo(user));
                    }
                    else if (entityChange.State == EntityState.Deleted)
                    {
                        await Listen.RemoveFailedAsync(await CreateWithValuesAsync(entityChange.OriginalValues), contextInfo);
                    }
                    throw new AuthorizationFailedException(authResult);
                }
            }

            // Added should be updated after saving changes for get the ID of the newly created entity
            var added = entries.Where(a => a.State == EntityState.Added).Select(a => a.Entity).Cast<T>().ToList();

            var modified =
                entries.Where(a => a.State == EntityState.Modified).Select(async a => await SelectBothAsync(a)).ToList();
            var removed =
                entries.Where(a => a.State == EntityState.Deleted).Select(async a => await SelectOriginalAsync(a)).ToList();

            var tasks = new List<Task>(removed);
            tasks.AddRange(modified);

            await Task.WhenAll(tasks.ToArray());

            if (removed.Any())
            {
                if (GetLogicalDeleteProperty(_type) == null)
                {
                    if (!IgnoreLogicalDeleteError &&
                        removed.Any(entry => GetLogicalDeleteProperty(entry.Result.GetType()) != null))
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

            tasks.Clear();

            tasks.AddRange(added.Select(async a => await Listen.CreatedAsync(a, contextInfo)));
            tasks.AddRange(
                modified.Select(
                    async a =>
                        await
                            Listen.ModifiedAsync((await a).Item1, (await a).Item2, contextInfo)));
            tasks.AddRange(removed.Select(async a => await Listen.RemovedAsync((await a), contextInfo)));

            await Task.WhenAll(tasks.ToArray());
        }

        public void SetEntityState(T entity, EntityState newState)
        {
            _dbcontext.Entry(entity).State = newState;
        }

        private static async Task<T> SelectCurrentAsync(DbEntityEntry a, Type type = null)
        {
            if (type == null)
            {
                type = a.Entity.GetType();
            }
            return await CreateWithValuesAsync(a.CurrentValues.Clone(), type);
        }
        private static async Task<T> SelectOriginalAsync(DbEntityEntry a, Type type = null)
        {
            if (type == null)
            {
                type = a.Entity.GetType();
            }
            return await CreateWithValuesAsync(a.OriginalValues.Clone(), type);
        }

        private static async Task<Tuple<T, T >> SelectBothAsync(DbEntityEntry a)
        {
            var type = a.Entity.GetType();
            return (new Tuple<T, T>(await SelectOriginalAsync(a, type), await SelectCurrentAsync(a, type)));
        }

        public EntityState GetEntityState(T entity)
        {
            return _dbcontext.Entry(entity).State;
        }
    }
}
