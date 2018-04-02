using System.Reflection;
using Unity.Build.Context;
using Unity.Build.Pipeline;
using Unity.Container.Registration;
using Unity.Lifetime;
using Unity.Storage;

// ReSharper disable RedundantLambdaParameterType

namespace Unity.Aspect.Build
{
    public static class BuildMappingAspect
    {

        public static RegisterPipeline ExplicitRegistrationMappingAspectFactory(RegisterPipeline next)
        {
            // Bypassed
            return next;

            //// Analise registration and generate mappings
            //return (ILifetimeContainer container, IPolicySet set, object[] args) =>
            //{
            //    // Build rest of pipeline, no mapping required
            //    next?.Invoke(container, set, args);
            //};
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
                    registration.MappedToType = genericRegistration.MappedToType
                                                                   .MakeGenericType(info.GenericTypeArguments);
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
