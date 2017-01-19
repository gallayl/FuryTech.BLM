using System.Threading.Tasks;

namespace BLM.Tests
{
    public class MockRemoveAuthorizer : AuthorizeRemove<MockEntity>
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task<AuthorizationResult> CanRemoveAsync(MockEntity entity, IContextInfo ctx)
        {
            if (entity.IsValid)
            {
                return AuthorizationResult.Success();
            }
            return AuthorizationResult.Fail("The entity is not valid :( ", entity);
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}