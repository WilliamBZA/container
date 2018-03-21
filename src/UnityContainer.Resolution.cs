using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Build.Context;
using Unity.Build.Pipeleine;
using Unity.Builder;
using Unity.Policy;
using Unity.Registration;

namespace Unity
{
    // Resolving Engine Implementation 
    public partial class UnityContainer
    {
        #region Mapping aspect

        public static RegisterPipeline MappingAspectFactory(RegisterPipeline next)
        {
            // Analyse registration and generate mappings
            return (IUnityContainer container, IPolicySet set, object[] args) =>
            {
                // TODO: add case of re - resolve

                // Statically registered type
                switch (set)
                {
                    case StaticRegistration staticRegistration:
                        if (null != staticRegistration.MappedToType && staticRegistration.RegisteredType != staticRegistration.MappedToType )
                        {
                            if (staticRegistration.MappedToType.GetTypeInfo().IsGenericTypeDefinition)
                            {
                                // TODO: Add proper error message
                                staticRegistration.ResolveMethod = (ref ResolutionContext context) => throw new InvalidOperationException("Attempting to build open generic type");

                                var definition = staticRegistration.MappedToType;
                                set.Set(typeof(MapTypePipeleine), (MapTypePipeleine)((Type[] getArgs) => definition.MakeGenericType(getArgs)));
                            }

                            // Build rest of pipeline
                            next?.Invoke(container, set, staticRegistration.MappedToType);
                            return;
                        }

                        break;

                    case InternalRegistration internalRegistration:
                        var info = internalRegistration.Type.GetTypeInfo();
                        if (info.IsGenericType)
                        {
                            var definition = info.GetGenericTypeDefinition();
                            var target = ((UnityContainer)container)._getRegistration(definition, internalRegistration.Name) ??
                                         ((UnityContainer)container)._getRegistration(definition, string.Empty) ??  // TODO: Check if this second check could be removed
                                          throw new InvalidOperationException("Trying to build interface");         // TODO: Add proper error message

                            var mapTypePipeleine = target.Get<MapTypePipeleine>() ?? 
                                                   throw new InvalidOperationException("Not generic base");         // TODO: Add proper error message

                            // Build rest of pipeline
                            next?.Invoke(container, set, mapTypePipeleine?.Invoke(info.GenericTypeArguments), target);
                            return;
                        }
                        break;
                }

                // Build rest of pipeline, no mapping required
                next?.Invoke(container, set, args);
            };
        }

        #endregion



        #region Resolving Enumerables


        internal static void ResolveArray<T>(IBuilderContext context)
        {
            var container = (UnityContainer)context.Container;
            var list = new List<T>();

            var registrations = (IList<InternalRegistration>)GetNamedRegistrations(container, typeof(T));
            for (var i = 0; i < registrations.Count; i++)
            {
                var registration = registrations[i];

                if (registration.Type.GetTypeInfo().IsGenericTypeDefinition)
                    list.Add((T)((BuilderContext)context).NewBuildUp(typeof(T), registration.Name));
                else
                    list.Add((T)((BuilderContext)context).NewBuildUp(registration));
            }

            context.Existing = list.ToArray();
            context.BuildComplete = true;
        }

        internal static void ResolveEnumerable<T>(IBuilderContext context)
        {
            var container = (UnityContainer)context.Container;
            var list = new List<T>();

            var registrations = (IList<InternalRegistration>)GetNotEmptyRegistrations(container, typeof(T));
            for (var i = 0; i < registrations.Count; i++)
            {
                var registration = registrations[i];

                if (registration.Type.GetTypeInfo().IsGenericTypeDefinition)
                    list.Add((T)((BuilderContext)context).NewBuildUp(typeof(T), registration.Name));
                else
                    list.Add((T)((BuilderContext)context).NewBuildUp(registration));
            }

            context.Existing = list;
            context.BuildComplete = true;
        }

        #endregion
    }
}
