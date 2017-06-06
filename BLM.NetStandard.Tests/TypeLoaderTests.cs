using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BLM.NetStandard.Interfaces;
using BLM.NetStandard.Interfaces.Authorize;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BLM.NetStandard.Tests
{
    class DummyClass
    {
        public int Id { get; set; }
    }

    class DummyAuthorizeCollection : AuthorizeCollection<DummyClass>
    {
        public override async Task<IQueryable<DummyClass>> AuthorizeCollectionAsync(IQueryable<DummyClass> entities, IContextInfo ctx)
        {
            return await Task.Factory.StartNew(() => entities);
        }
    }

    [TestClass]
    public class TypeLoaderTests
    {
        [TestMethod]
        public void LoadTypes()
        {
            var types = Loader.Types;
            Assert.IsNotNull(types);
            Assert.IsTrue(types.All(t => t.GetInterfaces().Contains(typeof(IBlmEntry))));
        }

        [TestMethod]
        public void GetInstance()
        {
            var instance = Loader.GetInstance<DummyAuthorizeCollection>();
            Assert.IsNotNull(instance);

            // And... again from cache
            var instance2 = Loader.GetInstance<DummyAuthorizeCollection>();
            Assert.IsNotNull(instance2);
        }

        [TestMethod]
        public void GetEntriesForType()
        {
            var entryList = Loader.GetEntriesFor<AuthorizeCollection<DummyClass>>();
            Assert.IsNotNull(entryList);

            foreach (var instance in entryList)
            {
                Assert.IsInstanceOfType(instance, typeof(AuthorizeCollection<DummyClass>));
            }

            // And...again from cache
            var entryList2 = Loader.GetEntriesFor<AuthorizeCollection<DummyClass>>();
            Assert.IsNotNull(entryList2);

            foreach (var instance in entryList2)
            {
                Assert.IsInstanceOfType(instance, typeof(AuthorizeCollection<DummyClass>));

            }
        }
    }
}
