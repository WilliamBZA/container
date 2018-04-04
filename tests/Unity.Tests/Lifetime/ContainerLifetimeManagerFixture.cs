using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity.Lifetime;

namespace Unity.Container.Tests.Lifetime
{
    [TestClass]
    public class ContainerControlledLifetimeManagerFixture
    {
        #region Tests

        /// <summary>
        /// Registering a type twice with SetSingleton method. once with default and second with name.
        /// </summary>
        [TestMethod]
        public void Container_Lifetime_ContainerControlledLifetimeManager_DoneTwice()
        {
            IUnityContainer uc = new UnityContainer();

            uc.RegisterType<A>(new ContainerControlledLifetimeManager())
                .RegisterType<A>("hello", new ContainerControlledLifetimeManager());
            A obj = uc.Resolve<A>();
            A obj1 = uc.Resolve<A>("hello");

            Assert.AreNotSame(obj, obj1);
        }

        /// <summary>
        /// Registering a type twice with SetSingleton method. once with default and second with name.
        /// </summary>
        [TestMethod]
        public void Container_Lifetime_ContainerControlledLifetimeManager_InstanceDoneTwice()
        {
            IUnityContainer uc = new UnityContainer();

            A aInstance = new A();
            uc.RegisterInstance(aInstance).RegisterInstance("hello", aInstance);
            A obj = uc.Resolve<A>();
            A obj1 = uc.Resolve<A>("hello");

            Assert.AreSame(obj, aInstance);
            Assert.AreSame(obj1, aInstance);
            Assert.AreSame(obj, obj1);
        }


        /// <summary>
        /// SetSingleton class A. Then register instance of class A twice. once by default second by name.
        /// </summary>
        [TestMethod]
        public void Container_Lifetime_ContainerControlledLifetimeManager_RegisterInstanceTwice()
        {
            IUnityContainer uc = new UnityContainer();

            A aInstance = new A();
            uc.RegisterInstance(aInstance).RegisterInstance("hello", aInstance);
            A obj = uc.Resolve<A>();
            A obj1 = uc.Resolve<A>("hello");

            Assert.AreSame(obj, obj1);
        }

        /// <summary>
        /// SetLifetime class A. Then use GetOrDefault method to get the instances, once without name, second with name.
        /// </summary>
        [TestMethod]
        public void Container_Lifetime_ContainerControlledLifetimeManager_GetTwice()
        {
            IUnityContainer uc = new UnityContainer();

            uc.RegisterType<A>(new ContainerControlledLifetimeManager());
            A obj = uc.Resolve<A>();
            A obj1 = uc.Resolve<A>("hello");

            Assert.AreNotSame(obj, obj1);
        }

        /// <summary>
        /// SetSingleton class A. Then register instance of class A twice. once by default second by name. 
        /// Then SetLifetime once default and then by name.
        /// </summary>
        [TestMethod]
        public void Container_Lifetime_ContainerControlledLifetimeManager_SetLifetimeTwice()
        {
            IUnityContainer container = new UnityContainer();

            A aInstance = new A();

            container.RegisterInstance(aInstance);
            container.RegisterInstance("hello", aInstance);
            container.RegisterType<A>(new ContainerControlledLifetimeManager());
            container.RegisterType<A>("hello1", new ContainerControlledLifetimeManager());

            A obj = container.Resolve<A>();
            A obj1 = container.Resolve<A>("hello1");

            Assert.AreNotSame(obj, obj1);
        }

        /// <summary>defect
        /// SetSingleton class A with name. then register instance of A twice. Once by name, second by default.       
        /// </summary>
        [TestMethod]
        public void Container_Lifetime_ContainerControlledLifetimeManager_Instance()
        {
            IUnityContainer uc = new UnityContainer();

            A aInstance = new A();
            uc.RegisterType<A>("SetA", new ContainerControlledLifetimeManager())
                .RegisterInstance(aInstance)
                .RegisterInstance("hello", aInstance);

            A obj = uc.Resolve<A>("SetA");
            A obj1 = uc.Resolve<A>();
            A obj2 = uc.Resolve<A>("hello");

            Assert.AreSame(obj1, obj2);
            Assert.AreNotSame(obj, obj2);
        }

        /// <summary>
        /// Use SetLifetime twice, once with parameter, and without parameter
        /// </summary>
        [TestMethod]
        public void Container_Lifetime_ContainerControlledLifetimeManager_SetLifetime()
        {
            IUnityContainer uc = new UnityContainer();

            uc.RegisterType<A>(new ContainerControlledLifetimeManager())
               .RegisterType<A>("hello", new ContainerControlledLifetimeManager());

            A obj = uc.Resolve<A>();
            A obj1 = uc.Resolve<A>("hello");

            Assert.AreNotSame(obj, obj1);
        }

        /// <summary>
        /// Registering a type in both parent as well as child. Now trying to ResolveMethod from both
        /// check if same or different instances are returned.
        /// </summary>
        [TestMethod]
        public void Container_Lifetime_ContainerControlledLifetimeManager_ParentAndChild()
        {
            //create unity container
            UnityContainer parentuc = new UnityContainer();

            //register type UnityTestClass
            parentuc.RegisterType<UnityTestClass>(new ContainerControlledLifetimeManager());

            UnityTestClass mytestparent = parentuc.Resolve<UnityTestClass>();
            mytestparent.Name = "Hello World";
            IUnityContainer childuc = parentuc.CreateChildContainer();
            childuc.RegisterType<UnityTestClass>(new ContainerControlledLifetimeManager());

            UnityTestClass mytestchild = childuc.Resolve<UnityTestClass>();

            Assert.AreNotSame(mytestparent.Name, mytestchild.Name);
        }


        #endregion


        #region Test Data

        public class A { }

        public class UnityTestClass
        {
            private string _name = "Hello";

            public string Name
            {
                get { return _name; }
                set { _name = value; }
            }
        }

        #endregion
    }
}
