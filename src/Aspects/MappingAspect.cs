using System.Reflection;
using Unity.Build.Context;
using Unity.Build.Pipeleine;
using Unity.Lifetime;
using Unity.Registration;
using Unity.Storage;

// ReSharper disable RedundantLambdaParameterType

namespace Unity.Aspects
{
    public static class MappingAspect
    {

        public static RegisterPipeline ExplicitRegistrationMappingAspectFactory(RegisterPipeline next)
        {
            // Analise registration and generate mappings
            return (ILifetimeContainer container, IPolicySet set, object[] args) =>
            {
                var registration = (ExplicitRegistration) set;
                if (null != registration.MappedToType && registration.Type != registration.MappedToType)
                {
                    if (registration.MappedToType.GetTypeInfo().IsGenericTypeDefinition)
                    {
                        // Create generic factory here 

                        // Really no need to do anything
                        //var definition = registration.MappedToType;
                    }

                    // Build rest of pipeline
                    next?.Invoke(container, set, registration.MappedToType);
                    return;
                }

                // Build rest of pipeline, no mapping required
                next?.Invoke(container, set, args);
            };
        }

        public static RegisterPipeline ImplicitRegistrationMappingAspectFactory(RegisterPipeline next)
        {
            // Analise registration and generate mappings
            return (ILifetimeContainer container, IPolicySet set, object[] args) =>
            {
                var registration = (ImplicitRegistration)set;
                var info = registration.Type.GetTypeInfo();

                // Create appropriate mapping if generic
                if (info.IsGenericType)
                {
                    var genericRegistration = (ExplicitRegistration)args[0];
                    var type = genericRegistration.MappedToType.MakeGenericType(info.GenericTypeArguments);
                    registration.MappedToType = type;
                }
                else if (!registration.BuildRequired && 
                         container.Container.IsRegistered(registration.MappedToType, registration.Name))
                {
                    registration.ResolveMethod = (ref ResolutionContext context) => 
                        context.Resolve(registration.MappedToType, registration.Name);
                }

                // Build rest of pipeline, no mapping required
                next?.Invoke(container, set, args);
            };
        }

    }
}
