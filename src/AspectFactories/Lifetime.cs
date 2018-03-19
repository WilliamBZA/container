using System;
using Unity.Build.Context;
using Unity.Build.Pipeline;
using Unity.Lifetime;
using Unity.Pipeline;
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
                return (IUnityContainer container, IPolicySet set, Type type, string name) =>
                {
                    var lifetime = (LifetimeManager)set.Get(typeof(ILifetimePolicy));

                    // Add to appropriate storage
                    var target = (lifetime is ISingletonLifetimePolicy) ? ((UnityContainer)container)._root : container;

                    // Add or replace if exists 
                    var previous = ((UnityContainer)target).Register((InternalRegistration)set);
                    if (previous is StaticRegistration old && old.LifetimeManager is IDisposable disposable)
                    {
                        // Dispose replaced lifetime manager
                        ((UnityContainer)target)._lifetimeContainer.Remove(disposable);
                        disposable.Dispose();
                    }

                    // Build rest of pipeline
                    if (null == lifetime?.GetValue(((UnityContainer)target)._lifetimeContainer))
                        next?.Invoke(container, set, type, name);

                    // No lifetime management if null or Transient
                    if (null == lifetime || lifetime is TransientLifetimeManager) return;

                    // If Disposable add to container's lifetime
                    if (lifetime is IDisposable manager)
                        ((UnityContainer)target)._lifetimeContainer.Add(manager);

                    // Add aspect to resolver
                    var pipeline = ((InternalRegistration)set).ResolveMethod;
                    if (null == pipeline)
                        ((InternalRegistration)set).ResolveMethod = (ref ResolutionContext context) => lifetime.GetValue(context.LifetimeContainer);
                    else
                        ((InternalRegistration)set).ResolveMethod = (ref ResolutionContext context) =>
                        {
                            var value = lifetime.GetValue(context.LifetimeContainer);
                            if (null != value) return value;
                            value = pipeline(ref context);
                            lifetime.SetValue(value, context.LifetimeContainer);
                            return value;
                        };
                };
            }
        }
    }
}
