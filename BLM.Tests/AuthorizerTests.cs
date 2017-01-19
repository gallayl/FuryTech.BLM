using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using BLM.Extensions;
using BLM.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BLM.Tests
{
    [TestClass]
    public class AuthorizerTests
    {
        private MockEntity valid = new MockEntity()
        {
            Id = 1,
            IsValid = true,
            IsVisible = true,
            IsVisible2 = true

        };

        private MockEntity invalid = new MockEntity()
        {
            Id = 2,
            IsValid = false,
            IsVisible = true,
            IsVisible2 = false
        };

        private MockEntity invisible = new MockEntity()
        {
            Id = 3,
            IsValid = true,
            IsVisible = false,
            IsVisible2 = true
        };

        IContextInfo ctx = new GenericContextInfo(Thread.CurrentPrincipal.Identity);

        [TestMethod]
        public async Task CreateSuccess()
        {
            Assert.IsTrue((await Authorize.CreateAsync(valid, ctx)).HasSucceeded());
        }

        [TestMethod]
        public async Task CreateFail()
        {
            Assert.IsFalse((await Authorize.CreateAsync(invalid, ctx)).HasSucceeded());
        }

        [TestMethod]
        public async Task Modify()
        {
            Assert.IsTrue((await Authorize.ModifyAsync(valid, valid, ctx)).HasSucceeded());

        }

        [TestMethod]
        public async Task ModifyFails()
        {
            Assert.IsFalse((await Authorize.ModifyAsync(invalid, invalid, ctx)).HasSucceeded());
        }

        [TestMethod]
        public async Task Remove()
        {
            Assert.IsTrue((await Authorize.RemoveAsync(valid, ctx)).HasSucceeded());

        }

        [TestMethod]
        public async Task RemoveFails()
        {
            Assert.IsFalse((await Authorize.RemoveAsync(invalid, ctx)).HasSucceeded());
        }

        [TestMethod]
        public void AuthorizeCollection()
        {
            var list = new List<MockEntity>()
            {
                valid, invalid, invisible
            }.AsQueryable();

            var authorizedCollection = Authorize.Collection(list, ctx);

            Assert.IsTrue(authorizedCollection.All(a => a.IsVisible && a.IsVisible2));
        }
    }
}
