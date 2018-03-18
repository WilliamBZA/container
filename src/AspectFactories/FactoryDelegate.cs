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
            return (IUnityContainer container, IPolicySet registration, Type type, string name) =>
            {
                switch (registration.Get(typeof(IInjectionFactory)))
                {
                    case Func<IUnityContainer, Type, string, object> function:
                        ((InternalRegistration)registration).Resolve = (ref ResolutionContext context) => function(context.LifetimeContainer.Container, type, name);
                        break;

                    case InjectionFactory injectionFactory:
                        ((InternalRegistration)registration).Resolve = (ref ResolutionContext context) => injectionFactory.Factory(context.LifetimeContainer.Container, type, name);
                        break;

                    default:
                        next?.Invoke(container, registration, type, name);
                        break;
                }
            };
        }
    }
}
