using System;
using Unity.Lifetime;
using Unity.Registration;

namespace Unity
{
    public partial class UnityContainer
    {
        public static class Lifetime
        {
            public static RegisterDelegate RegistrationAspectFactory(RegisterDelegate next)
            {
                // Create Lifetime registration aspect
                return (IUnityContainer container, ref RegistrationData data) =>
                {
                    var registration = (InternalRegistration)data.Registration;
                    var lifetime = (LifetimeManager)registration.Get(typeof(ILifetimePolicy));
                    if (null != lifetime && !(lifetime is TransientLifetimeManager))
                    {
                        // Add to appropriate storage
                        var target = (lifetime is ISingletonLifetimePolicy) ? ((UnityContainer)container)._root : container;

                        // Add or replace if exists 
                        var previous = ((UnityContainer)target).Register(data.Registration);
                        if (previous is StaticRegistration old && old.LifetimeManager is IDisposable disposable)
                        {
                            // Dispose replaced lifetime manager
                            ((UnityContainer)target)._lifetimeContainer.Remove(disposable);
                            disposable.Dispose();
                        }

                        // If Disposable add to container's lifetime
                        if (lifetime is IDisposable manager)
                            ((UnityContainer)target)._lifetimeContainer.Add(manager);

                        // Setup Instance
                        if (null != data.Instance)
                        {
                            lifetime.SetValue(data.Instance, ((UnityContainer)target)._lifetimeContainer);
                            registration.Resolve = (ref ResolutionContext context) => lifetime.GetValue(((UnityContainer)context.Container)._lifetimeContainer);
                            return;
                        }

                        // Setup Factory
                        if (null != data.Factory)
                        {
                            var factory = data.Factory;
                            registration.Resolve = (ref ResolutionContext context) =>
                            {
                                var value = lifetime.GetValue(((UnityContainer)context.Container)._lifetimeContainer);
                                if (null != value) return value;
                                value = factory(context.Container, registration.Type, registration.Name);
                                lifetime.SetValue(value, ((UnityContainer)context.Container)._lifetimeContainer);
                                return value;
                            };
                            return;
                        }

                        // Build rest of the pipeline
                        next?.Invoke(container, ref data);

                        // Add aspect to resolver
                        var pipeline = registration.Resolve;
                        if (null == pipeline)
                            registration.Resolve = (ref ResolutionContext context) => lifetime.GetValue(((UnityContainer)context.Container)._lifetimeContainer);
                        else
                            registration.Resolve = (ref ResolutionContext context) =>
                            {
                                var value = lifetime.GetValue(((UnityContainer)context.Container)._lifetimeContainer);
                                if (null != value) return value;
                                value = pipeline(ref context);
                                lifetime.SetValue(value, ((UnityContainer)context.Container)._lifetimeContainer);
                                return value;
                            };
                    }
                    else
                    {
                        // Build rest of pipeline
                        next?.Invoke(container, ref data);
                    }
                };
            }
        }
    }
}
