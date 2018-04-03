using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Build.Context;
using Unity.Build.Pipeline;
using Unity.Container.Registration;
using Unity.Lifetime;
using Unity.Registration;
using Unity.Storage;

namespace Unity
{
    public partial class UnityContainer
    {
        #region Build Aspect

        public static RegisterPipeline BuildExplicitRegistrationAspectFactory(RegisterPipeline next)
        {
            return (ILifetimeContainer lifetimeContainer, IPolicySet set, object[] args) =>
            {

                // Build rest of pipeline
                var pipeline = next?.Invoke(lifetimeContainer, set, args);

                // Check if anyone wants to hijack create process: if resolver exists, no need to do anything
                var registration = (ExplicitRegistration) set;
                if (null != pipeline)
                {
                    registration.EnableOptimization = false;
                    return pipeline;
                }

                // Analise the type and select build strategy
                var buildTypeInfo = registration.ImplementationType.GetTypeInfo();
                if (buildTypeInfo.IsGenericTypeDefinition)
                {
                    // Create generic type factory
                    InjectionConstructor ctor = null;
                    var unity = (UnityContainer)lifetimeContainer.Container;
                    var targetType = registration.ImplementationType ?? registration.Type;

                    // Enumerate all injection members
                    var members = registration.PopType<InjectionMember>()
                        .Where(m =>
                        {
                            if (!(m is InjectionConstructor constructor)) return true;
                            ctor = constructor;
                            return false;
                        })
                        .Cast<InjectionMember>()
                        .Concat(unity._injectionMembersPipeline(unity, targetType));

                    // Create the factory
                    registration.CreateActivator = (Type type) =>
                    {
                        // Create resolvers for each injection member
                        var memberResolvers = members.Select(m => m.CreateActivator)
                                                     .Where(f => null != f)
                                                     .Select(f => f(type))
                                                     .ToArray();

                        // Create object activator
                        var objectResolver = ctor.CreateActivator(type);

                        // Create composite resolver
                        return (ref ResolutionContext context) =>
                        {
                            object result;

                            try
                            {
                                result = objectResolver(ref context);
                                foreach (var resolveMethod in memberResolvers) resolveMethod(ref context); // TODO: Could be executed in parallel
                            }
                            catch (Exception e)
                            {
                                throw new InvalidOperationException($"Error creating object of type: {ctor.Constructor.DeclaringType}", e);
                            }

                            return result;
                        };
                    };
                }
                else
                {
                    var unity = (UnityContainer)lifetimeContainer.Container;
                    var members = registration.OfType<InjectionMember>()
                                              .Cast<InjectionMember>()
                                              .Where(m => !(m is InjectionConstructor))
                                              .Concat(unity._injectionMembersPipeline(unity, registration.ImplementationType));
                    registration.ResolveMethod = CreateResolver(unity, registration, set.Get<InjectionConstructor>() ??
                        unity._constructorSelectionPipeline(unity, registration.ImplementationType), members);
                }

                return pipeline;
            };
        }


        public static RegisterPipeline BuildImplicitRegistrationAspectFactory(RegisterPipeline next)
        {
            return (ILifetimeContainer lifetimeContainer, IPolicySet set, object[] args) =>
            {
                // Build rest of the pipeline first
                var pipeline = next?.Invoke(lifetimeContainer, set, args);

                // if resolver has been provided, no need to do anything
                var registration = (ImplicitRegistration)set;
                if (null != pipeline)
                {
                    registration.EnableOptimization = false;
                    return pipeline;
                }

                var info = registration.Type.GetTypeInfo();
                if (info.IsGenericType)
                {
                    var genericRegistration = (ExplicitRegistration)args[0];
                    pipeline = genericRegistration.CreateActivator?.Invoke(registration.ImplementationType) 
                                               ?? throw new InvalidOperationException("Unable to create resolver");    // TODO: Add proper error message
                }
                else
                {
                    var unity = (UnityContainer) lifetimeContainer.Container;
                    pipeline = CreateResolver(unity, registration, unity._constructorSelectionPipeline(unity, registration.ImplementationType),
                                                                   unity._injectionMembersPipeline(unity, registration.ImplementationType));
                }

                return pipeline;
            };
        }

        #endregion


        #region Implementation

        private static ResolveMethod CreateResolver(UnityContainer unity, ImplicitRegistration registration, 
            InjectionConstructor ctor, IEnumerable<InjectionMember> members)
        {
            // Get resolvers for all injection members
            var memberResolvers = members.Select(m => m.CreateActivator(registration.Type))
                                         .ToList();

            // Get object activator
            var objectResolver = ctor.CreateActivator(registration.ImplementationType) ??
                                 throw new InvalidOperationException("Unable to create activator");    // TODO: Add proper error message

            if (0 == memberResolvers.Count)
            {
                return (ref ResolutionContext context) => objectResolver(ref context);
            }
            else
            {
                var dependencyResolvers = memberResolvers;
                return (ref ResolutionContext context) =>
                {
                    var resolver = objectResolver(ref context);
                    foreach (var resolveMethod in dependencyResolvers) resolveMethod(ref context);
                    return resolver;
                };
            }
        }

        #endregion
    }
}
