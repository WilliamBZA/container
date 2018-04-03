using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity.Tests.TestObjects;

namespace Unity.Container.Tests
{
    [TestClass]
    public class DevelopmentTests
    {

        [TestMethod]
        public void Development_CurrentTest()
        {
            object resolved;
//            _container.RegisterType(typeof(IList<>), typeof(List<>), new InjectionConstructor());

            resolved = _container.Resolve<object>();
            resolved = _container.Resolve<Service1>();
            resolved = _container.Resolve<IList<object>>();

            Assert.IsNotNull(resolved);
        }


        ///////////////////////////////////////

        private IUnityContainer _container;

        [TestInitialize]
        public void Setup()
        {
            _container = new UnityContainer();
        }
    }

    public class Service1
    {
        IUnityContainer _contaoner;

        public Service1(IUnityContainer container)
        {
            _contaoner = container;
        }
    }


    // A dummy class to support testing type mapping
    public class Service : IService, IDisposable
    {
        public string ID { get; } = Guid.NewGuid().ToString();

        public bool Disposed = false;
        public void Dispose()
        {
            Disposed = true;
        }
    }
}
