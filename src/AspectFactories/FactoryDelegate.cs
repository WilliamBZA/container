using System;
using Unity.Build.Context;
using Unity.Build.Pipeline;
using Unity.Pipeline;
using Unity.Policy;
using Unity.Registration;
using Unity.Resolution;

namespace Unity.AspectFactories
{
    public static class FactoryDelegate
    {
        public static RegisterPipeline RegistrationAspectFactory(RegisterPipeline next)
        {
            // Create Factory Method registration aspect
            return (IUnityContainer container, IPolicySet set, Type type, string name) =>
            {
                switch (set.Get(typeof(IInjectionFactory)))
                {
                    case Func<IUnityContainer, Type, string, object> function:
                        ((InternalRegistration)set).ResolveMethod = (ref ResolutionContext context) => function(context.LifetimeContainer.Container, type, name);
                        break;

                    case InjectionFactory injectionFactory:
                        ((InternalRegistration)set).ResolveMethod = (ref ResolutionContext context) => injectionFactory.Factory(context.LifetimeContainer.Container, type, name);
                        break;

                    default:
                        next?.Invoke(container, set, type, name);
                        break;
                }
            };
        }
    }
}
