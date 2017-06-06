using System;
using System.Collections.Generic;
using System.Linq;
using BLM.NetStandard.Tests;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using BLM.NetStandard.Exceptions;

namespace BLM.EF7.Tests
{

    [TestClass]
    public class BasicTests : AbstractEfRepositoryTest
    {
        protected MockEntity ValidEntity = new MockEntity()
        {
            Id = 1,
            IsValid = true,
            IsVisible = true,
            IsVisible2 = true

        };

        protected MockEntity InvalidEntity = new MockEntity()
        {
            Id = 2,
            IsValid = false,
            IsVisible = true,
            IsVisible2 = false
        };

        protected MockEntity InvisibleEntity = new MockEntity()
        {
            Id = 3,
            IsValid = true,
            IsVisible = false,
            IsVisible2 = true
        };

        protected MockEntity InvisibleEntity2 = new MockEntity()
        {
            Id = 4,
            IsValid = true,
            IsVisible = false,
            IsVisible2 = true
        };

        [TestMethod]
        public virtual async Task Add()
        {
            await _repo.AddAsync(_identity, ValidEntity);
            await _repo.SaveChangesAsync(_identity);
            Assert.AreEqual(1, _db.MockEntities.Count());
        }

        [TestMethod]
        public virtual async Task AddFailure()
        {
            await _repo.AddAsync(_identity, InvalidEntity);
            try
            {
                await _repo.SaveChangesAsync(_identity);
            }
            catch (Exception e)
            {
                Assert.AreEqual(0, _db.MockEntities.Count());
                Assert.IsInstanceOfType(e, typeof(AuthorizationFailedException));
            }
        }

        [TestMethod]
        public virtual async Task AddRangeFail()
        {
            await _repo.AddRangeAsync(_identity, new List<MockEntity>() { ValidEntity, InvisibleEntity, InvalidEntity });
            try
            {
                await _repo.SaveChangesAsync(_identity);
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(e, typeof(AuthorizationFailedException));
                Assert.AreEqual(0, _db.MockEntities.Count());
            }
        }

        [TestMethod]
        public virtual async Task Modify()
        {
            ValidEntity.Guid = Guid.NewGuid().ToString();
            await _repo.AddAsync(_identity, ValidEntity);
            await _repo.SaveChangesAsync(_identity);

            var newGuid = Guid.NewGuid().ToString();
            var loadedValid = _repo.Entities(_identity).FirstOrDefault(a => a.Id == ValidEntity.Id);
            loadedValid.Guid = newGuid;
            await _repo.SaveChangesAsync(_identity);

            var reloadedValid = _repo.Entities(_identity).FirstOrDefault(a => a.Id == ValidEntity.Id);
            Assert.AreEqual(reloadedValid.Guid, newGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationFailedException))]
        public virtual async Task ModifyFailedAsync()
        {
            await _repo.AddAsync(_identity, ValidEntity);
            await _repo.SaveChangesAsync(_identity);

            var validToChange = _repo.Entities(_identity).FirstOrDefault(a => a.Id == ValidEntity.Id);
            validToChange.IsValid = false;

            await _repo.SaveChangesAsync(_identity);
        }

        [TestMethod]
        public virtual async Task RemoveAsync()
        {
            await _repo.AddAsync(_identity, ValidEntity);
            await _repo.SaveChangesAsync(_identity);

            await _repo.RemoveAsync(_identity, ValidEntity);
            await _repo.SaveChangesAsync(_identity);
        }

        [TestMethod]
        public virtual async Task RemoveRange()
        {
            await _repo.AddAsync(_identity, ValidEntity);
            await _repo.SaveChangesAsync(_identity);

            await _repo.RemoveRangeAsync(_identity, new List<MockEntity> { ValidEntity });
            await _repo.SaveChangesAsync(_identity);
        }



        [TestMethod]
        [ExpectedException(typeof(AuthorizationFailedException))]
        public virtual async Task RemoveFailed()
        {
            _db.Set<MockEntity>().Add(InvalidEntity);
            await _db.SaveChangesAsync();

            await _repo.RemoveAsync(_identity, InvalidEntity);
            await _repo.SaveChangesAsync(_identity);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationFailedException))]
        public virtual async Task RemoveRangeFailed()
        {
            _db.Set<MockEntity>().Add(InvalidEntity);
            await _db.SaveChangesAsync();

            await _repo.RemoveRangeAsync(_identity, new List<MockEntity> { InvalidEntity });
            await _repo.SaveChangesAsync(_identity);
        }


        [TestMethod]
        public virtual async Task AuthorizeCollection()
        {
            try
            {
                await _repo.AddRangeAsync(_identity, new List<MockEntity>() { ValidEntity, InvisibleEntity, InvisibleEntity2 });
                await _repo.SaveChangesAsync(_identity);
                var authorizationResult = (await _repo.EntitiesAsync(_identity));
                var queryResult = authorizationResult.All(a => a.IsVisible && a.IsVisible2);
                Assert.IsTrue(queryResult);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [TestMethod]
        public virtual async Task CtxAuthorizedEntitySet()
        {
            var ctx = new EfContextInfo(_identity, _db);

            _db.Set<MockEntity>().AddRange(new List<MockEntity>() { ValidEntity, InvalidEntity, InvisibleEntity, InvisibleEntity2 });
            await _db.SaveChangesAsync();

            Assert.IsTrue((await ctx.GetAuthorizedEntitySetAsync<MockEntity>()).All(a => a.IsVisible && a.IsVisible2));
        }

        [TestMethod]
        public virtual async Task CtxFullEntitySet()
        {
            var ctx = new EfContextInfo(_identity, _db);

            _db.Set<MockEntity>().AddRange(new List<MockEntity>() { ValidEntity, InvalidEntity, InvisibleEntity, InvisibleEntity2 });
            await _db.SaveChangesAsync();

            Assert.IsTrue(ctx.GetFullEntitySet<MockEntity>().Any(a => !a.IsVisible || !a.IsVisible2));
        }

        [TestMethod]
        public virtual async Task GetSetEntityState()
        {
            await _repo.AddAsync(_identity, ValidEntity);
            Assert.AreEqual(_repo.GetEntityState(ValidEntity), EntityState.Added);
            _repo.SetEntityState(ValidEntity, EntityState.Modified);
            Assert.AreEqual(_repo.GetEntityState(ValidEntity), EntityState.Modified);
        }

        [TestMethod]
        public virtual async Task SkipUnchangedEntities()
        {
            await _repo.AddAsync(_identity, ValidEntity);
            await _repo.SaveChangesAsync(_identity);

            var loaded = _repo.Entities(_identity).FirstOrDefault(a => a.Id == ValidEntity.Id);
            ValidEntity.Guid = Guid.NewGuid().ToString();
            _repo.SetEntityState(loaded, EntityState.Unchanged);

            await _repo.SaveChangesAsync(_identity);
        }
    }
}
