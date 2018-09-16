using System;
using System.Security.Principal;
using BLM.NetStandard.Tests;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BLM.EF7.Tests
{
    [TestClass]
    public class AbstractEfRepositoryTest
    {
        protected FakeDbContext _db;
        protected EfRepository<MockEntity> _repo;
        protected EfRepository<MockNestedEntity> _repoNested;
        protected EfRepository<MockInterpretedEntity> _repoInterpreted;
        protected IIdentity _identity;
        
        // Needed to proper DB name generation
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Init()
        {
            /// In EFCore 1.x there is no transient InMemory db, so we'll need to generate spearated db-s for testing.
            /// In EFCore 2.x there will be a TransientInMemoryDatabase, so we'll have to use that later.
            var dbContextOptionsBuilder = new DbContextOptionsBuilder();
            dbContextOptionsBuilder.UseInMemoryDatabase($"{TestContext.FullyQualifiedTestClassName}.{TestContext.TestName}-{Guid.NewGuid()}");
            //var dbContextOptionsBuilder = InMemoryDbContextOptionsExtensions.UseTransientInMemoryDatabase(new DbContextOptionsBuilder(new DbContextOptions<FakeDbContext>()));


            _db = new FakeDbContext(dbContextOptionsBuilder.Options);

            _repo = new EfRepository<MockEntity>(_db);
            _repoNested = new EfRepository<MockNestedEntity>(_db);
            _repoInterpreted = new EfRepository<MockInterpretedEntity>(_db);
            _identity = new GenericIdentity("gallayb");

            EfChangeListener.Reset();
            Entity1 = new MockEntity()
            {
                Id = 1,
                IsValid = true,
                IsVisible = true,
                IsVisible2 = true
            };

            Entity2 = new LogicalDeleteEntity()
            {
                Id = 2,
                IsValid = true,
                IsVisible = true,
                IsVisible2 = true
            };

            Entity3 = new LogicalDeleteEntity()
            {
                Id = 3,
                IsValid = true,
                IsVisible = true,
                IsVisible2 = true
            };
        }

        [TestCleanup]
        public virtual void Cleanup()
        {
            _repo?.Dispose();
            _db?.Dispose();
        }

        protected MockEntity Entity1 { get; set; }
        protected LogicalDeleteEntity Entity2 { get; set; }
        protected LogicalDeleteEntity Entity3 { get; set; }
    }
}
