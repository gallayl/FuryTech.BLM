using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Security.Principal;
using System.Threading.Tasks;

namespace BLM.EF6
{
    public interface IEfRepository : IDisposable
    {
        Task<AuthorizationResult> AuthorizeEntityChangeAsync(IIdentity user, DbEntityEntry ent);

        Task DistributeToListenersAsync(List<object> added, EfContextInfo contextInfo, List<Tuple<object, object>> modified,
            List<object> removed, bool isChildRepository);

    }
}
