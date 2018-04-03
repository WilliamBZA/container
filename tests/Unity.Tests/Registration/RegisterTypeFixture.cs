using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity.Exceptions;
using Unity.Lifetime;
using Unity.Registration;

namespace Unity.Container.Tests.Registration
{
    [TestClass]
    public class RegisterTypeFixture
    {
        #region Setup

        private IUnityContainer _container;

        [TestInitialize]
        public void Setup() { _container = new UnityContainer(); }


        public static IEnumerable<object[]> TestMethodInput
        {
            get
            {                         //  test,   registerType,         name,        mappedType,            LifetimeManager,      InjectionMembers,   Resolve
                yield return new object[] { 03,   typeof(G2Service<,>), null,        null,                  null,                 null,               typeof(G1Service<int>) };
                yield return new object[] { 02,   typeof(G1Service<>),  null,        null,                  null,                 null,               typeof(G1Service<int>) };
                yield return new object[] { 01,   typeof(Service),      null,        null,                  null,                 null,               typeof(IService) };
                yield return new object[] { 11,   typeof(IService),     null,        typeof(Service),       null,                 null,               typeof(IService) };
            }
        }

        public static IEnumerable<object[]> TestMethodInputRegisterFail
        {
            get
            {                         
                yield return new object[] { null, null, null, null, null };
                yield return new object[] { typeof(Service), null, typeof(object), null, null };
            }
        }

        public static IEnumerable<object[]> TestMethodInputResolveFail
        {
            get
            {                         //  test,   registerType,       name,         mappedType,            LifetimeManager,      InjectionMembers
                yield return new object[] { 01, typeof(Service),      null,         null,                  null,                 null };
            }
        }

        #endregion


        #region Tests

        [DataTestMethod]
        [DynamicData(nameof(TestMethodInput))]
        public void Container_Registration_RegisterType(int test, Type registerType, string name, Type mappedType, LifetimeManager manager, InjectionMember[] members, Type resolveType)
        {
            // Set
            _container.RegisterType(registerType, name, mappedType, manager, members);

            // Act
            var value = _container.Resolve(resolveType, name);

            // Verify
            Assert.IsNotNull(value);
        }

        [DataTestMethod]
        [DynamicData(nameof(TestMethodInputRegisterFail))]
        [ExpectedException(typeof(ArgumentException), AllowDerivedTypes = true)]
        public void Container_Registration_RegisterType_Register_Fail(Type registerType, string name, Type mappedType, LifetimeManager manager, InjectionMember[] members)
        {
            // Set
            _container.RegisterType(registerType, name, mappedType, manager, members);
        }

        [DataTestMethod]
        [DynamicData(nameof(TestMethodInputResolveFail))]
        [ExpectedException(typeof(ResolutionFailedException))]
        public void Container_Registration_RegisterType_Resolve_Fail(int test, Type registerType, string name, Type mappedType, LifetimeManager manager, InjectionMember[] members)
        {
            // Set
            _container.RegisterType(registerType, name, mappedType, manager, members);

            // Act
            _container.Resolve(registerType, name);
        }

        #endregion


        #region Test Data

        // ReSharper disable UnusedMember.Local
        // ReSharper disable UnusedParameter.Local

        private interface IService { }

        private class Service : IService { }

        private class G1Service<T>
        {
        }

        private class G2Service<T1, T2>
        {
        }

        #endregion
    }
}
