using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity.Attributes;
using Unity.Select.Constructor;

namespace Unity.Container.Tests.Pipeline.Selection
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
        public void Container_Pipeline_Selection_SelectLongestConstructor(int test, Type type, int index)
        {
            var ctors = type.GetTypeInfo().DeclaredConstructors.ToArray();
            var selector = SelectLongestConstructor.SelectConstructorPipelineFactory(null);
            var selection = selector(type);

            Assert.AreEqual(index, Array.IndexOf(ctors, selection.Constructor));
        }

        [DataTestMethod]
        [DynamicData(nameof(TestMethodFailInput))]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Container_Pipeline_Selection_SelectLongestConstructor_fail(int test, Type type)
        {
            var ctors = type.GetTypeInfo().DeclaredConstructors.ToArray();
            var selector = SelectLongestConstructor.SelectConstructorPipelineFactory(null);
            var selection = selector(type);
        }



        [DataTestMethod]
        [DynamicData(nameof(TestMethodInput))]
        public void Container_Pipeline_Selection_SelectInjectionConstructor(int test, Type type, int index)
        {
            var ctors = type.GetTypeInfo().DeclaredConstructors.ToArray();
            var selector = SelectInjectionConstructor.SelectConstructorPipelineFactory(null);
            var selection = selector(type);

            Assert.AreEqual(index, Array.IndexOf(ctors, selection.Constructor));
        }

        [DataTestMethod]
        [DynamicData(nameof(TestMethodFailInput))]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Container_Pipeline_Selection_SelectInjectionConstructor_fail(int test, Type type)
        {
            var ctors = type.GetTypeInfo().DeclaredConstructors.ToArray();
            var selector = SelectInjectionConstructor.SelectConstructorPipelineFactory(null);
            var selection = selector(type);
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
