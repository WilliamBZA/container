using System;
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
            return (IUnityContainer container, IPolicySet set, object[] args) =>
            {
                var registration = (ExplicitRegistration)set;

                // Add injection members policies to the registration
                if (null != args && 0 < args.Length)
                {
                    foreach (var member in args.OfType<InjectionMember>())
                    {
                        // Enforce no MappedToType with InjectionFactory
                        if (member is InjectionFactory && registration.MappedToType != registration.RegisteredType)
                            throw new InvalidOperationException("Registration where both MappedToType and InjectionFactory are set is not supported");

                        // Mark as requiring build if any of injectors marked with IRequireBuild
                        if (member is IRequireBuild) registration.BuildRequired = true;

                        // Add policies
                        member.AddPolicies(registration.RegisteredType, registration.Name, registration.MappedToType, registration);
                    }
                }

                // Add to appropriate storage
                var target = registration.LifetimeManager is ISingletonLifetimePolicy ? ((UnityContainer)container)._root : container;

                // Add or replace if exists 
                var previous = ((UnityContainer)target).Register((ImplicitRegistration)set);
                if (previous is ExplicitRegistration old && old.LifetimeManager is IDisposable disposable)
                {
                    // Dispose replaced lifetime manager
                    ((UnityContainer)target)._lifetimeContainer.Remove(disposable);
                    disposable.Dispose();
                }

                // If Disposable add to container's lifetime
                if (registration.LifetimeManager is IDisposable manager)
                    ((UnityContainer)target)._lifetimeContainer.Add(manager);

                // Build rest of pipeline
                next?.Invoke(container, set);
                //if (null == registration.LifetimeManager.GetValue(((UnityContainer)target)._lifetimeContainer))
            };
        }

        public static RegisterPipeline DynamicRegistrationAspectFactory(RegisterPipeline next)
        {
            // Analise registration and generate mappings
            return (IUnityContainer container, IPolicySet set, object[] args) =>
            {
                // TODO: add case of re-resolve

                // Build rest of pipeline, no mapping required
                next?.Invoke(container, set, args);
            };
        }

        #endregion


        #region Mapping aspect

        public static RegisterPipeline MappingAspectFactory(RegisterPipeline next)
        {
            // Analise registration and generate mappings
            return (IUnityContainer container, IPolicySet set, object[] args) =>
            {
                // TODO: add case of re - resolve

                switch (set)
                {
                    // Explicit registration
                    case ExplicitRegistration explicitRegistration:
                        if (null != explicitRegistration.MappedToType && explicitRegistration.RegisteredType != explicitRegistration.MappedToType)
                        {
                            if (explicitRegistration.MappedToType.GetTypeInfo().IsGenericTypeDefinition)
                            {
                                // Create generic factory here 

                                // Really no need to do anything
                                var definition = explicitRegistration.MappedToType;
                                set.Set(typeof(MapType), (MapType)((Type[] getArgs) => definition.MakeGenericType(getArgs)));
                            }

                            // Build rest of pipeline
                            next?.Invoke(container, set, explicitRegistration.MappedToType);
                            return;
                        }

                        break;

                    // Implicit registration
                    case ImplicitRegistration internalRegistration:
                        var info = internalRegistration.Type.GetTypeInfo();
                        if (info.IsGenericType)
                        {
                            var definition = info.GetGenericTypeDefinition();
                            var registry = ((UnityContainer) container)._getType(definition) ?? throw  new InvalidOperationException("No such type");                             // TODO: Add proper error message
                            var target = registry[internalRegistration.Name] ?? registry[null] ?? registry[string.Empty] ?? throw new InvalidOperationException("No such type");  // TODO: Add proper error message
                            
                            var map = target.Get<MapType>();

                            // Build rest of pipeline
                            next?.Invoke(container, set, map(info.GenericTypeArguments), target);
                            return;
                        }
                        break;
                }

                // Build rest of pipeline, no mapping required
                next?.Invoke(container, set, args);
            };
        }

        #endregion


        #region Build Aspect


        public static RegisterPipeline BuildAspectFactory(RegisterPipeline next)
        {
            return (IUnityContainer container, IPolicySet set, object[] args) =>
            {
                var unity = (UnityContainer) container;

                switch (set)
                {
                    // Explicit registration
                    case ExplicitRegistration explicitRegistration:
                        var buildType = explicitRegistration.MappedToType ?? 
                                        explicitRegistration.RegisteredType;

                        if (null == buildType)
                        {
                        }
                        else if (buildType.GetTypeInfo().IsGenericTypeDefinition)   // Create generic type factory
                        {
                            var ctor = unity._constructorSelectionPipeline(container, explicitRegistration);
                            var methods = unity._methodsSelectionPipeline(container,  explicitRegistration);
                            var properties = unity._propertiesSelectionPipeline(container, explicitRegistration);

                            explicitRegistration.ResolveMethodFactory = (Type createdType) =>
                            {
                                var ctorResolve = ctor.ResolveMethodFactory(createdType);
                                //var methodsResolve = methods.ResolveMethodFactory(createdType);
                                //var propertiesResolve = properties.ResolveMethodFactory(createdType);

                                return (ref ResolutionContext context) =>
                                {
                                    context.Existing = ctorResolve(ref context);
                                    //context.Existing = propertiesResolve(ref context);
                                    //context.Existing = methodsResolve(ref context);

                                    return context.Existing;
                                };
                            };
                        }
                        else if (buildType.IsConstructedGenericType)
                        {
                            
                        }
                        else
                        {
                            
                        }

                        // Build rest of pipeline
                        next?.Invoke(container, set, explicitRegistration.MappedToType);
                        return;

                        break;

                    // Implicit registration
                    case ImplicitRegistration internalRegistration:
                        var info = internalRegistration.Type.GetTypeInfo();
                        if (info.IsGenericType)
                        {
                            var targetType = (Type)args[0];
                            var targetSet = (ExplicitRegistration)args[1];

                            internalRegistration.ResolveMethod = targetSet.ResolveMethodFactory(targetType);

                            // Build rest of pipeline
                            next?.Invoke(container, set);
                            return;
                        }
                        break;
                }


                //Type type;
                //var container = (UnityContainer)unity;
                //var registration = (ImplicitRegistration)set;

                //switch (args?.Length)
                //{
                //    case 1:     // Mapped POCO
                //        type = (Type)args[0];
                //        break;

                //    case 2:     // Mapped generic with source
                //        type = (Type)args[0];
                //        registration = (ImplicitRegistration)args[1];
                //        break;

                //    default:    // No mapping    
                //        type = registration.Type;
                //        break;
                //}

                //var info = type.GetTypeInfo();

                //if (info.IsGenericTypeDefinition)
                //{


                //    // Add factory
                //    //set.Set(typeof(ResolveFactory<Type>), ctor.ResolveFactory);


                //    //buildMethod = (ref ResolutionContext c) =>
                //    //{
                //    //    ResolveMethod ctorMethod = ctor.ResolveFactory(type);
                //    //    //Resolve properties
                //    //    //Resolve methods

                //    //    buildMethod = (ref ResolutionContext context) =>
                //    //    {
                //    //        //context.Existing = createMethod(ref context);
                //    //        //context.Existing = properties(ref context);
                //    //        //context.Existing = methods(ref context);
                //    //        return context.Existing;
                //    //    };

                //    //    // Cleanup

                //    //    return buildMethod(ref c);
                //    //};
                //}
                //else if (type.IsConstructedGenericType)
                //{
                //    var factory = registration.Get<ResolveFactory<Type>>();
                //    ResolveMethod buildMethod = factory?.Invoke(type);

                //    // Cleanup
                //}

                //// Build rest of pipeline
                //next?.Invoke(unity, set, args);

                //// TODO: set.Clear(typeof(SelectConstructor)); // Release injectors and such
                //// TODO: Asynchronously create and compile expression

                //// Build
                //var pipeline = ((ImplicitRegistration)set).ResolveMethod;
                //((ImplicitRegistration)set).ResolveMethod = (ref ResolutionContext context) =>
                //{
                //    context.Existing = pipeline?.Invoke(ref context);// ??
                //    //                   buildMethod(ref context);

                //    return context.Existing;
                //};
            };
        }

        #endregion
    }
}
