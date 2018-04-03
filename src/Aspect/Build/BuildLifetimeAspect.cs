using System;
using System.Diagnostics;
using System.Reflection;
using Unity.Build.Context;
using Unity.Build.Pipeline;
using Unity.Build.Policy;
using Unity.Container.Registration;
using Unity.Exceptions;
using Unity.Lifetime;
using Unity.Storage;

// ReSharper disable RedundantLambdaParameterType

namespace Unity.Aspect.Build
{
    public static class BuildLifetimeAspect
    {
        public static RegisterPipeline ExplicitRegistrationLifetimeAspectFactory(RegisterPipeline next)
        {
            // Create Lifetime registration aspect
            return (ILifetimeContainer lifetimeContainer, IPolicySet set, object[] args) =>
            {
                // Build rest of pipeline first
                var pipeline = next?.Invoke(lifetimeContainer, set, args);

                // Create aspect
                var registration = (ExplicitRegistration)set;
                if (registration.LifetimeManager is IRequireBuild) registration.BuildRequired = true;

                // No lifetime management if Transient
                return registration.LifetimeManager is TransientLifetimeManager 
                    ? pipeline 
                    : CreateLifetimeAspect(pipeline, registration.LifetimeManager);
            };
        }


        public static RegisterPipeline ImplicitRegistrationLifetimeAspectFactory(RegisterPipeline next)
        {
            // Create Lifetime registration aspect
            return (ILifetimeContainer lifetimeContainer, IPolicySet set, object[] args) =>
            {
                // Build rest of the pipeline first
                var pipeline = next?.Invoke(lifetimeContainer, set, args);

                var registration = (ImplicitRegistration)set;

                // Create appropriate lifetime manager
                if (registration.Type.GetTypeInfo().IsGenericType)
                {
                    // When type is Generic this aspect expects to get corresponding open generic registration
                    Debug.Assert(null != args && 0 < args.Length, "No generic definition provided");    // TODO: Add proper error message
                    Debug.Assert(args[0] is ExplicitRegistration, "Registration of incorrect type");    // TODO: Add proper error message

                    var genericRegistration = (ExplicitRegistration)args[0];
                    if (!(genericRegistration.LifetimeManager is ILifetimeFactoryPolicy factoryPolicy) ||
                          genericRegistration.LifetimeManager is TransientLifetimeManager) return pipeline;

                    var manager = (LifetimeManager)factoryPolicy.CreateLifetimePolicy();
                    if (manager is IDisposable) lifetimeContainer.Add(manager);
                    if (manager is IRequireBuild) registration.BuildRequired = true;

                    // Add aspect
                    return CreateLifetimeAspect(pipeline, manager);
                }

                return pipeline;
            };
        }


        private static ResolveMethod CreateLifetimeAspect(ResolveMethod pipeline, LifetimeManager manager)
        {
            // Instance manager
            if (null == pipeline)
                return (ref ResolutionContext context) => manager.GetValue(context.LifetimeContainer);

            // Create and recover if required
            if (manager is IRequiresRecovery recoveryPolicy)
            {
                return (ref ResolutionContext context) =>
                {
                    try
                    {
                        var value = manager.GetValue(context.LifetimeContainer);
                        if (null != value) return value;
                        value = pipeline(ref context);
                        manager.SetValue(value, context.LifetimeContainer);
                        return value;
                    }
                    finally
                    {
                        recoveryPolicy.Recover();
                    }
                };
            }

            // Just create and store
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
