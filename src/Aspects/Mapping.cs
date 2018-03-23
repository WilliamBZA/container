using System;
using System.Reflection;
using Unity.Build.Pipeleine;
using Unity.Registration;

// ReSharper disable RedundantLambdaParameterType

namespace Unity.Aspects
{
    public static class MappingAspect
    {

        public static RegisterPipeline ExplicitRegistrationMappingAspectFactory(RegisterPipeline next)
        {
            // Analise registration and generate mappings
            return (IUnityContainer container, ImplicitRegistration registration, object[] args) =>
            {
                var explicitRegistration = (ExplicitRegistration) registration;
                if (null != explicitRegistration.MappedToType && explicitRegistration.RegisteredType != explicitRegistration.MappedToType)
                {
                    if (explicitRegistration.MappedToType.GetTypeInfo().IsGenericTypeDefinition)
                    {
                        // Create generic factory here 

                        // Really no need to do anything
                        var definition = explicitRegistration.MappedToType;
                        registration.Set(typeof(MapType), (MapType)((Type[] getArgs) => definition.MakeGenericType(getArgs)));
                    }

                    // Build rest of pipeline
                    next?.Invoke(container, registration, explicitRegistration.MappedToType);
                    return;
                }

                // Build rest of pipeline, no mapping required
                next?.Invoke(container, registration, args);
            };
        }

        public static RegisterPipeline ImplicitRegistrationMappingAspectFactory(RegisterPipeline next)
        {
            // Analise registration and generate mappings
            return (IUnityContainer container, ImplicitRegistration registration, object[] args) =>
            {
                var info = registration.Type.GetTypeInfo();
                if (info.IsGenericType)
                {
                    //// Find implementation for this type
                    //var definition = info.GetGenericTypeDefinition();
                    //var registry = ((UnityContainer)container)._getType(definition) ??
                    //               throw new InvalidOperationException("No such type");    // TODO: Add proper error message
                    //var target = registry[internalRegistration.Name] ??                     // TODO: Optimize these two calls
                    //             registry[null] ?? registry[string.Empty] ??
                    //             throw new InvalidOperationException("No such type");       // TODO: Add proper error message
                    //var builType = ((ExplicitRegistration)target).MappedToType;
                    //var map = target.Get<MapType>();

                    //// Build rest of pipeline
                    //next?.Invoke(container, registration, map(info.GenericTypeArguments), target);
                    //return;
                }

                // Build rest of pipeline, no mapping required
                next?.Invoke(container, registration, args);
            };
        }

    }
}
