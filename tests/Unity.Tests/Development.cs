using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Unity.Tests.TestObjects;

namespace Unity.Tests
{
    [TestClass]
    public class DevelopmentTests
    {

        [TestMethod]
        public void Development_CurrentTest()
        {
            _container.RegisterType(typeof(IList<>), typeof(List<>));

            Assert.IsNotNull(_container.Resolve<IList<object>>());
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
