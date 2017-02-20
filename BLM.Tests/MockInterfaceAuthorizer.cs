using System.Linq;
using System.Threading.Tasks;
using BLM.Interfaces;

namespace BLM.Tests
{
    public class MockInterfaceAuthorizer : AuthorizeCRUD<IMockEntity>
    {
        public override async Task<IQueryable<IMockEntity>> AuthorizeCollectionAsync(IQueryable<IMockEntity> entities, IContextInfo ctx)
        {
            return entities.Where(a => a.IsVisible);
        }

        public override async Task<AuthorizationResult> CanCreateAsync(IMockEntity entity, IContextInfo ctx)
        {
            return entity.IsValid ? AuthorizationResult.Success() : AuthorizationResult.Fail("Not valid!", entity);
        }

        public override async Task<AuthorizationResult> CanModifyAsync(IMockEntity originalEntity, IMockEntity modifiedEntity, IContextInfo ctx)
        {
            return modifiedEntity.IsValid ? AuthorizationResult.Success() : AuthorizationResult.Fail("Not valid!", modifiedEntity);

        }

        public override async Task<AuthorizationResult> CanRemoveAsync(IMockEntity entity, IContextInfo ctx)
        {
            return entity.IsValid ? AuthorizationResult.Success() : AuthorizationResult.Fail("Not valid!", entity);
        }
    }
}
