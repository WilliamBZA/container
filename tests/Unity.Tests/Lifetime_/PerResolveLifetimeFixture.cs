﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity;
using Unity.Attributes;
using Unity.Lifetime;
using Unity.Registration;

namespace Microsoft.Practices.Unity.Tests
{
    /// <summary>
    /// Summary description for PerResolveLifetimeFixture
    /// </summary>
    [TestClass]
    public class PerResolveLifetimeFixture
    {
        [TestMethod]
        public void ContainerCanBeConfiguredForPerBuildSingleton()
        {
            var container = new UnityContainer()
                .RegisterType<IPresenter, MockPresenter>()
                .RegisterType<IView, View>(new PerResolveLifetimeManager());
        }

        [TestMethod]
        public void ViewIsReusedAcrossGraph()
        {
            var container = new UnityContainer()
                .RegisterType<IPresenter, MockPresenter>()
                .RegisterType<IView, View>(new PerResolveLifetimeManager());

            var view = container.Resolve<IView>();

            var realPresenter = (MockPresenter)view.Presenter;
            Assert.AreSame(view, realPresenter.View);
        }

        [TestMethod]
        public void ViewsAreDifferentInDifferentResolveCalls()
        {
            var container = new UnityContainer()
                .RegisterType<IPresenter, MockPresenter>()
                .RegisterType<IView, View>(new PerResolveLifetimeManager());

            var view1 = container.Resolve<IView>();
            var view2 = container.Resolve<IView>();

            Assert.AreNotSame(view1, view2);
        }

        [TestMethod]
        public void PerBuildLifetimeIsHonoredWhenUsingFactory()
        {
            var container = new UnityContainer()
                .RegisterType<SomeService>(
                    new PerResolveLifetimeManager(),
                    new InjectionFactory(c => new SomeService()));

            var rootService = container.Resolve<AService>();
            Assert.AreSame(rootService.SomeService, rootService.OtherService.SomeService);
        }

        // A small object graph to verify per-build configuration works

        public interface IPresenter { }
        public class MockPresenter : IPresenter
        {
            public IView View { get; set; }

            public MockPresenter(IView view)
            {
                View = view;
            }
        }

        public interface IView
        {
            IPresenter Presenter { get; set; }
        }

        public class View : IView
        {
            [Dependency]
            public IPresenter Presenter { get; set; }
        }

        public class SomeService { }

        public class SomeOtherService
        {
            public SomeService SomeService { get; set; }
            public SomeOtherService(SomeService someService)
            {
                this.SomeService = someService;
            }
        }

        public class AService
        {
            public AService(SomeOtherService otherService)
            {
                this.OtherService = otherService;
            }

            [Dependency]
            public SomeService SomeService { get; set; }

            public SomeOtherService OtherService { get; set; }
        }
    }
}