using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Unity.Build.Context;
using Unity.Build.Factory;
using Unity.Build.Pipeleine;
using Unity.Build.Policy;
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
            return (IUnityContainer container, ImplicitRegistration registration, object[] args) =>
            {
                var explicitRegistration = (ExplicitRegistration)registration;

                // Add injection members policies to the registration
                if (null != args && 0 < args.Length)
                {
                    foreach (var member in args.OfType<InjectionMember>())
                    {
                        // Enforce no MappedToType with InjectionFactory
                        if (member is InjectionFactory && explicitRegistration.MappedToType != explicitRegistration.RegisteredType)
                            throw new InvalidOperationException("Registration where both MappedToType and InjectionFactory are set is not supported");

                        // Mark as requiring build if any of injectors marked with IRequireBuild
                        if (member is IRequireBuild) explicitRegistration.BuildRequired = true;

                        // Add policies
                        member.AddPolicies(explicitRegistration.RegisteredType, explicitRegistration.Name, explicitRegistration.MappedToType, explicitRegistration);
                    }
                }

                // Add to appropriate storage
                var target = explicitRegistration.LifetimeManager is ISingletonLifetimePolicy ? ((UnityContainer)container)._root : container;

                // Add or replace if exists 
                var previous = ((UnityContainer)target).Register(registration);
                if (previous is ExplicitRegistration old && old.LifetimeManager is IDisposable disposable)
                {
                    // Dispose replaced lifetime manager
                    ((UnityContainer)target)._lifetimeContainer.Remove(disposable);
                    disposable.Dispose();
                }

                // If Disposable add to container's lifetime
                if (explicitRegistration.LifetimeManager is IDisposable manager)
                    ((UnityContainer)target)._lifetimeContainer.Add(manager);

                // Build rest of pipeline
                next?.Invoke(container, registration);
                //if (null == registration.LifetimeManager.GetValue(((UnityContainer)target)._lifetimeContainer))
            };
        }

        public static RegisterPipeline DynamicRegistrationAspectFactory(RegisterPipeline next)
        {
            // Analise registration and generate mappings
            return (IUnityContainer container, ImplicitRegistration registration, object[] args) =>
            {
                var info = registration.Type.GetTypeInfo();
                
                // Generic types require implementation
                if (info.IsGenericType)     
                {
                    // Find implementation for this type
                    var unity = (UnityContainer) container;
                    var definition = info.GetGenericTypeDefinition();
                    var registry = unity._getType(definition) ??
                                   throw new InvalidOperationException("No such type"); // TODO: Add proper error message

                    // This registration must be present to proceed
                    var target = (string.IsNullOrEmpty(registration.Name) 
                               ? registry[null] ??
                                 registry[string.Empty]
                               : registry[registration.Name] ??
                                 registry[null] ?? 
                                 registry[string.Empty]) 
                               ?? throw new InvalidOperationException("No such type");    // TODO: Add proper error message

                    // Build rest of pipeline
                    next?.Invoke(container, registration, target, unity._lifetimeContainer);
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


        public static RegisterPipeline BuildAspectFactory(RegisterPipeline next)
        {
            return (IUnityContainer container, ImplicitRegistration registration, object[] args) =>
            {
                var unity = (UnityContainer) container;

                switch (registration)
                {
                    // Explicit registration
                    case ExplicitRegistration explicitRegistration:
                        var buildType = explicitRegistration.MappedToType ?? 
                                        explicitRegistration.RegisteredType;
                        var buildTypeInfo = buildType.GetTypeInfo();
                        if (null == buildType)
                        {
                        }
                        else if (buildTypeInfo.IsGenericTypeDefinition)   
                        {
                            // Create generic type factory
                            explicitRegistration.ResolveFactory = MakeGenericFactory((UnityContainer) container, explicitRegistration);
                        }
#if NET40
                        else if (buildTypeInfo.IsConstructedGenericType)
#else
                        else if (buildType.IsConstructedGenericType)
#endif
                        {
                        }
                        else
                        {
                            
                        }
                        break;

                    // Implicit registration
                    case ImplicitRegistration internalRegistration:
                        var info = internalRegistration.Type.GetTypeInfo();
                        if (info.IsGenericType)
                        {
                            //var targetType = (Type)args[0];
                            //var targetSet = (ExplicitRegistration)args[1];

                            //internalRegistration.ResolveMethod = targetSet.ResolveMethodFactory(targetType);

                            //// Build rest of pipeline
                            //next?.Invoke(container, set);
                            //return;
                        }
                        break;
                }

                // Build rest of pipeline
                next?.Invoke(container, registration);
            };
        }

        #endregion

        private static ResolveMethodFactory<Type> MakeGenericFactory(UnityContainer container, ExplicitRegistration registration)
        {
            InjectionConstructor ctor = null;
            var targetType = registration.MappedToType ?? registration.RegisteredType;
            var members = registration.PopType<InjectionMember>()
                                      .Where(m => 
                                          {
                                              if (!(m is InjectionConstructor constructor)) return true;
                                              ctor = constructor;
                                              return false;
                                          })
                                      .Cast<InjectionMember>()
                                      .Concat(container._injectionMembersPipeline(container, targetType));

            return (Type type) =>
            {
                var memberResolves = members.Select(m => m.ResolveFactory ?? (null != m.ResolveMethod ? t => m.ResolveMethod : (ResolveMethodFactory<Type>)null))
                                            .Where(f => null != f)
                                            .Select(f => f(type))
                                            .ToArray();
                var objectResolver = ctor.ResolveFactory(type);

                return (ref ResolutionContext context) =>
                {
                    context.Existing = objectResolver(ref context);
                    foreach (var resolveMethod in memberResolves) resolveMethod(ref context); // TODO: Could be executed in parallel
                    return  context.Existing;
                };
            };
        }
    }
}
