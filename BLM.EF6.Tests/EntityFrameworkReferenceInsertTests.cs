using System.Collections.Generic;
using System.Threading.Tasks;
using BLM.Exceptions;
using BLM.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BLM.EF6.Tests
{
    [TestClass]
    public class EntityFrameworkReferenceInsertTests : AbstractEfRepositoryTest
    {
        

        protected MockEntity ValidEntity = new MockEntity()
        {
            Id = 1,
            IsValid = true,
            IsVisible = true,
            IsVisible2 = true
        };

        protected MockEntity ValidEntity2 = new MockEntity()
        {
            Id = 2,
            IsValid = true,
            IsVisible = true,
            IsVisible2 = true

        };

        protected MockEntity InvalidEntity = new MockEntity()
        {
            Id = 3,
            IsValid = false,
            IsVisible = true,
            IsVisible2 = false
        };


        [TestMethod]
        public async Task AddOneWithoutNesting()
        {
            await _repoNested.AddAsync(_identity, new MockNestedEntity());
            await _repoNested.SaveChangesAsync(_identity);

        }

        [TestMethod]
        public async Task AddOneWithSingleNestingExisting()
        {
            await _repoNested.AddAsync(_identity, new MockNestedEntity()
            {
                MockEntities = new List<MockEntity>{ ValidEntity }
            });
            await _repoNested.SaveChangesAsync(_identity);
        }

        [TestMethod]
        public async Task AddTwoWithSingleNestingExisting()
        {
            await _repoNested.AddAsync(_identity, new MockNestedEntity()
            {
                MockEntities = new List<MockEntity> { ValidEntity, ValidEntity2 }
            });
            await _repoNested.SaveChangesAsync(_identity);

        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationFailedException))]
        public async Task AddOneInvalidNestingExisting()
        {
            await _repoNested.AddAsync(_identity, new MockNestedEntity()
            {
                MockEntities = new List<MockEntity> { InvalidEntity }
            });
            await _repoNested.SaveChangesAsync(_identity);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationFailedException))]
        public async Task AddOneValidAndOneInvalidNestingExisting()
        {
            await _repoNested.AddAsync(_identity, new MockNestedEntity()
            {
                MockEntities = new List<MockEntity> { ValidEntity, InvalidEntity }
            });
            await _repoNested.SaveChangesAsync(_identity);
        }

        [TestMethod]
        public async Task AddOneDeepInsert()
        {
            await _repoNested.AddAsync(_identity, new MockNestedEntity()
            {
                MockEntities = new List<MockEntity> { new MockEntity {IsValid = true} }
            });
            await _repoNested.SaveChangesAsync(_identity);
        }

        [TestMethod]
        public async Task AddTwoDeepInsert()
        {
            await _repoNested.AddAsync(_identity, new MockNestedEntity()
            {
                MockEntities = new List<MockEntity> { new MockEntity { IsValid = true }, new MockEntity { IsValid = true } }
            });
            await _repoNested.SaveChangesAsync(_identity);
        }
    }
}
