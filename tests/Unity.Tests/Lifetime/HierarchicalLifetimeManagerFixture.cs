using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity.Lifetime;

namespace Unity.Container.Tests.Lifetime
{
    [TestClass]
    public class HierarchicalLifetimeManagerFixture
    {
        #region Setup

        private IUnityContainer _child1;
        private IUnityContainer _child2;
        private IUnityContainer _parentContainer;

        [TestInitialize]
        public void Setup()
        {
            _parentContainer = new UnityContainer();
            _child1 = _parentContainer.CreateChildContainer();
            _child2 = _parentContainer.CreateChildContainer();

            _parentContainer.RegisterType<TestClass>(new HierarchicalLifetimeManager());
        }

        #endregion


        #region Tests

        [TestMethod]
        public void Container_Lifetime_HierarchicalLifetimeManager_ResolvingInParent()
        {
            var o1 = _parentContainer.Resolve<TestClass>();
            var o2 = _parentContainer.Resolve<TestClass>();
            Assert.AreSame(o1, o2);
        }

        [TestMethod]
        public void Container_Lifetime_HierarchicalLifetimeManager_ParentAndChildResolve()
        {
            var o1 = _parentContainer.Resolve<TestClass>();
            var o2 = _child1.Resolve<TestClass>();
            Assert.AreNotSame(o1, o2);
        }

        [TestMethod]
        public void Container_Lifetime_HierarchicalLifetimeManager_ChildResolve()
        {
            var o1 = _child1.Resolve<TestClass>();
            var o2 = _child1.Resolve<TestClass>();
            Assert.AreSame(o1, o2);
        }

        [TestMethod]
        public void Container_Lifetime_HierarchicalLifetimeManager_SiblingContainersResolve()
        {
            var o1 = _child1.Resolve<TestClass>();
            var o2 = _child2.Resolve<TestClass>();
            Assert.AreNotSame(o1, o2);
        }

        [TestMethod]
        public void Container_Lifetime_HierarchicalLifetimeManager_DisposingOfChildContainer()
        {
            var o1 = _parentContainer.Resolve<TestClass>();
            var o2 = _child1.Resolve<TestClass>();

            _child1.Dispose();
            Assert.IsFalse(o1.Disposed);
            Assert.IsTrue(o2.Disposed);
        }

        #endregion


        #region Test Data

        public class TestClass : IDisposable
        {
            public bool Disposed { get; private set; }

            public void Dispose()
            {
                Disposed = true;
            }
        }

        #endregion
    }
}
