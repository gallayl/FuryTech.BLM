using System.Linq;
using System.Threading.Tasks;
using BLM.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BLM.EF6.Tests
{
    /// <summary>
    /// Summary description for ListenerTests
    /// </summary>
    [TestClass]
    public class RepositoryListenerTests : AbstractEfRepositoryTest
    {
        [TestCleanup]
        public override void Cleanup()
        {
            _repo?.Dispose();
            _db?.Dispose();
        }

        /// <summary>
        /// Adds two entities to the _repo
        /// </summary>
        /// <returns></returns>
        protected async Task _add()
        {
            await _repo.AddAsync(_identity, Entity1);
            await _repo.AddAsync(_identity, Entity2);
            await _repo.SaveChangesAsync(_identity);
        }

        [TestMethod]
        public async Task Add()
        {
            await _add();
            Assert.AreEqual(2, _repo.Entities(_identity).Count());
        }

        [TestMethod]
        [ExpectedException(typeof(BLM.Exceptions.LogicalSecurityRiskException))]
        public async Task LogicalDelete_ALL_Throw_SecurityRiskException()
        {
            await _add(); ;
            Assert.AreEqual(2, EfChangeListener.CreatedEntities.Count);

            await _repo.RemoveRangeAsync(_identity, _repo.Entities(_identity));
            await _repo.SaveChangesAsync(_identity);
        }

        [TestMethod]
        public async Task LogicalDelete_MockEntity_NoException()
        {
            await _add(); ;
            Assert.AreEqual(2, EfChangeListener.CreatedEntities.Count);

            await _repo.RemoveAsync(_identity, Entity1);
            await _repo.SaveChangesAsync(_identity);

            Assert.AreEqual(1, _repo.Entities(_identity).Count());
            Assert.AreEqual(1, EfChangeListener.RemovedEntities.Count);
            // The LogicalDeleting mock entity should just modified
            Assert.AreEqual(false, EfChangeListener.WasOnModifiedCalled);
        }

        [TestMethod]
        public async Task LogicalDelete_ALL_IgnoreLogicalDeleteAttributes_NoException()
        {
            await _add(); ;
            Assert.AreEqual(2, EfChangeListener.CreatedEntities.Count);

            _repo.IgnoreLogicalDeleteError = true;

            await _repo.RemoveRangeAsync(_identity, _repo.Entities(_identity));
            await _repo.SaveChangesAsync(_identity);

            Assert.AreEqual(0, _repo.Entities(_identity).Count());
            Assert.AreEqual(2, EfChangeListener.RemovedEntities.Count);
            // The LogicalDeleting mock entity should just modified
            Assert.AreEqual(false, EfChangeListener.WasOnModifiedCalled);
        }

        [TestMethod]
        public async Task LogicalDelete()
        {
            using (EfRepository<LogicalDeleteEntity> localRepository = new EfRepository<LogicalDeleteEntity>(_db))
            {
                await localRepository.AddAsync(_identity, Entity2);
                await localRepository.AddAsync(_identity, Entity3);
                await localRepository.SaveChangesAsync(_identity);

                Assert.AreEqual(2, EfChangeListener.CreatedEntities.Count);
                Assert.AreEqual(2, (await localRepository.EntitiesAsync(_identity)).Count());

                await localRepository.RemoveAsync(_identity, Entity2);
                await localRepository.SaveChangesAsync(_identity);

                Assert.AreEqual(2, (await localRepository.EntitiesAsync(_identity)).Count());
                var entities = (await localRepository.EntitiesAsync(_identity)).ToArray();
                Assert.AreEqual(1, (await localRepository.EntitiesAsync(_identity)).Count(entity => entity.IsDeleted && entity.Id == 1));
                Assert.AreEqual(1, (await localRepository.EntitiesAsync(_identity)).Count(entity => !entity.IsDeleted && entity.Id == 2));
                Assert.AreEqual(1, EfChangeListener.RemovedEntities.Count);
                Assert.AreEqual(0, EfChangeListener.ModifiedNewEntities.Count);
                Assert.AreEqual(0, EfChangeListener.ModifiedOriginalEntities.Count);
                // The LogicalDeleting mock entity should just modified
                Assert.AreEqual(false, EfChangeListener.WasOnModifiedCalled);
            }
        }
    }
}
