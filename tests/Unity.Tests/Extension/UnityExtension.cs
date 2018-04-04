// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

//===============================================================================

using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Tests.TestDoubles;
using Microsoft.Practices.Unity.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity.Builder;
using Unity.Tests.Container;

namespace Unity.Tests.Extension
{
    [TestClass]
    public class UnityExtension
    {
        [TestMethod]
        public void Ext_ContainerCallsExtensionsInitializeMethod()
        {
            MockContainerExtension extension = new MockContainerExtension();
            IUnityContainer container = new UnityContainer();
            container.AddExtension(extension);

            Assert.IsTrue(extension.InitializeWasCalled);
        }

        [TestMethod]
        public void Ext_ReceivesExtensionContextInInitialize()
        {
            MockContainerExtension extension = new MockContainerExtension();
            IUnityContainer container = new UnityContainer();
            container.AddExtension(extension);

            Assert.IsNotNull(extension.Context);
            Assert.AreSame(container, extension.Context.Container);
        }

        [TestMethod]
        public void Ext_CanGetConfigurationInterfaceFromExtension()
        {
            MockContainerExtension extension = new MockContainerExtension();
            IUnityContainer container = new UnityContainer()
                .AddExtension(extension);
            IMockConfiguration config = container.Configure<IMockConfiguration>();

            Assert.AreSame(extension, config);
            Assert.AreSame(container, config.Container);
        }

        [TestMethod]
        public void Ext_CanGetConfigurationWithoutGenericMethod()
        {
            MockContainerExtension extension = new MockContainerExtension();
            IUnityContainer container = new UnityContainer()
                .AddExtension(extension);
            IMockConfiguration config = (IMockConfiguration)container.Configure(typeof(IMockConfiguration));
            
            Assert.AreSame(extension, config);
            Assert.AreSame(container, config.Container);
        }

        [TestMethod]
        public void Ext_CanAddStrategy()
        {
            SpyStrategy spy = new SpyStrategy();
            SpyExtension extension = new SpyExtension(spy, UnityBuildStage.PostInitialization);

            IUnityContainer container = new UnityContainer()
                .AddExtension(extension);
            object result = container.Resolve<object>();
            
            Assert.IsTrue(spy.BuildUpWasCalled);
            Assert.AreSame(result, spy.Existing);
        }

        [TestMethod]
        public void Ext_StrategiesAreCalledAfterTypesAreRegistered()
        {
            IUnityContainer container = new UnityContainer();

            container.RegisterType<ITest, ATest>();

            SpyStrategy spy = new SpyStrategy();
            SpyExtension extension = new SpyExtension(spy, UnityBuildStage.PostInitialization);

            container.AddExtension(extension);
            object result = container.Resolve<ITest>();

            Assert.IsTrue(spy.BuildUpWasCalled);
            Assert.AreSame(result, spy.Existing);
        }

        [TestMethod]
        public void Ext_CanAddPolicy()
        {
            SpyStrategy spy = new SpyStrategy();
            SpyPolicy spyPolicy = new SpyPolicy();
            SpyExtension extension =
                new SpyExtension(spy, UnityBuildStage.PostInitialization, spyPolicy, typeof(SpyPolicy));
            IUnityContainer container = new UnityContainer()
                .AddExtension(extension);
            container.Resolve<object>();
            
            Assert.IsTrue(spyPolicy.WasSpiedOn);
        }

        [TestMethod]
        public void Ext_CancheckDefaultBehavior()
        {
            IUnityContainer container = new UnityContainer();
            object result = container.Resolve<object>();
            Assert.IsNotNull(result);
        }
    }
}