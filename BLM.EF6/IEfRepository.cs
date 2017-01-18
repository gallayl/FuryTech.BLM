using System;
using System.Data.Entity.Infrastructure;
using System.Security.Principal;
using System.Threading.Tasks;

namespace BLM.EF6
{
    public interface IEfRepository : IDisposable
    {
        Task<AuthorizationResult> AuthorizeEntityChangeAsync(IIdentity user, DbEntityEntry ent);

    }
}
