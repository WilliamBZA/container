using System;
using System.Reflection;
using Unity.Build.Context;
using Unity.Build.Pipeline;
using Unity.Policy;
using Unity.Registration;

namespace Unity.AspectFactories
{
    public static class Build
    {
        public static RegisterPipeline RegistrationAspectFactory(RegisterPipeline next)
        {
            return (IUnityContainer container, IPolicySet registration, Type type, string name) =>
            {
                ((ITypeBuildInfo)registration).Constructor = registration.Get<SelectConstructor>()?
                                                                         .Invoke(type);


                // Build rest of pipeline
                next?.Invoke(container, registration, type, name);

                // Create
                var activate = type.GetTypeInfo().IsGenericTypeDefinition ? null : ((ITypeBuildInfo)registration).Constructor.CreateResolver(type);
                var pipeline = ((InternalRegistration)registration).Resolve;
                ((InternalRegistration)registration).Resolve = (ref ResolutionContext context) =>
                {
                    if (null == context.Existing)
                    {
                        if (null == activate)
                            activate = ((ITypeBuildInfo)registration).Constructor.CreateResolver(type);

                        context.Existing = activate(ref context);
                    }

                    return activate?.Invoke(ref context) ?? context.Existing;
                };

                // Asynchronously optimaize
            };
        }
    }
}
