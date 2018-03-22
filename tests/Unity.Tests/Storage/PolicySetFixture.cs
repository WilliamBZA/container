using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity.Registration;
using Unity.Storage;

namespace Unity.Container.Tests.Storage
{
    [TestClass]
    public class PolicySetFixture
    {
        #region Setup

        public static IEnumerable<object[]> TestMethodInput
        {
            get
            {
                yield return new object[] { 0, new ImplicitRegistration(typeof(TestClass), null) };
                yield return new object[] { 0, new ExplicitRegistration(typeof(TestClass), null, null) };
            }
        }

        #endregion


        #region Tests

        [DataTestMethod]
        [DynamicData(nameof(TestMethodInput))]
        [Ignore]
        public void Container_Storage_PolicySet(int test, IPolicySet set)
        {
            Assert.IsNull(set.Get(typeof(TestClass)));

            // Can add
            set.Set(typeof(TestClass), new TestClass());
            Assert.IsNotNull(set.Get(typeof(TestClass)));

            // TODO: Add tests for the rest of methods
        }

        #endregion


        #region Test Data

        // ReSharper disable UnusedMember.Local
        // ReSharper disable UnusedParameter.Local

        private class TestClass
        {
        }

        #endregion
    }
}
