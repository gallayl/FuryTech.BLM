﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Security.Principal;

using BLM.Repository;

namespace BLM.EF6
{
    public class EfRepository<T> : IRepository<T> where T: class, new()
    {

        private readonly DbContext _dbcontext;
        private readonly DbSet<T> _dbset;

        public EfRepository(DbContext db)
        {
            _dbcontext = db;
            _dbset = db.Set<T>();

        }

        private EfContextInfo GetContextInfo(IIdentity user)
        {
            return new EfContextInfo(user, _dbcontext);
        }

        private void AuthorizeAdd(IIdentity usr, T newEntity)
        {
            _dbset.Attach(newEntity);
            if (!Authorizer.CanInsert(newEntity, GetContextInfo(usr)))
            {
                _listenerManager.TriggerOnCreationFailed(newEntity, GetContextInfo(usr));
                
                _dbcontext.Entry(newEntity).State = EntityState.Detached;
                throw new UnauthorizedAccessException();
            }
        }

        public void Add(IIdentity user, T newItem)
        {
            AuthorizeAdd(user, newItem);
            _dbset.Add(newItem);
        }

        public void AddRange(IIdentity user, IEnumerable<T> newItems)
        {
            var newItemlist = newItems.ToList();
            foreach (var item in newItemlist)
            {
                AuthorizeAdd(user, item);
            }
            _dbset.AddRange(newItemlist);
        }

        public void Dispose()
        {
            _dbcontext?.Dispose();
        }

        public IQueryable<T> Entities(IIdentity user)
        {
            return Authorizer.AuthorizeCollection(_dbset, GetContextInfo(user));
        }

        private void AuthorizeRemove(IIdentity user, T item)
        {
            if (!Authorizer.CanRemove(item, GetContextInfo(user)))
            {
                _listenerManager.TriggerOnRemoveFailed(item, GetContextInfo(user));
                throw new UnauthorizedAccessException();
            }
        }

        public void Remove(IIdentity user, T item)
        {
            AuthorizeRemove(user, item);
            _dbset.Remove(item);
        }

        public void RemoveRange(IIdentity user, IEnumerable<T> items)
        {
            var entityList = items.ToList();
            foreach (var entity in entityList)
            {
                AuthorizeRemove(user, entity);

            }
            _dbset.RemoveRange(entityList);
        }

        private bool AuthorizeEntityChange(IIdentity user, DbEntityEntry ent)
        {

            if (ent.State == EntityState.Unchanged || ent.State == EntityState.Detached)
                return true;

            if (ent.Entity is T)
            {
                var casted = ent.Cast<T>();
                switch (ent.State)
                {
                    case EntityState.Added:
                        var createdInterpreted = _listenerManager.TriggerOnBeforeCreate(casted.Entity, GetContextInfo(user)) as T;
                        return Authorizer.CanInsert(createdInterpreted, GetContextInfo(user));
                    case EntityState.Modified:
                        var original = CreateWithValues(casted.OriginalValues);
                        var modified = CreateWithValues(casted.CurrentValues);
                        var modifiedInterpreted = _listenerManager.TriggerOnBeforeModify(original, modified, GetContextInfo(user)) as T;
                        foreach (var field in ent.CurrentValues.PropertyNames)
                        {
                            ent.CurrentValues[field] = modifiedInterpreted.GetType().GetProperty(field).GetValue(modifiedInterpreted, null);
                        }
                        return Authorizer.CanUpdate(original, modifiedInterpreted, GetContextInfo(user));
                    case EntityState.Deleted:
                        return Authorizer.CanRemove(casted.Entity, GetContextInfo(user));
                    default:
                        throw new InvalidOperationException();
                }
            } else
            {
                throw new InvalidOperationException($"Changes for entity type '{ent.Entity.GetType().FullName}' is not supported in a context of a repository with type '{typeof(T).FullName}'");
            }
        }
        private T CreateWithValues(DbPropertyValues values)
        {
            T entity = new T();
            Type type = typeof(T);

            foreach (var name in values.PropertyNames)
            {
                var property = type.GetProperty(name);
                property.SetValue(entity, values.GetValue<object>(name));
            }

            return entity;
        }

        public void SaveChanges(IIdentity user)
        {
            _dbcontext.ChangeTracker.DetectChanges();
            var entries = _dbcontext.ChangeTracker.Entries().ToList();
            foreach (var entityChange in _dbcontext.ChangeTracker.Entries())
            {
                if (!AuthorizeEntityChange(user, entityChange))
                {
                    if (entityChange.State == EntityState.Modified)
                    {
                        _listenerManager.TriggerOnModificationFailed(CreateWithValues(entityChange.OriginalValues), entityChange.Entity as T, GetContextInfo(user));
                    }
                    else if (entityChange.State == EntityState.Deleted)
                    {
                        _listenerManager.TriggerOnRemoveFailed(CreateWithValues(entityChange.OriginalValues), GetContextInfo(user));
                    }
                    throw new UnauthorizedAccessException();
                }
            }

            var added = entries.Where(a => a.State == EntityState.Added).ToList();
            var modified = entries.Where(a => a.State == EntityState.Modified).ToList();
            var removed = entries.Where(a => a.State == EntityState.Deleted).Select(a=>CreateWithValues(a.OriginalValues)).ToList();

            _dbcontext.SaveChanges();

            added.ForEach(a => _listenerManager.TriggerOnCreated(a.Entity, GetContextInfo(user)));
            modified.ForEach(a => _listenerManager.TriggerOnModified( CreateWithValues(a.OriginalValues), a.Entity, GetContextInfo(user)));
            removed.ForEach(a => _listenerManager.TriggerOnRemoved(a, GetContextInfo(user)));
        }

        public void SetEntityState(T entity, EntityState newState)
        {
            _dbcontext.Entry(entity).State = newState;
        }

        public EntityState GetEntityState(T entity)
        {
            return _dbcontext.Entry(entity).State;
        }
    }
}
