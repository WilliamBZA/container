using Unity.Build.Pipeleine;
using Unity.Registration;

namespace Unity.Aspects
{
    public static class FactoryDelegateAspect
    {
        public static RegisterPipeline DelegateAspectFactory(RegisterPipeline next)
        {
            // Create Factory Method registration aspect
            return (IUnityContainer container, ImplicitRegistration registration, object[] args) =>
            {
                switch (registration.Get(typeof(IInjectionFactory)))
                {
                    //case Func<IUnityContainer, Type, string, object> function:
                    //    ((ImplicitRegistration)set).ResolveMethod = (ref ResolutionContext context) => function(context.LifetimeContainer.Container, type, name);
                    //    break;

                    //case InjectionFactory injectionFactory:
                    //    ((ImplicitRegistration)set).ResolveMethod = (ref ResolutionContext context) => injectionFactory.Factory(context.LifetimeContainer.Container, type, name);
                    //    break;

                    default:
                        next?.Invoke(container, registration, args);
                        break;
                }
            };
        }
    }
}
