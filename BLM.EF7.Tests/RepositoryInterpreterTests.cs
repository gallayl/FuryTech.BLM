using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BLM.EF7.Tests
{
    [TestClass]
    public class RepositoryInterpreterTests : AbstractEfRepositoryTest
    {
        [TestMethod]
        public async Task TestCreateInterpret()
        {
            await _repoInterpreted.AddAsync(_identity, new MockInterpretedEntity()
            {
                MockInterpretedValue = MockInterpretedValue.Default
            });
            await _repoInterpreted.SaveChangesAsync(_identity);

            Assert.AreEqual(1, _repoInterpreted.Entities(_identity).Count());
            Assert.AreEqual(MockInterpretedValue.CreateInterpreted, _repoInterpreted.Entities(_identity).FirstOrDefault().MockInterpretedValue);
        }

        [TestMethod]
        public async Task TestModifyInterpret()
        {
            _db.MockInterpretedEntities.Add(new MockInterpretedEntity()
            {
                MockInterpretedValue = MockInterpretedValue.Default
            });
            await _db.SaveChangesAsync();

            var entity = _repoInterpreted.Entities(_identity).FirstOrDefault();
            entity.Index = 2;
            await _repoInterpreted.SaveChangesAsync(_identity);

            Assert.AreEqual(1, _repoInterpreted.Entities(_identity).Count());
            Assert.AreEqual(MockInterpretedValue.ModifyInterpreted, _repoInterpreted.Entities(_identity).FirstOrDefault().MockInterpretedValue);
        }
    }
}
