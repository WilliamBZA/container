using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity;
using Unity.Exceptions;
using Unity.Registration;

namespace Microsoft.Practices.Unity.Tests
{
    [TestClass]
    public class ConstructorWithOutAndRefParametersFixture
    {
        [TestMethod]
        public void ResolvingANewInstanceOfTypeWithCtorWithRefParameterThrows()
        {
            IUnityContainer container = new UnityContainer();

            try
            {
                TypeWithConstructorWithRefParameter instance = container.Resolve<TypeWithConstructorWithRefParameter>();
                Assert.Fail("should have thrown");
            }
            catch (ResolutionFailedException)
            {
                // expected
            }
        }

        [TestMethod]
        public void ResolvingANewInstanceOfTypeWithCtorWithOutParameterThrows()
        {
            IUnityContainer container = new UnityContainer();

            try
            {
                TypeWithConstructorWithOutParameter instance = container.Resolve<TypeWithConstructorWithOutParameter>();
                Assert.Fail("should have thrown");
            }
            catch (ResolutionFailedException)
            {
                // expected
            }
        }

        public class TypeWithConstructorWithRefParameter
        {
            public TypeWithConstructorWithRefParameter(ref string ignored)
            {
            }

            public int Property { get; set; }
        }

        public class TypeWithConstructorWithOutParameter
        {
            public TypeWithConstructorWithOutParameter(out string ignored)
            {
                ignored = null;
            }

            public int Property { get; set; }
        }
    }
}
