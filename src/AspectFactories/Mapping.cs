using System;
using System.Reflection;
using Unity.Policy;
using Unity.Registration;
using Unity.Resolution;
using Unity.Storage;

namespace Unity
{
    public partial class UnityContainer
    {
        public static class Mapping
        {
            public static RegisterPipeline RegistrationAspectFactory(RegisterPipeline next)
            {
                return (IUnityContainer container, IPolicySet registration, Type type, string name) =>
                {
                    // Build rest of pipeline
                    next?.Invoke(container, registration, type, name);
                };
            }




            public static ResolvePipeline AspectFactory(InternalRegistration registration, ResolvePipeline next)
            {
                if (registration is StaticRegistration staticRegistration)
                {
                    if (null == staticRegistration.MappedToType || ReferenceEquals(staticRegistration.RegisteredType, staticRegistration.MappedToType))
                        return next;

                    // Analyse Static Registration
                    foreach (var policy in registration)
                    {
                        // Build MappedTo type
                        if (policy is IRequireBuildUpPolicy)
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
