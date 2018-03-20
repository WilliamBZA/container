using System;
using System.Linq;
using System.Reflection;
using Unity.Build.Context;
using Unity.Build.Pipeleine;
using Unity.Build.Pipeline;
using Unity.Policy;
using Unity.Registration;

namespace Unity
{
    public partial class UnityContainer
    {
        public static class Mapping
        {
            public static RegisterPipeline MappingAspectFactory(RegisterPipeline next)
            {
                // Analyse registration and generate mappings
                return (IUnityContainer container, IPolicySet set, Type type, string name) =>
                {
                    if (set is StaticRegistration staticRegistration)
                    {
                        if (null != staticRegistration.MappedToType && staticRegistration.RegisteredType != staticRegistration.MappedToType)
                        {
                            var buildRequired = staticRegistration.LifetimeManager is IRequireBuildUpPolicy ||
                                                staticRegistration.OfType<IRequireBuildUpPolicy>().Any();

                            // Build
                            if (staticRegistration.RegisteredType.GetTypeInfo().IsGenericTypeDefinition &&
                                staticRegistration.MappedToType.GetTypeInfo().IsGenericTypeDefinition)
                            {
                                var definition = staticRegistration.MappedToType;
                                MapTypePipeleine pipeleine = (Type[] args) => definition.MakeGenericType(args);
                                staticRegistration.ResolveMethod = (ref ResolutionContext context) => throw new InvalidOperationException("Attempting to build open generic type"); 
                                set.Set(typeof(MapTypePipeleine), pipeleine);
                            }
                            else
                            {
                                var pipeline = staticRegistration.ResolveMethod;
                                staticRegistration.ResolveMethod = (ref ResolutionContext context) =>
                                {
                                    context.ImplementationType = staticRegistration.MappedToType;
                                    return pipeline?.Invoke(ref context);
                                };
                            }
                        }
                    }
                    else if (set is InternalRegistration internalRegistration)
                    {
                        var info = internalRegistration.Type.GetTypeInfo();
                        if (info.IsGenericType)
                        {
                            var definition = info.GetGenericTypeDefinition();
                            var arguments = info.GenericTypeArguments;
                            var map = (MapTypePipeleine)((UnityContainer)container)._get(definition, internalRegistration.Name, typeof(MapTypePipeleine)) ??
                                      (MapTypePipeleine)((UnityContainer)container)._get(definition, string.Empty, typeof(MapTypePipeleine));
                            var implementationType = map?.Invoke(info.GenericTypeArguments);
                            var pipeline = internalRegistration.ResolveMethod;
                            if (null == implementationType)
                            {
                                internalRegistration.ResolveMethod = (ref ResolutionContext context) =>
                                {
                                    var runtimeMap = (MapTypePipeleine)context.Get(definition, internalRegistration.Name, typeof(MapTypePipeleine)) ??
                                              (MapTypePipeleine)context.Get(definition, string.Empty, typeof(MapTypePipeleine));

                                    context.ImplementationType = runtimeMap?.Invoke(arguments);
                                    return pipeline?.Invoke(ref context);
                                };
                            }
                            else
                            {
                                internalRegistration.ResolveMethod = (ref ResolutionContext context) =>
                                {
                                    context.ImplementationType = implementationType;
                                    return pipeline?.Invoke(ref context);
                                };
                            }
                        }
                    }

                    // Build rest of pipeline
                    next?.Invoke(container, set, type, name);
                };
            }




            public static ResolveMethod AspectFactory(InternalRegistration registration, ResolveMethod next)
            {
                if (registration is StaticRegistration staticRegistration)
                {
                    if (null == staticRegistration.MappedToType || ReferenceEquals(staticRegistration.RegisteredType, staticRegistration.MappedToType))
                        return next;

                    // Analyse Static Registration
                    foreach (var policy in registration.OfType<IRequireBuildUpPolicy>())
                    {
                        return (ref ResolutionContext context) =>
                        {
                            //context.Target = new LinkedNode<Type, IPolicySet>
                            //{
                            //    Key = staticRegistration.MappedToType,
                            //    Value = context.Registration,
                            //    Next = context.Target
                            //};
                            return next(ref context);
                        };
                    }

                    // Resolve MappedTo type
                    return (ref ResolutionContext context) =>
                    {
                        //context.Type = staticRegistration.MappedToType;


                        return next(ref context);
                    };
                }
                else if (registration.Type.GetTypeInfo().IsGenericType)
                {
                    // Analyse Dynamic Registration

                    return next;
                }
                else
                    return next;
            }

            private static object BuildKeyUpMapping(ref ResolutionContext context)
            {
                throw new NotImplementedException();
            }


        }
    }
}
