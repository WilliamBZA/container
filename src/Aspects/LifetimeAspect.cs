using System;
using System.Diagnostics;
using System.Reflection;
using Unity.Build.Context;
using Unity.Build.Pipeleine;
using Unity.Build.Pipeline;
using Unity.Build.Policy;
using Unity.Lifetime;
using Unity.Registration;
using Unity.Storage;

// ReSharper disable RedundantLambdaParameterType

namespace Unity.Aspects
{
    public static class LifetimeAspect
    {
        public static RegisterPipeline ExplicitRegistrationLifetimeAspectFactory(RegisterPipeline next)
        {
            // Create Lifetime registration aspect
            return (ILifetimeContainer container, IPolicySet set, object[] args) =>
            {
                // Build rest of pipeline
                next?.Invoke(container, set, args);

                // Create aspect
                var registration = (ExplicitRegistration) set;
                if (registration.LifetimeManager is IRequireBuild) registration.BuildRequired = true;

                // No lifetime management if Transient
                if (registration.LifetimeManager is TransientLifetimeManager) return;

                // Add aspect
                registration.ResolveMethod = CreateLifetimeAspect(registration.ResolveMethod, registration.LifetimeManager);
            };
        }


        public static RegisterPipeline ImplicitRegistrationLifetimeAspectFactory(RegisterPipeline next)
        {
            // Create Lifetime registration aspect
            return (ILifetimeContainer container, IPolicySet set, object[] args) =>
            {
                // Build rest of the pipeline first
                next?.Invoke(container, set, args);

                var registration = (ImplicitRegistration)set;

                // Create appropriate lifetime manager
                if (registration.Type.GetTypeInfo().IsGenericType)
                {
                    // When type is Generic this aspect expects to get corresponding open generic registration
                    Debug.Assert(null != args && 0 < args.Length, "No generic definition provided");    // TODO: Add proper error message
                    Debug.Assert(args[0] is ExplicitRegistration, "Registration of incorrect type");    // TODO: Add proper error message

                    var genericRegistration = (ExplicitRegistration)args[0];
                    if (!(genericRegistration.LifetimeManager is ILifetimeFactoryPolicy factoryPolicy) ||
                          genericRegistration.LifetimeManager is TransientLifetimeManager) return;

                    var manager = (LifetimeManager)factoryPolicy.CreateLifetimePolicy();
                    if (manager is IDisposable) container.Add(manager);
                    if (manager is IRequireBuild) registration.BuildRequired = true;

                    // Add aspect
                    registration.ResolveMethod = CreateLifetimeAspect(registration.ResolveMethod, manager);
                }
            };
        }


        private static ResolveMethod CreateLifetimeAspect(ResolveMethod pipeline, LifetimeManager manager)
        {
            if (null == pipeline)
                return (ref ResolutionContext context) => manager.GetValue(context.LifetimeContainer);

            return (ref ResolutionContext context) =>
            {
                var value = manager.GetValue(context.LifetimeContainer);
                if (null != value) return value;
                value = pipeline(ref context);
                manager.SetValue(value, context.LifetimeContainer);
                return value;
            };
        }
    }
}
