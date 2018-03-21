using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity.Attributes;
using Unity.Build.Selected;
using Unity.Build.Selection;
using Unity.Registration;

namespace Unity.Container.Tests.Build.Selection
{
    [TestClass]
    public class SelectConstructorFixture
    {
        #region Setup

        public static IEnumerable<object[]> TestMethodInput
        {
            get
            {
                yield return new object[] { 0, typeof(Tst1), 1 };
                yield return new object[] { 1, typeof(TestClass<int, string, Delegate, Type>), 4 };
            }
        }

        public static IEnumerable<object[]> TestMethodFailInput
        {
            get
            {
                yield return new object[] { 0, typeof(Tst0) };
            }
        }

        #endregion


        [DataTestMethod]
        [DynamicData(nameof(TestMethodInput))]
        [Ignore]
        public void Container_Build_Selection_SelectLongestConstructor(int test, Type type, int index)
        {
            //var ctors = type.GetTypeInfo().DeclaredConstructors.ToArray();
            //var ctor = new SelectedConstructor(ctors[index]);
            //var selector = SelectLongestConstructor.SelectConstructorPipelineFactory(null);
            //var registration = new InternalRegistration(type, null);
            //var selection = selector(null, registration);

            //Assert.AreEqual(ctor.Constructor, selection.Constructor);
        }

        [DataTestMethod]
        [DynamicData(nameof(TestMethodFailInput))]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Container_Build_Selection_SelectLongestConstructor_fail(int test, Type type)
        {
            var selector = SelectLongestConstructor.SelectConstructorPipelineFactory(null);
            var registration = new InternalRegistration(type, null);
            var selection = selector(null, registration);
        }



        [DataTestMethod]
        [DynamicData(nameof(TestMethodInput))]
        public void Container_Build_Selection_SelectInjectionConstructor(int test, Type type, int index)
        {
            var ctors = type.GetTypeInfo().DeclaredConstructors.ToArray();
            var ctor = new SelectedConstructor(ctors[index]);
            var selector = SelectInjectionMembers.SelectConstructorPipelineFactory(null);
            var registration = new InternalRegistration(type, null, typeof(SelectedConstructor), ctor);
            var selection = selector(null, registration);

            Assert.AreEqual(ctor, selection);
        }


        #region Test Data

        // ReSharper disable UnusedMember.Local
        // ReSharper disable UnusedParameter.Local

        private class Tst0
        {
            // Simple types                              
            public Tst0() { }
            [InjectionConstructor]
            public Tst0(int a) { }
            [InjectionConstructor]
            public Tst0(string a) { }
        }

        private class Tst1
        {
            // Simple types                              
            public Tst1() { }
            [InjectionConstructor]
            public Tst1(int a) { }
        }

        private class Tst2
        {
            // Simple types                              
            public Tst2() { }
            public Tst2(int a) { }
            public Tst2(object a) { }
        }

        private class TestClass<TA, TB, TC, TD>
        {
            // Simple types                              
            public TestClass() { }
            public TestClass(TA a) { }
            public TestClass(TA a, TB b) { }
            public TestClass(TA a, TB b, TC c) { }
            [InjectionConstructor]
            public TestClass(TA a, TB b, TC c, TD d) { }

        }

        #endregion
    }

}
