using System;
using Unity.Policy;

namespace Unity.AspectFactories
{
    public static class Properties
    {
        public static RegisterPipeline RegistrationAspectFactory(RegisterPipeline next)
        {
            return (IUnityContainer container, IPolicySet registration, Type type, string name) =>
            {
                // Build rest of pipeline
                next?.Invoke(container, registration, type, name);
            };
        }
    }
}
