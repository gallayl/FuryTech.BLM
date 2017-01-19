using System.Linq;
using BLM.Interfaces.Authorize;
using System.Threading.Tasks;
using BLM.Interfaces;

namespace BLM.Tests
{
    public class MockCollectionAuthorizer2 : AuthorizeCollection<MockEntity>
    {
        public override async Task<IQueryable<MockEntity>> AuthorizeCollectionAsync(IQueryable<MockEntity> entities, IContextInfo ctx)
        {
            return await Task.Factory.StartNew(() => entities.Where(a => a.IsVisible2));
        }
    }
}