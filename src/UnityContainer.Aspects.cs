using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Build.Context;
using Unity.Build.Pipeleine;
using Unity.Build.Pipeline;
using Unity.Build.Policy;
using Unity.Container.Registration;
using Unity.Lifetime;
using Unity.Registration;
using Unity.Storage;

namespace Unity
{
    public partial class UnityContainer
    {
        #region Registration Aspects

        private static RegisterPipeline StaticRegistrationAspectFactory(RegisterPipeline next)
        {
            // Setup and add registration to container
            return (ILifetimeContainer container, IPolicySet set, object[] args) =>
            {
                // Add injection members policies to the registration
                var registration = (ExplicitRegistration)set;
                if (null != args && 0 < args.Length)
                {
                    foreach (var member in args.OfType<InjectionMember>())
                    {
                        // Validate against MappedToType with InjectionFactory
                        if (member is InjectionFactory && registration.MappedToType != registration.Type)  // TODO: Add proper error message
                            throw new InvalidOperationException("Registration where both MappedToType and InjectionFactory are set is not supported");

                        // Mark as requiring build if any one of the injectors are marked with IRequireBuild
                        if (member is IRequireBuild) registration.BuildRequired = true;

                        // Add policies
                        member.AddPolicies(registration.Type, registration.Name, registration.MappedToType, registration);
                    }
                }

                // Add to appropriate storage
                var lifetime = ((ExplicitRegistration)registration).LifetimeManager;
                var unity = lifetime is ISingletonLifetimePolicy ? ((UnityContainer)container.Container)._root : (UnityContainer)container.Container;

                // Add or replace if exists 
                var previous = unity.Register(registration);
                if (previous is ExplicitRegistration old && old.LifetimeManager is IDisposable disposableOld)
                {
                    // Dispose replaced lifetime manager
                    unity._lifetimeContainer.Remove(disposableOld);
                    disposableOld.Dispose();
                }

                // If Disposable add to container's lifetime
                if (lifetime is IDisposable) unity._lifetimeContainer.Add(lifetime);

                // Build rest of pipeline
                next?.Invoke(container, registration);
            };
        }

        public static RegisterPipeline DynamicRegistrationAspectFactory(RegisterPipeline next)
        {
            // Analise registration and generate mappings
            return (ILifetimeContainer container, IPolicySet set, object[] args) =>
            {
                var registration = (ImplicitRegistration)set;
                var info = registration.Type.GetTypeInfo();
                
                // Generic types require implementation
                if (info.IsGenericType)     
                {
                    // Find implementation for this type
                    var unity = (UnityContainer) container.Container;
                    var definition = info.GetGenericTypeDefinition();
                    var registry = unity._getType(definition) ??
                                   throw new InvalidOperationException("No such type"); // TODO: Add proper error message

                    // This registration must be present to proceed
                    var target = (null == registration.Name 
                               ? registry[null]
                               : registry[registration.Name] ?? registry[null]) 
                               ?? throw new InvalidOperationException("No such type");    // TODO: Add proper error message

                    // Build rest of pipeline
                    next?.Invoke(container, registration, target);
                }
                else
                {
                    // Build rest of pipeline
                    next?.Invoke(container, registration);
                }
            };
        }

        #endregion


        #region Build Aspect

        public static RegisterPipeline BuildExplicitRegistrationAspectFactory(RegisterPipeline next)
        {
            return (ILifetimeContainer container, IPolicySet set, object[] args) =>
            {

                // Build rest of pipeline
                next?.Invoke(container, set, args);
                                                                                                                                                                         

                // Check if anyone wants to hijack create process: if resolver exists, no need to do anything
                var registration = (ExplicitRegistration) set;
                if (null != registration.ResolveMethod)
                {
                    registration.EnableOptimization = false;
                    return;
                }

                // Analise the type and select build strategy
                var buildTypeInfo = registration.MappedToType.GetTypeInfo();
                if (buildTypeInfo.IsGenericTypeDefinition)
                {
                    // Create generic type factory
                    InjectionConstructor ctor = null;
                    var unity = (UnityContainer)container.Container;
                    var targetType = registration.MappedToType ?? registration.Type;

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
                    registration.Resolver = (Type type) =>
                    {
                        // Create resolvers for each injection member
                        var memberResolvers = members.Select(m => m.Resolver)
                                                     .Where(f => null != f)
                                                     .Select(f => f(type))
                                                     .ToArray();

                        // Create object activator
                        var objectResolver = ctor.Resolver(type);

                        // Create composite resolver
                        return (ref ResolutionContext context) =>
                        {
                            try
                            {
                                context.Existing = objectResolver(ref context);
                                foreach (var resolveMethod in memberResolvers) resolveMethod(ref context); // TODO: Could be executed in parallel
                            }
                            catch (Exception e)
                            {
                                throw new InvalidOperationException($"Error creating object of type: {ctor.Constructor.DeclaringType}", e);
                            }

                            return context.Existing;
                        };
                    };
                }
                else
                {
                    var unity = (UnityContainer)container.Container;
                    var members = registration.OfType<InjectionMember>()
                                              .Cast<InjectionMember>()
                                              .Where(m => !(m is InjectionConstructor))
                                              .Concat(unity._injectionMembersPipeline(unity, registration.MappedToType));
                    registration.ResolveMethod = CreateResolver(unity, registration, set.Get<InjectionConstructor>() ??
                        unity._constructorSelectionPipeline(unity, registration.MappedToType), members);
                }
            };
        }


        public static RegisterPipeline BuildImplicitRegistrationAspectFactory(RegisterPipeline next)
        {
            return (ILifetimeContainer container, IPolicySet set, object[] args) =>
            {
                // Build rest of the pipeline first
                next?.Invoke(container, set, args);

                // if resolver has been provided, no need to do anything
                var registration = (ImplicitRegistration)set;
                if (null != registration.ResolveMethod)
                {
                    registration.EnableOptimization = false;
                    return;
                }

                var info = registration.Type.GetTypeInfo();
                if (info.IsGenericType)
                {
                    var genericRegistration = (ExplicitRegistration)args[0];
                    registration.ResolveMethod = genericRegistration.Resolver?.Invoke(registration.MappedToType) 
                                               ?? throw new InvalidOperationException("Unable to create resolver");    // TODO: Add proper error message
                }
                else
                {
                    var unity = (UnityContainer) container.Container;
                    registration.ResolveMethod = CreateResolver(unity, registration,
                        unity._constructorSelectionPipeline(unity, registration.MappedToType),
                        unity._injectionMembersPipeline(unity, registration.MappedToType));
                }
            };
        }

        #endregion


        #region Implementation

        private static ResolveMethod CreateResolver(UnityContainer unity, ImplicitRegistration registration, 
            InjectionConstructor ctor, IEnumerable<InjectionMember> members)
        {
            // Get resolvers for all injection members
            var memberResolvers = members.Select(m => m.Resolver(registration.Type))
                                         .ToList();

            // Get object activator
            var objectResolver = ctor.Resolver(registration.MappedToType) ??
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
                    context.Existing = objectResolver(ref context);
                    foreach (var resolveMethod in dependencyResolvers) resolveMethod(ref context);
                    return context.Existing;
                };
            }
        }

        #endregion
    }
}
