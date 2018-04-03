using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity.Lifetime;
using Unity.Registration;

namespace Unity.Container.Tests.Registration
{
    [TestClass]
    public class RegisterTypeAsyncFixture
    {
        #region Setup

        private IUnityContainerAsync _container;

        [TestInitialize]
        public void Setup() { _container = new UnityContainer(); }


        public static IEnumerable<object[]> TestMethodInput
        {
            get
            {                         //  test,   registerType,         name,        mappedType,            LifetimeManager,      InjectionMembers,   Resolve
                yield return new object[] { 01,   typeof(Service),      null,        null,                  null,                 null,               typeof(IService) };
                yield return new object[] { 11,   typeof(IService),     null,        typeof(Service),       null,                 null,               typeof(IService) };
            }
        }

        #endregion


        #region Tests

        [DataTestMethod]
        [DynamicData(nameof(TestMethodInput))]
        public void Container_Registration_RegisterTypeAsync(int test, Type registerType, string name, Type mappedType, LifetimeManager manager, InjectionMember[] members, Type resolveType)
        {
            // Set
            _container.RegisterTypeAsync(registerType, name, mappedType, manager, members);

            // Act
            var value = _container.Resolve(resolveType, name);

            // Verify
            Assert.IsNotNull(value);
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
