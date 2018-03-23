using System;
using System.Diagnostics;
using System.Reflection;
using Unity.Build.Context;
using Unity.Build.Pipeleine;
using Unity.Build.Policy;
using Unity.Lifetime;
using Unity.Registration;

// ReSharper disable RedundantLambdaParameterType

namespace Unity.Aspects
{
    public static class LifetimeAspect
    {
        public static RegisterPipeline ExplicitRegistrationLifetimeAspectFactory(RegisterPipeline next)
        {
            // Create Lifetime registration aspect
            return (IUnityContainer container, ImplicitRegistration registration, object[] args) =>
            {
                // Build rest of pipeline
                next?.Invoke(container, registration, args);

                // Create aspect

                LifetimeManager lifetime;
                if (registration is ExplicitRegistration staticRegistration)
                {
                    lifetime = staticRegistration.LifetimeManager;
                    if (lifetime is IRequireBuild) staticRegistration.BuildRequired = true;
                }
                else
                {
                    lifetime = (LifetimeManager)registration.Get(typeof(ILifetimePolicy));
                }


                // No lifetime management if null or Transient
                if (null == lifetime || lifetime is TransientLifetimeManager) return;

                // Add aspect
                var pipeline = registration.ResolveMethod;
                if (null == pipeline)
                    registration.ResolveMethod = (ref ResolutionContext context) => lifetime.GetValue(context.LifetimeContainer);
                else
                    registration.ResolveMethod = (ref ResolutionContext context) =>
                    {
                        var value = lifetime.GetValue(context.LifetimeContainer);
                        if (null != value) return value;
                        value = pipeline(ref context);
                        lifetime.SetValue(value, context.LifetimeContainer);
                        return value;
                    };
            };
        }


        public static RegisterPipeline ImplicitRegistrationLifetimeAspectFactory(RegisterPipeline next)
        {
            // Create Lifetime registration aspect
            return (IUnityContainer container, ImplicitRegistration registration, object[] args) =>
            {
                // Create appropriate lifetime manager
                if (registration.Type.GetTypeInfo().IsGenericType)
                {
                    // When type is Generic this aspect expects to get corresponding open generic 
                    // registration and lifetime container
                    Debug.Assert(null != args && 0 < args.Length, "No generic definition provided");    // TODO: Add proper error message
                    Debug.Assert(args[0] is ExplicitRegistration, "Registration of incorrect type");    // TODO: Add proper error message
                    Debug.Assert(args[1] is ILifetimeContainer,   "Registration of incorrect type");    // TODO: Add proper error message

                    var explicitRegistration = (ExplicitRegistration)args[0];
                    if (explicitRegistration.LifetimeManager is ILifetimeFactoryPolicy factoryPolicy)
                    {
                        var manager = factoryPolicy.CreateLifetimePolicy();
                        if (manager is IDisposable) ((ILifetimeContainer)args[1]).Add(manager);

                        var pipeline = registration.ResolveMethod;
                        if (null == pipeline)
                            registration.ResolveMethod = (ref ResolutionContext context) => manager.GetValue(context.LifetimeContainer);
                        else
                            registration.ResolveMethod = (ref ResolutionContext context) =>
                            {
                                var value = manager.GetValue(context.LifetimeContainer);
                                if (null != value) return value;
                                value = pipeline(ref context);
                                manager.SetValue(value, context.LifetimeContainer);
                                return value;
                            };
                    }
                }

                // Build rest of pipeline and pass arguments too
                next?.Invoke(container, registration, args);
            };
        }
    }
}
