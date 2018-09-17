using System.Threading.Tasks;
using BLM.NetStandard.Interfaces;

namespace BLM.NetStandard.Tests
{
    public class MockModifyAuthorizer : AuthorizeModify<MockEntity> {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task<AuthorizationResult> CanModifyAsync(MockEntity originalEntity, MockEntity modifiedEntity, IContextInfo ctx)
        {
            if (modifiedEntity.IsValid)
            {
                return AuthorizationResult.Success();
            }
            return AuthorizationResult.Fail("The entity is not valid :( ", modifiedEntity);
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}