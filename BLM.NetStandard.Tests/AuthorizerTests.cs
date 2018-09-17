using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using BLM.NetStandard.Interfaces;
using BLM.NetStandard.Extensions;
using System.Security.Principal;

namespace BLM.NetStandard.Tests
{
    [TestClass]
    public class AuthorizerTests
    {
        private readonly MockEntity _valid = new MockEntity()
        {
            Id = 1,
            IsValid = true,
            IsVisible = true,
            IsVisible2 = true

        };

        private readonly MockEntity _invalid = new MockEntity()
        {
            Id = 2,
            IsValid = false,
            IsVisible = true,
            IsVisible2 = false
        };

        private readonly MockEntity _invisible = new MockEntity()
        {
            Id = 3,
            IsValid = true,
            IsVisible = false,
            IsVisible2 = true
        };

        private readonly MockImplementedEntity _validMockImplementedEntity = new MockImplementedEntity()
        {
            IsValid = true,
            IsVisible = true
        };

        private readonly MockImplementedEntity _invalidMockImplementedEntity = new MockImplementedEntity()
        {
            IsValid = false,
            IsVisible = true
        };

        private readonly MockImplementedEntity _invisibleMockImplementedEntity = new MockImplementedEntity()
        {
            IsVisible = false
        };

        readonly IContextInfo _ctx = new GenericContextInfo(new GenericIdentity("gallayb"));

        [TestMethod]
        public async Task CreateSuccess()
        {
            Assert.IsTrue((await Authorize.CreateAsync(_valid, _ctx)).HasSucceeded());
        }

        [TestMethod]
        public async Task CreateFail()
        {
            Assert.IsFalse((await Authorize.CreateAsync(_invalid, _ctx)).HasSucceeded());
        }

        [TestMethod]
        public async Task Modify()
        {
            Assert.IsTrue((await Authorize.ModifyAsync(_valid, _valid, _ctx)).HasSucceeded());

        }

        [TestMethod]
        public async Task ModifyFails()
        {
            Assert.IsFalse((await Authorize.ModifyAsync(_invalid, _invalid, _ctx)).HasSucceeded());
        }

        [TestMethod]
        public async Task Remove()
        {
            Assert.IsTrue((await Authorize.RemoveAsync(_valid, _ctx)).HasSucceeded());

        }

        [TestMethod]
        public async Task RemoveFails()
        {
            Assert.IsFalse((await Authorize.RemoveAsync(_invalid, _ctx)).HasSucceeded());
        }

        [TestMethod]
        public void Collection()
        {
            var list = new List<MockEntity>()
            {
                _valid, _invalid, _invisible
            }.AsQueryable();

            var authorizedCollection = Authorize.Collection(list, _ctx);

            Assert.IsTrue(authorizedCollection.All(a => a.IsVisible && a.IsVisible2));
        }

        [TestMethod]
        public async Task InterfacedCreate()
        {
            var result = await Authorize.CreateAsync(_validMockImplementedEntity, _ctx);
            Assert.IsTrue(result.HasSucceeded());
        }

        [TestMethod]
        public async Task InterfacedCreateFail()
        {
            var result = await Authorize.CreateAsync(_invalidMockImplementedEntity, _ctx);
            Assert.IsFalse(result.HasSucceeded());
        }


        [TestMethod]
        public async Task InterfacedModify()
        {
            var result = await Authorize.ModifyAsync(_validMockImplementedEntity, _validMockImplementedEntity, _ctx);
            Assert.IsTrue(result.HasSucceeded());
        }

        [TestMethod]
        public async Task InterfacedModifyFail()
        {
            var result = await Authorize.ModifyAsync(_validMockImplementedEntity, _invalidMockImplementedEntity, _ctx);
            Assert.IsFalse(result.HasSucceeded());
        }


        [TestMethod]
        public async Task InterfacedRemove()
        {
            var result = await Authorize.RemoveAsync(_validMockImplementedEntity, _ctx);
            Assert.IsTrue(result.HasSucceeded());
        }

        [TestMethod]
        public async Task InterfacedRemovefail()
        {
            var result = await Authorize.RemoveAsync(_invalidMockImplementedEntity, _ctx);
            Assert.IsFalse(result.HasSucceeded());
        }

        [TestMethod]
        public async Task InterfacedCollection()
        {
            var collection = new List<MockImplementedEntity>()
            {
                _validMockImplementedEntity,
                _invalidMockImplementedEntity,
                _invisibleMockImplementedEntity
            };

            var authorized = await Authorize.CollectionAsync(collection.AsQueryable(), _ctx);
            Assert.IsTrue(authorized.Any(a=>a.IsVisible));

        }

    }
}
