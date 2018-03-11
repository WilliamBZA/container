using System;
using Unity.Policy;
using Unity.Registration;
using Unity.Resolution;

namespace Unity.AspectFactories
{
    public static class Creation
    {
        public static RegisterPipeline RegistrationAspectFactory(RegisterPipeline next)
        {
            return (IUnityContainer container, IPolicySet registration, Type type, string name) =>
            {
                // Get Constructor info
                if (null != type)
                {
                    ((ITypeBuildInfo)registration).Constructor = registration.Get<SelectConstructorPipeline>()?
                                                                             .Invoke(container, type, name); 
                }

                // Build rest of pipeline
                next?.Invoke(container, registration, type, name);

                // Create
                var pipeline = ((InternalRegistration)registration).Resolve;
                ((InternalRegistration)registration).Resolve = (ref ResolutionContext context) =>
                {
                    if (null != context.Existing) return pipeline?.Invoke(ref context);


                    return pipeline?.Invoke(ref context) ?? context.Existing;
                };

                // Asynchronously optimaize
            };
        }
    }
}
