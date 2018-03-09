using System;
using System.Collections.Generic;
using System.Text;

namespace Unity.AspectFactories
{
    public static class Build
    {
        public static RegisterDelegate RegistrationAspectFactory(RegisterDelegate next)
        {
            // Create Setup registration aspect
            return (IUnityContainer container, ref RegistrationData data) =>
            {

                // Build rest of pipeline
                next?.Invoke(container, ref data);
            };
        }
    }
}
