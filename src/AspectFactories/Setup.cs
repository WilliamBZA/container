using Unity.Registration;

namespace Unity.AspectFactories
{
    public static class Setup
    {
        public static RegisterDelegate RegistrationAspectFactory(RegisterDelegate next)
        {
            // Create Setup registration aspect
            return (IUnityContainer container, ref RegistrationData data) =>
            {
                // Add Injection Members
                InjectionMember[] injectionMembers = data.InjectionMembers;
                if (null != injectionMembers && 0 < injectionMembers.Length && 
                    data.Registration is StaticRegistration registration)
                {
                    if (null != injectionMembers && injectionMembers.Length > 0)
                    {
                        foreach (var member in injectionMembers)
                        {
                            member.AddPolicies(registration.RegisteredType, registration.Name, registration.MappedToType, registration);
                        }
                    }
                }

                // Build rest of pipeline
                next?.Invoke(container, ref data);
            };
        }
    }
}
