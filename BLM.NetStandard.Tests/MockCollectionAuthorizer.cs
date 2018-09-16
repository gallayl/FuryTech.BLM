using System.Linq;
using System.Threading.Tasks;
using BLM.NetStandard.Interfaces;

namespace BLM.NetStandard.Tests
{
    public class MockCollectionAuthorizer : AuthorizeCollection<MockEntity>
    {
        public override async Task<IQueryable<MockEntity>> AuthorizeCollectionAsync(IQueryable<MockEntity> entities, IContextInfo ctx)
        {
            return await Task.Factory.StartNew(() => entities.Where(a => a.IsVisible));
        }
        
    }

}