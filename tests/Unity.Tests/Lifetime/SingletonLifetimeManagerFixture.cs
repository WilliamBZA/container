using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Unity.Lifetime;

namespace Unity.Container.Tests.Lifetime
{
    [TestClass]
    public class SingletonLifetimeManagerFixture
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
        }

        #endregion

        // TODO: Cover all execution paths
        #region Tests

        [TestMethod]
        public void Container_Lifetime_SingletonLifetimeManager_ResolvingInParent()
        {
            _parentContainer.RegisterType<TestClass>(new SingletonLifetimeManager());
            var o1 = _parentContainer.Resolve<TestClass>();
            var o2 = _parentContainer.Resolve<TestClass>();
            Assert.IsNotNull(o1);
            Assert.AreSame(o1, o2);
        }

        [TestMethod]
        public void Container_Lifetime_SingletonLifetimeManager_Resolve()
        {
            _parentContainer.RegisterType<TestClass>(new SingletonLifetimeManager());
            var o1 = _parentContainer.Resolve<TestClass>();
            var o2 = _child1.Resolve<TestClass>();
            Assert.IsNotNull(o1);
            Assert.AreSame(o1, o2);
        }

        [TestMethod]
        public void Container_Lifetime_SingletonLifetimeManager_ChildRegistrationResolve()
        {
            _child1.RegisterType<TestClass>(new SingletonLifetimeManager());
            var o1 = _parentContainer.Resolve<TestClass>();
            var o2 = _child1.Resolve<TestClass>();
            Assert.IsNotNull(o1);
            Assert.AreSame(o1, o2);
        }

        [TestMethod]
        public void Container_Lifetime_SingletonLifetimeManager_ChildResolve()
        {
            _parentContainer.RegisterType<TestClass>(new SingletonLifetimeManager());
            var o1 = _child1.Resolve<TestClass>();
            var o2 = _child1.Resolve<TestClass>();
            Assert.IsNotNull(o1);
            Assert.AreSame(o1, o2);
        }

        [TestMethod]
        public void Container_Lifetime_SingletonLifetimeManager_Siblings()
        {
            _parentContainer.RegisterType<TestClass>(new SingletonLifetimeManager());
            var o1 = _child1.Resolve<TestClass>();
            var o2 = _child2.Resolve<TestClass>();
            Assert.IsNotNull(o1);
            Assert.AreSame(o1, o2);
        }

        [TestMethod]
        public void Container_Lifetime_SingletonLifetimeManager_SiblingsChild1()
        {
            _child1.RegisterType<TestClass>(new SingletonLifetimeManager());
            var o1 = _child1.Resolve<TestClass>();
            var o2 = _child2.Resolve<TestClass>();
            Assert.AreSame(o1, o2);
        }

        [TestMethod]
        public void Container_Lifetime_SingletonLifetimeManager_SiblingsChild2()
        {
            _child2.RegisterType<TestClass>(new SingletonLifetimeManager());
            var o1 = _child1.Resolve<TestClass>();
            var o2 = _child2.Resolve<TestClass>();
            Assert.IsNotNull(o1);
            Assert.AreSame(o1, o2);
        }

        [TestMethod]
        public void Container_Lifetime_SingletonLifetimeManager_Disposing()
        {
            _parentContainer.RegisterType<TestClass>(new SingletonLifetimeManager());
            var o1 = _parentContainer.Resolve<TestClass>();
            var o2 = _child1.Resolve<TestClass>();

            Assert.IsNotNull(o1);
            Assert.IsNotNull(o2);

            _child1.Dispose();
            Assert.IsFalse(o1.Disposed);
            Assert.IsTrue(o2.Disposed);
        }

        [TestMethod]
        public void Container_Lifetime_SingletonLifetimeManager_DisposingChild()
        {
            _child1.RegisterType<TestClass>(new SingletonLifetimeManager());
            var o1 = _parentContainer.Resolve<TestClass>();
            var o2 = _child1.Resolve<TestClass>();

            Assert.IsNotNull(o1);
            Assert.IsNotNull(o2);

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
