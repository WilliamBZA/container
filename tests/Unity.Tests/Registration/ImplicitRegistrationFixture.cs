using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity.Container.Tests.Extension;

namespace Unity.Container.Tests.Registration
{
    [TestClass]
    public class ImplicitRegistrationFixture
    {
        #region Setup

        private IUnityContainerAsync _container;
        private UnityContainer.UnityContainerConfigurator _configuration;

        [TestInitialize]
        public void Setup()
        {
            _container = new UnityContainer();
            _container.AddExtension(new TestingExtension());
            _configuration = _container.Configure<UnityContainer.UnityContainerConfigurator>();
        }


        #endregion


        #region Tests

        [TestMethod]
        public void Container_Registration_ImplicitRegistration()
        {
            Assert.IsNotNull(_configuration);
            Assert.IsNotNull(_container);



        }



        #endregion


        #region Test Data

        // ReSharper disable UnusedMember.Local
        // ReSharper disable UnusedParameter.Local

        private interface IService { }

        private class Service : IService { }

        #endregion
    }
}
