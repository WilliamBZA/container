using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity.Lifetime;
using Unity.Tests.Generics;

namespace Unity.Container.Tests.Lifetime
{
    [TestClass]
    public class PerThreadLifetimeManagerFixture
    {
        #region Setup

        private static void HelperThreadProcedure(object o)
        {
            ThreadInformation info = (ThreadInformation) o;

            IHaveManyGenericTypesClosed resolve1 = info.Container.Resolve<IHaveManyGenericTypesClosed>();
            IHaveManyGenericTypesClosed resolve2 = info.Container.Resolve<IHaveManyGenericTypesClosed>();

            Assert.AreSame(resolve1, resolve2);

            info.SetThreadResult(Thread.CurrentThread, resolve1);
        }
        
        #endregion


        #region Tests

        [TestMethod]
        public void Container_Lifetime_PerThreadLifetimeManager_SameThread()
        {
            IUnityContainer container = new UnityContainer();

            container.RegisterType<IHaveManyGenericTypesClosed, HaveManyGenericTypesClosed>(new PerThreadLifetimeManager());

            IHaveManyGenericTypesClosed a = container.Resolve<IHaveManyGenericTypesClosed>();
            IHaveManyGenericTypesClosed b = container.Resolve<IHaveManyGenericTypesClosed>();

            Assert.AreSame(a, b);
        }

        [TestMethod]
        public void Container_Lifetime_PerThreadLifetimeManager_DifferentThreads()
        {
            IUnityContainer container = new UnityContainer();

            container.RegisterType<IHaveManyGenericTypesClosed, HaveManyGenericTypesClosed>(new PerThreadLifetimeManager());

            Thread t1 = new Thread(new ParameterizedThreadStart(HelperThreadProcedure));
            Thread t2 = new Thread(new ParameterizedThreadStart(HelperThreadProcedure));

            ThreadInformation info =
                new ThreadInformation(container);

            t1.Start(info);
            t2.Start(info);
            t1.Join();
            t2.Join();

            IHaveManyGenericTypesClosed a = new List<IHaveManyGenericTypesClosed>(info.ThreadResults.Values)[0];
            IHaveManyGenericTypesClosed b = new List<IHaveManyGenericTypesClosed>(info.ThreadResults.Values)[1];

            Assert.AreNotSame(a, b);
        }

        #endregion


        #region Test Data

        public class ThreadInformation
        {
            private readonly IUnityContainer _container;
            private readonly Dictionary<Thread, IHaveManyGenericTypesClosed> _threadResults;
            private readonly object dictLock = new object();

            public ThreadInformation(IUnityContainer container)
            {
                _container = container;
                _threadResults = new Dictionary<Thread, IHaveManyGenericTypesClosed>();
            }

            public IUnityContainer Container
            {
                get { return _container; }
            }

            public Dictionary<Thread, IHaveManyGenericTypesClosed> ThreadResults
            {
                get { return _threadResults; }
            }

            public void SetThreadResult(Thread t, IHaveManyGenericTypesClosed result)
            {
                lock (dictLock)
                {
                    _threadResults.Add(t, result);
                }
            }
        }

        #endregion
    }
}
