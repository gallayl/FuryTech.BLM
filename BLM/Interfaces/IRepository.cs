using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace BLM.Interfaces
{

    public interface IRepository
    {
        void SaveChanges(IIdentity user);
        Task SaveChangesAsync(IIdentity user);
    }

    public interface IRepository<T> : IRepository<T, T> where T : class
    {

    }

    public interface IRepository<in TInput, out TOutput> : IDisposable, IRepository where TInput : class where TOutput: class
    {
        IQueryable<TOutput> Entities(IIdentity user);
        void Add(IIdentity user, TInput newItem);
        Task AddAsync(IIdentity user, TInput newItem);
        void AddRange(IIdentity user, IEnumerable<TInput> newItems);
        Task AddRangeAsync(IIdentity user, IEnumerable<TInput> newItems);
        void Remove(IIdentity usr, TInput item);
        Task RemoveAsync(IIdentity usr, TInput item);
        void RemoveRange(IIdentity usr, IEnumerable<TInput> items);
        Task RemoveRangeAsync(IIdentity usr, IEnumerable<TInput> items);
    }

}
