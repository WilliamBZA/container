using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity.Lifetime;

namespace Unity.Container.Tests.Hierarchy
{
    #region Tests

    [TestClass]
    public class ContainerHierarchyFixture
    {
        /// <summary>
        /// create parent and child container and then get the parent from child using the property parent.
        /// </summary>
        [TestMethod]
        public void CheckParentContOfChild()
        {
            var uc = new UnityContainer();
            var ucchild = uc.CreateChildContainer();
    
            object obj = ucchild.Parent;
            
            Assert.AreSame(uc, obj);
        }

        /// <summary>
        /// Check what do we get when we ask for parent's parent container
        /// </summary>
        [TestMethod]
        public void Container_Hierarchy_CheckParentContOfParent()
        {
            var uc = new UnityContainer();
            var ucchild = uc.CreateChildContainer();
            
            object obj = uc.Parent;
            
            Assert.IsNull(obj);
        }

        /// <summary>
        /// Check whether child inherits the configuration of the parent container or not using registertype method
        /// </summary>
        [TestMethod]
        public void Container_Hierarchy_ChildInheritsParentsConfiguration_RegisterTypeResolve()
        {
            var parent = new UnityContainer();
            parent.RegisterType<ITestService, TestService>(new ContainerControlledLifetimeManager());

            var child = parent.CreateChildContainer();
            var objtest = child.Resolve<ITestService>();

            Assert.IsNotNull(objtest);
            Assert.IsInstanceOfType(objtest, typeof(TestService));
        }

        /// <summary>
        /// Check whether child inherits the configuration of the parent container or 
        /// not, using register instance method
        /// </summary>
        [TestMethod]
        public void Container_Hierarchy_ChildInheritsParentsConfiguration_RegisterInstanceResolve()
        {
            var parent = new UnityContainer();
            var obj = new TestService();
            
            parent.RegisterInstance<ITestService>("InParent", obj);

            IUnityContainer child = parent.CreateChildContainer();
            ITestService objtest = child.Resolve<ITestService>("InParent");

            Assert.IsNotNull(objtest);
            Assert.AreSame(objtest, obj);
        }

        /// <summary>
        /// Check whether child inherits the configuration of the parent container or 
        /// not,using register type method and then resolve all
        /// </summary>
        [TestMethod]
        public void Container_Hierarchy_ChildInheritsParentsConfiguration_RegisterTypeResolveAll()
        {
            var parent = new UnityContainer();
            parent.RegisterType<ITestService, TestService>()
                .RegisterType<ITestService, TestService1>("first")
                .RegisterType<ITestService, TestService2>("second");

            var child = parent.CreateChildContainer()
                .RegisterType<ITestService, TestService3>("third");

            var list = new List<ITestService>(child.ResolveAll<ITestService>());
            
            Assert.AreEqual(3, list.Count);
        }

        /// <summary>
        /// Check whether child inherits the configuration of the parent container or 
        /// not, Using register instance method and then resolve all
        /// </summary>
        [TestMethod]
        public void Container_Hierarchy_ChildInheritsParentsConfiguration_RegisterInstanceResolveAll()
        {
            var objdefault = new TestService();
            var objfirst = new TestService1();
            var objsecond = new TestService2();
            var objthird = new TestService3();
            var  parent = new UnityContainer();
            
            parent.RegisterInstance<ITestService>(objdefault)
                .RegisterInstance<ITestService>("first", objfirst)
                .RegisterInstance<ITestService>("second", objsecond);

            var child = parent.CreateChildContainer()
                .RegisterInstance<ITestService>("third", objthird);

            var list = new List<ITestService>(child.ResolveAll<ITestService>());
            
            Assert.AreEqual(3, list.Count);
        }

        /// <summary>
        /// Register same type in parent and child and see the behavior
        /// </summary>
        [TestMethod]
        public void Container_Hierarchy_RegisterSameTypeInChildAndParentOverriden()
        {
            var parent = new UnityContainer();
            parent.RegisterType<ITestService, TestService>();
            var child = parent.CreateChildContainer()
                .RegisterType<ITestService, TestService1>();

            var parentregister = parent.Resolve<ITestService>();
            var childregister = child.Resolve<ITestService>();

            Assert.IsInstanceOfType(parentregister, typeof(TestService));
            Assert.IsInstanceOfType(childregister, typeof(TestService1));
        }

        /// <summary>
        /// Register type in parent and resolve using child.
        /// Change in parent and changes reflected in child.
        /// </summary>
        [TestMethod]
        public void Container_Hierarchy_ChangeInParentConfigurationIsReflectedInChild()
        {
            var parent = new UnityContainer();
            parent.RegisterType<ITestService, TestService>();
            var child = parent.CreateChildContainer();

            var first = child.Resolve<ITestService>();
            parent.RegisterType<ITestService, TestService1>();
            var second = child.Resolve<ITestService>();

            Assert.IsInstanceOfType(first, typeof(TestService));
            Assert.IsInstanceOfType(second, typeof(TestService1));
        }

        /// <summary>
        /// dispose parent container, child should get disposed.
        /// </summary>
        [TestMethod]
        public void Container_Hierarchy_WhenDisposingParentChildDisposes()
        {
            var parent = new UnityContainer();
            var child = parent.CreateChildContainer();

            var obj = new TestService3();
            child.RegisterInstance<TestService3>(obj);

            parent.Dispose();
            Assert.IsTrue(obj.WasDisposed);
        }

        /// <summary>
        /// dispose child, check if parent is disposed or not.
        /// </summary>
        [TestMethod]
        public void Container_Hierarchy_ParentNotDisposedWhenChildDisposed()
        {
            var parent = new UnityContainer();
            var child = parent.CreateChildContainer();
            var obj1 = new TestService();
            var obj3 = new TestService3();
            parent.RegisterInstance(obj1);
            child.RegisterInstance(obj3);

            child.Dispose();
            //parent not getting disposed
            Assert.IsFalse(obj1.WasDisposed);

            //child getting disposed.
            Assert.IsTrue(obj3.WasDisposed);
        }

        [TestMethod]
        public void Container_Hierarchy_ChainOfContainers()
        {
            var parent = new UnityContainer();
            var child1 = parent.CreateChildContainer();
            var child2 = child1.CreateChildContainer();
            var child3 = child2.CreateChildContainer();

            var obj1 = new TestService();

            parent.RegisterInstance("InParent", obj1);
            child1.RegisterInstance("InChild1", obj1);
            child2.RegisterInstance("InChild2", obj1);
            child3.RegisterInstance("InChild3", obj1);

            object objresolve = child3.Resolve<TestService>("InParent");
            object objresolve1 = parent.Resolve<TestService>("InChild3");

            Assert.AreSame(obj1, objresolve);
            
            child1.Dispose();
            
            //parent not getting disposed
            Assert.IsTrue(obj1.WasDisposed);
        }

        #endregion


        #region Test Data

        public interface ITestService { }

        public class TestService : ITestService, IDisposable
        {
            private bool wasDisposed = false;

            public bool WasDisposed
            {
                get { return wasDisposed; }
                set { wasDisposed = value; }
            }

            public void Dispose()
            {
                wasDisposed = true;
            }
        }

        public class TestService1 : ITestService { }

        public class TestService2 : ITestService { }

        public class TestService3 : ITestService, IDisposable
        {
            private bool wasDisposed = false;

            public bool WasDisposed
            {
                get { return wasDisposed; }
                set { wasDisposed = value; }
            }

            public void Dispose()
            {
                wasDisposed = true;
            }
        }

        #endregion
    }
}