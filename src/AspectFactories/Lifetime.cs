using System;
using Unity.Lifetime;
using Unity.Policy;
using Unity.Registration;
using Unity.Resolution;

namespace Unity
{
    public partial class UnityContainer
    {
        public static class Lifetime
        {
            public static RegisterPipeline RegistrationAspectFactory(RegisterPipeline next)
            {
                // Create Lifetime registration aspect
                return (IUnityContainer container, IPolicySet registration, Type type, string name) =>
                {
                    var lifetime = (LifetimeManager)registration.Get(typeof(ILifetimePolicy));

                    // Add to appropriate storage
                    var target = (lifetime is ISingletonLifetimePolicy) ? ((UnityContainer)container)._root : container;

                    // Add or replace if exists 
                    var previous = ((UnityContainer)target).Register((InternalRegistration)registration);
                    if (previous is StaticRegistration old && old.LifetimeManager is IDisposable disposable)
                    {
                        // Dispose replaced lifetime manager
                        ((UnityContainer)target)._lifetimeContainer.Remove(disposable);
                        disposable.Dispose();
                    }

                    // Build rest of pipeline
                    if (null == lifetime?.GetValue(((UnityContainer)target)._lifetimeContainer))
                        next?.Invoke(container, registration, type, name);

                    // No lifetime management if null or Transient
                    if (null == lifetime || lifetime is TransientLifetimeManager) return;

                    // If Disposable add to container's lifetime
                    if (lifetime is IDisposable manager)
                        ((UnityContainer)target)._lifetimeContainer.Add(manager);

                    // Add aspect to resolver
                    var pipeline = ((InternalRegistration)registration).Resolve;
                    if (null == pipeline)
                        ((InternalRegistration)registration).Resolve = (ref ResolutionContext context) => lifetime.GetValue(((UnityContainer)context.Container)._lifetimeContainer);
                    else
                        ((InternalRegistration)registration).Resolve = (ref ResolutionContext context) =>
                        {
                            var value = lifetime.GetValue(((UnityContainer)context.Container)._lifetimeContainer);
                            if (null != value) return value;
                            value = pipeline(ref context);
                            lifetime.SetValue(value, ((UnityContainer)context.Container)._lifetimeContainer);
                            return value;
                        };
                };
            }
        }
    }
}
