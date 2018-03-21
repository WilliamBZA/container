using Unity.Build.Pipeleine;
using Unity.Policy;
using Unity.Registration;

namespace Unity.Aspects
{
    public static class FactoryDelegateAspect
    {
        public static RegisterPipeline DelegateAspectFactory(RegisterPipeline next)
        {
            // Create Factory Method registration aspect
            return (IUnityContainer container, IPolicySet set, object[] args) =>
            {
                switch (set.Get(typeof(IInjectionFactory)))
                {
                    //case Func<IUnityContainer, Type, string, object> function:
                    //    ((InternalRegistration)set).ResolveMethod = (ref ResolutionContext context) => function(context.LifetimeContainer.Container, type, name);
                    //    break;

                    //case InjectionFactory injectionFactory:
                    //    ((InternalRegistration)set).ResolveMethod = (ref ResolutionContext context) => injectionFactory.Factory(context.LifetimeContainer.Container, type, name);
                    //    break;

                    default:
                        next?.Invoke(container, set, args);
                        break;
                }
            };
        }
    }
}
