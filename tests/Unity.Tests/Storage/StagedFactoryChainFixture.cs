using System.Collections;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity.Build.Pipeline;
using Unity.Build.Stage;
using Unity.Container.Storage;
using Unity.Lifetime;
using Unity.Storage;

namespace Unity.Container.Tests.Storage
{
    [TestClass]
    public class StagedFactoryChainFixture
    {
        #region Setup

        private string _data;
        private StagedFactoryChain<RegisterPipeline, RegisterStage> _chain;

        [TestInitialize]
        public void Setup()
        {
            _data = null;
            _chain = new StagedFactoryChain<RegisterPipeline, RegisterStage>
            {
                {TestAspectFactory1, RegisterStage.Setup},
                {TestAspectFactory2, RegisterStage.Collections},
                {TestAspectFactory3, RegisterStage.Lifetime},
                {TestAspectFactory4, RegisterStage.TypeMapping},
                {TestAspectFactory5, RegisterStage.PreCreation},
                {TestAspectFactory6, RegisterStage.Creation},
                {TestAspectFactory7, RegisterStage.Initialization},
                {TestAspectFactory8, RegisterStage.PostInitialization}
            };
            _chain.Invalidated += (sender, args) => _data = "Invalidated-";
        }

        #endregion


        #region Tests

        [TestMethod]
        public void Container_Storage_StagedFactoryChain_empty()
        {
            StagedFactoryChain<RegisterPipeline, RegisterStage> chain = new StagedFactoryChain<RegisterPipeline, RegisterStage>();
            Assert.IsNull(chain.BuildPipeline());
        }

        [TestMethod]
        public void Container_Storage_StagedFactoryChain_build()
        {
            var method = _chain.BuildPipeline();
            Assert.IsNotNull(method);

            _data = _data + "-";
            method.Invoke(null, null, null);
            Assert.AreEqual("87654321-12345678", _data);
        }

        [TestMethod]
        public void Container_Storage_StagedFactoryChain_enumerable()
        {
            Assert.IsNotNull(((IEnumerable)_chain).GetEnumerator());
        }

        [TestMethod]
        public void Container_Storage_StagedFactoryChain_enumerable_gen()
        {
            var array = _chain.ToArray();
            Assert.IsNotNull(array);
            Assert.AreEqual(8, array.Length);
        }

        [TestMethod]
        public void Container_Storage_StagedFactoryChain_add()
        {
            Assert.IsTrue(_chain.Remove(TestAspectFactory8));
            _chain.Add(TestAspectFactory8, RegisterStage.Setup);

            var method = _chain.BuildPipeline();
            Assert.IsNotNull(method);

            _data = _data + "-";
            method.Invoke(null, null, null);
            Assert.AreEqual("Invalidated-76543218-81234567", _data);
            var array = _chain.ToArray();
            Assert.IsNotNull(array);
            Assert.AreEqual(8, array.Length);
        }

        [TestMethod]
        public void Container_Storage_StagedFactoryChain_remove()
        {
            Assert.IsTrue(_chain.Remove(TestAspectFactory8));
            Assert.IsFalse(_chain.Remove(TestAspectFactory8));
            var method = _chain.BuildPipeline();
            Assert.IsNotNull(method);

            _data = _data + "-";
            method.Invoke(null, null, null);
            Assert.AreEqual("Invalidated-7654321-1234567", _data);
            var array = _chain.ToArray();
            Assert.IsNotNull(array);
            Assert.AreEqual(7, array.Length);
        }


        #endregion


        #region Test Data

        public RegisterPipeline TestAspectFactory1(RegisterPipeline next)
        {
            _data = _data + "1";

            return (container, set, args) =>
            {
                _data = _data + "1";
                next?.Invoke(container, set, args);
            };
        }

        public RegisterPipeline TestAspectFactory2(RegisterPipeline next)
        {
            _data = _data + "2";

            return (ILifetimeContainer container, IPolicySet set, object[] args) =>
            {
                _data = _data + "2";
                next?.Invoke(container, set, args);
            };
        }

        public RegisterPipeline TestAspectFactory3(RegisterPipeline next)
        {
            _data = _data + "3";

            return (ILifetimeContainer container, IPolicySet set, object[] args) =>
            {
                _data = _data + "3";
                next?.Invoke(container, set, args);
            };
        }

        public RegisterPipeline TestAspectFactory4(RegisterPipeline next)
        {
            _data = _data + "4";
            return (ILifetimeContainer container, IPolicySet set, object[] args) =>
            {
                _data = _data + "4";
                next?.Invoke(container, set, args);
            };
        }

        public RegisterPipeline TestAspectFactory5(RegisterPipeline next)
        {
            _data = _data + "5";

            return (ILifetimeContainer container, IPolicySet set, object[] args) =>
            {
                _data = _data + "5";
                next?.Invoke(container, set, args);
            };
        }

        public RegisterPipeline TestAspectFactory6(RegisterPipeline next)
        {
            _data = _data + "6";

            return (ILifetimeContainer container, IPolicySet set, object[] args) =>
            {
                _data = _data + "6";
                next?.Invoke(container, set, args);
            };
        }

        public RegisterPipeline TestAspectFactory7(RegisterPipeline next)
        {
            _data = _data + "7";

            return (ILifetimeContainer container, IPolicySet set, object[] args) =>
            {
                _data = _data + "7";
                next?.Invoke(container, set, args);
            };
        }

        public RegisterPipeline TestAspectFactory8(RegisterPipeline next)
        {
            _data = _data + "8";
            return (ILifetimeContainer container, IPolicySet set, object[] args) =>
            {
                _data = _data + "8";
                next?.Invoke(container, set, args);
            };
        }

        #endregion
    }
}
