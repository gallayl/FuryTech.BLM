using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using BLM.NetStandard;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BLM.EF7
{
    public interface IEfRepository : IDisposable
    {
        Task<AuthorizationResult> AuthorizeEntityChangeAsync(IIdentity user, EntityEntry ent);

        Task DistributeToListenersAsync(List<object> added, EfContextInfo contextInfo, List<Tuple<object, object>> modified,
            List<object> removed, bool isChildRepository);

    }
}
