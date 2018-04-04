using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity.Lifetime;

namespace Unity.Container.Tests.Lifetime
{
    #region Tests

    /// <summary>
    /// Summary description for MyTest
    /// </summary>
    [TestClass]
    public class ExternallyControlledLifetimeManagerFixture
    {
        /// <summary>
        /// Registering a type as singleton and handling its lifetime. Using SetLifetime method.
        /// </summary>
        [TestMethod]
        public void Container_Lifetime_ExternallyControlledLifetimeManager_SetLifetimeTwice()
        {
            IUnityContainer uc = new UnityContainer();
            
            uc.RegisterType<A>(new ContainerControlledLifetimeManager())
                .RegisterType<A>("hello", new ExternallyControlledLifetimeManager());
            A obj = uc.Resolve<A>();
            A obj1 = uc.Resolve<A>("hello");
            
            Assert.AreNotSame(obj, obj1);
        }

        /// <summary>
        /// SetSingleton class A. Then register instance of class A once by default second by name and
        /// again register instance by another name with lifetime control as false.
        /// Then SetLifetime once default and then by name.
        /// </summary>
        [TestMethod]
        public void Container_Lifetime_ExternallyControlledLifetimeManager_Twice()
        {
            IUnityContainer uc = new UnityContainer();

            A aInstance = new A();
            uc.RegisterInstance(aInstance)
                .RegisterInstance("hello", aInstance)
                .RegisterInstance("hi", aInstance, new ExternallyControlledLifetimeManager());

            A obj = uc.Resolve<A>();
            A obj1 = uc.Resolve<A>("hello");
            A obj2 = uc.Resolve<A>("hi");

            Assert.AreSame(obj, obj1);
            Assert.AreSame(obj1, obj2);
        }

        /// <summary>
        /// SetLifetime class A. Then register instance of class A once by default second by name and
        /// again register instance by another name with lifetime control as false.
        /// Then SetLifetime once default and then by name.
        /// </summary>
        [TestMethod]
        public void Container_Lifetime_ExternallyControlledLifetimeManager_SingletonDiffNames()
        {
            IUnityContainer uc = new UnityContainer();

            A aInstance = new A();
            uc.RegisterType<A>(new ContainerControlledLifetimeManager())
                .RegisterInstance(aInstance)
                .RegisterInstance("hello", aInstance)
                .RegisterInstance("hi", aInstance, new ExternallyControlledLifetimeManager());

            A obj = uc.Resolve<A>();
            A obj1 = uc.Resolve<A>("hello");
            A obj2 = uc.Resolve<A>("hi");
            
            Assert.AreSame(obj, obj1);
            Assert.AreSame(obj1, obj2);
        }

        /// <summary>
        /// SetSingleton class A with name. Then register instance of class A once by default second by name and
        /// again register instance by another name with lifetime control as false.
        /// Then SetLifetime once default and then by name.
        /// </summary>
        [TestMethod]
        public void Container_Lifetime_ExternallyControlledLifetimeManager_DiffNames()
        {
            IUnityContainer uc = new UnityContainer();

            A aInstance = new A();
            uc.RegisterType<A>("set", new ContainerControlledLifetimeManager())
                .RegisterInstance(aInstance)
                .RegisterInstance("hello", aInstance)
                .RegisterInstance("hi", aInstance, new ExternallyControlledLifetimeManager());

            A obj = uc.Resolve<A>("set");
            A obj1 = uc.Resolve<A>("hello");
            A obj2 = uc.Resolve<A>("hi");
            
            Assert.AreNotSame(obj, obj1);
            Assert.AreSame(obj1, obj2);
            Assert.AreSame(aInstance, obj1);
        }

        /// <summary>
        /// SetLifetime class A with name. Then register instance of class A once by default second by name and
        /// again register instance by another name with lifetime control as false.
        /// Then SetLifetime once default and then by name.
        /// </summary>
        [TestMethod]
        public void Container_Lifetime_ExternallyControlledLifetimeManager_InstanceDiffNames()
        {
            IUnityContainer uc = new UnityContainer();

            A aInstance = new A();
            uc.RegisterType<A>("set", new ContainerControlledLifetimeManager())
                .RegisterInstance(aInstance)
                .RegisterInstance("hello", aInstance)
                .RegisterInstance("hi", aInstance, new ExternallyControlledLifetimeManager());

            A obj = uc.Resolve<A>("set");
            A obj1 = uc.Resolve<A>("hello");
            A obj2 = uc.Resolve<A>("hi");
            
            Assert.AreNotSame(obj, obj1);
            Assert.AreSame(aInstance, obj1);
            Assert.AreSame(obj1, obj2);
        }

        /// <summary>
        /// SetSingleton class A. Then register instance of class A once by default second by name and
        /// lifetime as true. Then again register instance by another name with lifetime control as true
        /// then register.
        /// Then SetLifetime once default and then by name.
        /// </summary>
        [TestMethod]
        public void Container_Lifetime_ExternallyControlledLifetimeManager_AandBWithSameName()
        {
            IUnityContainer uc = new UnityContainer();

            A aInstance = new A();
            B bInstance = new B();
            uc.RegisterType<A>(new ContainerControlledLifetimeManager())
                .RegisterInstance(aInstance)
                .RegisterInstance("hello", aInstance)
                .RegisterInstance("hi", bInstance)
                .RegisterInstance("hello", bInstance, new ExternallyControlledLifetimeManager());

            A obj = uc.Resolve<A>();
            A obj1 = uc.Resolve<A>("hello");
            B obj2 = uc.Resolve<B>("hello");
            B obj3 = uc.Resolve<B>("hi");
            
            Assert.AreSame(obj, obj1);
            Assert.AreNotSame(obj, obj2);
            Assert.AreNotSame(obj1, obj2);
            Assert.AreSame(obj2, obj3);
        }

        /// <summary>
        /// Register class A as singleton then use RegisterInstance to register instance 
        /// of class A.
        /// </summary>
        [TestMethod]
        public void Container_Lifetime_ExternallyControlledLifetimeManager_Instance()
        {
            IUnityContainer uc = new UnityContainer();

            var aInstance = new ATest();
            uc.RegisterType(typeof(ATest), new ContainerControlledLifetimeManager());
            uc.RegisterType<ATest>("SetA", new ContainerControlledLifetimeManager());
            uc.RegisterInstance(aInstance);
            uc.RegisterInstance("hello", aInstance);
            uc.RegisterInstance("hello", aInstance, new ExternallyControlledLifetimeManager());

            var obj =  uc.Resolve<ATest>();
            var obj1 = uc.Resolve<ATest>("SetA");
            var obj2 = uc.Resolve<ATest>("hello");

            Assert.AreNotSame(obj, obj1);
            Assert.AreSame(obj, obj2);
        }

        /// <summary>
        /// Verify Lifetime managers. When registered using externally controlled and freed, new instance is 
        /// returned when again resolve is done.
        /// </summary>
        [TestMethod]
        public void Container_Lifetime_ExternallyControlledLifetimeManager_Lifetime()
        {
            IUnityContainer parentuc = new UnityContainer();

            parentuc.RegisterType<UnityTestClass>(new ExternallyControlledLifetimeManager());

            UnityTestClass parentinstance = parentuc.Resolve<UnityTestClass>();
            parentinstance.Name = "Hello World Ob1";
            // ReSharper disable once RedundantAssignment
            parentinstance = null;
            GC.Collect();
            UnityTestClass parentinstance1 = parentuc.Resolve<UnityTestClass>();

            Assert.AreSame("Hello", parentinstance1.Name);
        }

        /// <summary>
        /// Verify Lifetime managers. When registered using externally controlled. Should return me with new 
        /// instance every time when asked by ResolveMethod.
        /// Bug ID : 16372
        /// </summary>
        [TestMethod]
        public void Container_Lifetime_ExternallyControlledLifetimeManager_Resolve()
        {
            IUnityContainer parentuc = new UnityContainer();
            parentuc.RegisterType<UnityTestClass>(new ExternallyControlledLifetimeManager());

            UnityTestClass parentinstance = parentuc.Resolve<UnityTestClass>();
            parentinstance.Name = "Hello World Ob1";

            UnityTestClass parentinstance1 = parentuc.Resolve<UnityTestClass>();

            Assert.AreSame(parentinstance.Name, parentinstance1.Name);
        }

        /// <summary>
        /// Verify Lifetime managers. When registered using container controlled and freed, even then
        /// same instance is returned when asked for ResolveMethod.
        /// </summary>
        [TestMethod]
        public void Container_Lifetime_ExternallyControlledLifetimeManager_Use()
        {
            UnityTestClass obj1 = new UnityTestClass();

            obj1.Name = "InstanceObj";

            UnityContainer parentuc = new UnityContainer();
            parentuc.RegisterType<UnityTestClass>(new ContainerControlledLifetimeManager());

            UnityTestClass parentinstance = parentuc.Resolve<UnityTestClass>();
            parentinstance.Name = "Hello World Ob1";
            parentinstance = null;
            GC.Collect();

            UnityTestClass parentinstance1 = parentuc.Resolve<UnityTestClass>();

            Assert.AreSame("Hello World Ob1", parentinstance1.Name);
        }

        /// <summary>
        /// The ResolveMethod method returns the object registered with the named mapping, 
        /// or raises an exception if there is no mapping that matches the specified name. Testing this scenario
        /// Bug ID : 16371
        /// </summary>
        [TestMethod]
        public void Container_Lifetime_ExternallyControlledLifetimeManager_WithName()
        {
            IUnityContainer uc = new UnityContainer();

            UnityTestClass obj = uc.Resolve<UnityTestClass>("Hello");

            Assert.IsNotNull(obj);
        }

        [TestMethod]
        public void Container_Lifetime_ExternallyControlledLifetimeManager_Empty()
        {
            UnityContainer uc1 = new UnityContainer();

            uc1.RegisterType<ATest>(new ContainerControlledLifetimeManager());
            uc1.RegisterType<ATest>(String.Empty, new ContainerControlledLifetimeManager());
            uc1.RegisterType<ATest>(null, new ContainerControlledLifetimeManager());

            ATest a = uc1.Resolve<ATest>();
            ATest b = uc1.Resolve<ATest>(String.Empty);
            ATest c = uc1.Resolve<ATest>((string)null);

            Assert.AreEqual(a, b);
            Assert.AreEqual(b, c);
            Assert.AreEqual(a, c);
        }

        #endregion


        #region Test Data

        public class A { }

        public class B{ }

        public interface ITTest { }

        public class ATest : ITTest
        {
            public string Strtest = "Hello";
        }
        public class UnityTestClass
        {
            private string name = "Hello";

            public string Name
            {
                get { return name; }
                set { name = value; }
            }
        }

        #endregion
    }
}