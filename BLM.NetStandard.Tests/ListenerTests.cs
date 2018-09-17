using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLM.NetStandard.Interfaces;
using BLM.NetStandard.Interfaces.Listen;
using System.Security.Principal;

#pragma warning disable 1998
#pragma warning disable IDE1006 // Naming Styles
// ReSharper disable UnusedMember.Global
// ReSharper disable StaticMemberInGenericType

namespace BLM.NetStandard.Tests
{
    #region GENERIC Listener
    public class Listener<T> : IListenCreated<T>, IListenCreateFailed<T>,
        IListenModified<T>, IListenModificationFailed<T>,
        IListenRemoved<T>, IListenRemoveFailed<T>
    {
        public static bool WasOnCreatedCalled;
        public static bool WasOnCreationValidationFailedCalled;
        public static bool WasOnModifiedCalled;
        public static bool? EntitesWereNotTheSameOnCalling;
        public static bool WasOnModificationFailedCalled;
        public static bool WasOnDeletedCalled;
        public static bool WasOnDeletionFailedCalled;

        public virtual async Task OnCreatedAsync(T entity, IContextInfo user)
        {
            WasOnCreatedCalled = true;
        }

        public virtual async Task OnCreateFailedAsync(T entity, IContextInfo user)
        {
            WasOnCreationValidationFailedCalled = true;
        }

        public virtual async Task OnModifiedAsync(T original, T modified, IContextInfo user)
        {
            if (original != null)
            {
                EntitesWereNotTheSameOnCalling = !original.Equals(modified);
            }
            else if (modified != null)
            {
                EntitesWereNotTheSameOnCalling = true;
            }
            else
            {
                EntitesWereNotTheSameOnCalling = false;
            }
            WasOnModifiedCalled = true;
        }
        public virtual async Task OnModificationFailedAsync(T original, T modified, IContextInfo user)
        {
            if (original != null)
            {
                EntitesWereNotTheSameOnCalling = !original.Equals(modified);
            }
            else if (modified != null)
            {
                EntitesWereNotTheSameOnCalling = !modified.Equals(original);
            }
            else
            {
                EntitesWereNotTheSameOnCalling = false;
            }
            WasOnModificationFailedCalled = true;
        }


        public virtual async Task OnRemovedAsync(T entity, IContextInfo user)
        {
            WasOnDeletedCalled = true;
        }


        public virtual async Task OnRemoveFailedAsync(T entity, IContextInfo user)
        {
            WasOnDeletionFailedCalled = true;
        }

        public static void Reset()
        {
            WasOnCreatedCalled = false;
            WasOnCreationValidationFailedCalled = false;
            WasOnModifiedCalled = false;
            WasOnModificationFailedCalled = false;
            WasOnDeletedCalled = false;
            WasOnDeletionFailedCalled = false;

            EntitesWereNotTheSameOnCalling = null;
        }
    }
    #endregion

    public class MockListener : Listener<MockEntity> { }

    public class MockListener2 : Listener<MockEntity> { }

    public class ObjectListener : Listener<object> { }


    [TestClass]
    public class ListenerTests
    {

        MockEntity ent = new MockEntity();
        MockEntity ent2 = new MockEntity()
        {
            Guid = Guid.NewGuid().ToString()
        };
        GenericContextInfo ctx = new GenericContextInfo(new GenericIdentity("gallayb"));

        [TestMethod]
        public async Task Created()
        {
            MockListener.Reset();
            await Listen.CreatedAsync(ent, ctx);

            Assert.IsTrue(MockListener.WasOnCreatedCalled);
            Assert.IsFalse(MockListener.WasOnCreationValidationFailedCalled);
            Assert.IsFalse(MockListener.WasOnModifiedCalled);
            Assert.IsFalse(MockListener.WasOnModificationFailedCalled);
            Assert.IsFalse(MockListener.WasOnDeletedCalled);
            Assert.IsFalse(MockListener.WasOnDeletionFailedCalled);
        }

        [TestMethod]
        public async Task CreateFailed()
        {
            MockListener.Reset();
            await Listen.CreateFailedAsync(ent, ctx);

            Assert.IsFalse(MockListener.WasOnCreatedCalled);
            Assert.IsTrue(MockListener.WasOnCreationValidationFailedCalled);
            Assert.IsFalse(MockListener.WasOnModifiedCalled);
            Assert.IsFalse(MockListener.WasOnModificationFailedCalled);
            Assert.IsFalse(MockListener.WasOnDeletedCalled);
            Assert.IsFalse(MockListener.WasOnDeletionFailedCalled);
        }

        [TestMethod]
        public async Task Modified()
        {
            MockListener.Reset();
            ObjectListener.Reset();

            await Listen.ModifiedAsync(ent, ent2, ctx);

            Assert.IsFalse(MockListener.WasOnCreatedCalled);
            Assert.IsFalse(MockListener.WasOnCreationValidationFailedCalled);
            Assert.IsTrue(MockListener.WasOnModifiedCalled);
            Assert.IsFalse(MockListener.WasOnModificationFailedCalled);
            Assert.IsFalse(MockListener.WasOnDeletedCalled);
            Assert.IsFalse(MockListener.WasOnDeletionFailedCalled);

            Assert.IsTrue(MockListener.EntitesWereNotTheSameOnCalling.HasValue && MockListener.EntitesWereNotTheSameOnCalling.Value);
        }

        /// <summary>
        /// Test the 3 different modify listeners, all of them need to run, and we pass two different entities into them
        /// </summary>
        [TestMethod]
        public async Task MultipleModifiedListeners()
        {
            MockListener.Reset();
            MockListener2.Reset();
            ObjectListener.Reset();

            await Listen.ModifiedAsync(ent, ent2, ctx);

            Assert.IsFalse(MockListener.WasOnCreatedCalled);
            Assert.IsFalse(MockListener.WasOnCreationValidationFailedCalled);
            Assert.IsTrue(MockListener.WasOnModifiedCalled);
            Assert.IsFalse(MockListener.WasOnModificationFailedCalled);
            Assert.IsFalse(MockListener.WasOnDeletedCalled);
            Assert.IsFalse(MockListener.WasOnDeletionFailedCalled);

            Assert.IsFalse(MockListener2.WasOnCreatedCalled);
            Assert.IsFalse(MockListener2.WasOnCreationValidationFailedCalled);
            Assert.IsTrue(MockListener2.WasOnModifiedCalled);
            Assert.IsFalse(MockListener2.WasOnModificationFailedCalled);
            Assert.IsFalse(MockListener2.WasOnDeletedCalled);
            Assert.IsFalse(MockListener2.WasOnDeletionFailedCalled);

            Assert.IsFalse(ObjectListener.WasOnCreatedCalled);
            Assert.IsFalse(ObjectListener.WasOnCreationValidationFailedCalled);
            Assert.IsTrue(ObjectListener.WasOnModifiedCalled);
            Assert.IsFalse(ObjectListener.WasOnModificationFailedCalled);
            Assert.IsFalse(ObjectListener.WasOnDeletedCalled);
            Assert.IsFalse(ObjectListener.WasOnDeletionFailedCalled);

            Assert.IsTrue(MockListener.EntitesWereNotTheSameOnCalling.HasValue && MockListener.EntitesWereNotTheSameOnCalling.Value);
            Assert.IsTrue(MockListener2.EntitesWereNotTheSameOnCalling.HasValue && MockListener2.EntitesWereNotTheSameOnCalling.Value);
            Assert.IsTrue(ObjectListener.EntitesWereNotTheSameOnCalling.HasValue && ObjectListener.EntitesWereNotTheSameOnCalling.Value);
        }

        /// <summary>
        /// Only the object modify listener should run, because we pass two (different) objects into the listener
        /// </summary>
        [TestMethod]
        public async Task MultipleModifiedListeners_TestInheritance()
        {
            MockListener.Reset();
            MockListener2.Reset();
            ObjectListener.Reset();

            await Listen.ModifiedAsync(new object(), new { sajt = true }, ctx);

            Assert.IsFalse(MockListener.WasOnCreatedCalled);
            Assert.IsFalse(MockListener.WasOnCreationValidationFailedCalled);
            Assert.IsFalse(MockListener.WasOnModifiedCalled);
            Assert.IsFalse(MockListener.WasOnModificationFailedCalled);
            Assert.IsFalse(MockListener.WasOnDeletedCalled);
            Assert.IsFalse(MockListener.WasOnDeletionFailedCalled);

            Assert.IsFalse(MockListener2.WasOnCreatedCalled);
            Assert.IsFalse(MockListener2.WasOnCreationValidationFailedCalled);
            Assert.IsFalse(MockListener2.WasOnModifiedCalled);
            Assert.IsFalse(MockListener2.WasOnModificationFailedCalled);
            Assert.IsFalse(MockListener2.WasOnDeletedCalled);
            Assert.IsFalse(MockListener2.WasOnDeletionFailedCalled);

            Assert.IsFalse(ObjectListener.WasOnCreatedCalled);
            Assert.IsFalse(ObjectListener.WasOnCreationValidationFailedCalled);
            Assert.IsTrue(ObjectListener.WasOnModifiedCalled);
            Assert.IsFalse(ObjectListener.WasOnModificationFailedCalled);
            Assert.IsFalse(ObjectListener.WasOnDeletedCalled);
            Assert.IsFalse(ObjectListener.WasOnDeletionFailedCalled);

            Assert.IsNull(MockListener.EntitesWereNotTheSameOnCalling);
            Assert.IsNull(MockListener2.EntitesWereNotTheSameOnCalling);
            Assert.IsTrue(ObjectListener.EntitesWereNotTheSameOnCalling.HasValue && ObjectListener.EntitesWereNotTheSameOnCalling.Value);
        }

        /// <summary>
        /// A little loadtest like something for check the performance is OK
        /// </summary>
        [TestMethod]
        public async Task LoadTest()
        {
            MockListener.Reset();
            MockListener2.Reset();
            ObjectListener.Reset();

            var start = DateTime.Now;

            List<Task> tasks = new List<Task>();

            for (int i = 0; i < 500000; i++)
            {
                tasks.Add(Listen.ModifiedAsync(ent, ent, ctx));
            }

            Task.WaitAll(tasks.ToArray());

            var time = DateTime.Now.Subtract(start).TotalMilliseconds;

            Assert.IsTrue(time < 1500, "time < 1500");

            Assert.IsFalse(MockListener.WasOnCreatedCalled);
            Assert.IsFalse(MockListener.WasOnCreationValidationFailedCalled);
            Assert.IsTrue(MockListener.WasOnModifiedCalled);
            Assert.IsFalse(MockListener.WasOnModificationFailedCalled);
            Assert.IsFalse(MockListener.WasOnDeletedCalled);
            Assert.IsFalse(MockListener.WasOnDeletionFailedCalled);

            Assert.IsFalse(MockListener2.WasOnCreatedCalled);
            Assert.IsFalse(MockListener2.WasOnCreationValidationFailedCalled);
            Assert.IsTrue(MockListener2.WasOnModifiedCalled);
            Assert.IsFalse(MockListener2.WasOnModificationFailedCalled);
            Assert.IsFalse(MockListener2.WasOnDeletedCalled);
            Assert.IsFalse(MockListener2.WasOnDeletionFailedCalled);


            Assert.IsFalse(ObjectListener.WasOnCreatedCalled);
            Assert.IsFalse(ObjectListener.WasOnCreationValidationFailedCalled);
            Assert.IsTrue(ObjectListener.WasOnModifiedCalled);
            Assert.IsFalse(ObjectListener.WasOnModificationFailedCalled);
            Assert.IsFalse(ObjectListener.WasOnDeletedCalled);
            Assert.IsFalse(ObjectListener.WasOnDeletionFailedCalled);
        }

        [TestMethod]
        public async Task ModificationFailed()
        {
            MockListener.Reset();
            await Listen.ModificationFailedAsync(ent, ent, ctx);

            Assert.IsFalse(MockListener.WasOnCreatedCalled);
            Assert.IsFalse(MockListener.WasOnCreationValidationFailedCalled);
            Assert.IsFalse(MockListener.WasOnModifiedCalled);
            Assert.IsTrue(MockListener.WasOnModificationFailedCalled);
            Assert.IsFalse(MockListener.WasOnDeletedCalled);
            Assert.IsFalse(MockListener.WasOnDeletionFailedCalled);
        }

        [TestMethod]
        public async Task Removed()
        {
            MockListener.Reset();
            await Listen.RemovedAsync(ent, ctx);

            Assert.IsFalse(MockListener.WasOnCreatedCalled);
            Assert.IsFalse(MockListener.WasOnCreationValidationFailedCalled);
            Assert.IsFalse(MockListener.WasOnModifiedCalled);
            Assert.IsFalse(MockListener.WasOnModificationFailedCalled);
            Assert.IsTrue(MockListener.WasOnDeletedCalled);
            Assert.IsFalse(MockListener.WasOnDeletionFailedCalled);
        }

        [TestMethod]
        public async Task RemoveFailed()
        {
            MockListener.Reset();
            await Listen.RemoveFailedAsync(ent, ctx);

            Assert.IsFalse(MockListener.WasOnCreatedCalled);
            Assert.IsFalse(MockListener.WasOnCreationValidationFailedCalled);
            Assert.IsFalse(MockListener.WasOnModifiedCalled);
            Assert.IsFalse(MockListener.WasOnModificationFailedCalled);
            Assert.IsFalse(MockListener.WasOnDeletedCalled);
            Assert.IsTrue(MockListener.WasOnDeletionFailedCalled);
        }
    }
}
// ReSharper restore UnusedMember.Global
// ReSharper restore StaticMemberInGenericType
