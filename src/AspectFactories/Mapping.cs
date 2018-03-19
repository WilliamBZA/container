using System;
using System.Reflection;
using Unity.Build.Context;
using Unity.Build.Pipeline;
using Unity.Policy;
using Unity.Registration;

namespace Unity
{
    public partial class UnityContainer
    {
        public static class Mapping
        {
            public static RegisterPipeline RegistrationAspectFactory(RegisterPipeline next)
            {
                return (IUnityContainer container, IPolicySet set, Type type, string name) =>
                {
                    // Build rest of pipeline
                    next?.Invoke(container, set, type, name);
                };
            }




            public static ResolveMethod AspectFactory(InternalRegistration registration, ResolveMethod next)
            {
                if (registration is StaticRegistration staticRegistration)
                {
                    if (null == staticRegistration.MappedToType || ReferenceEquals(staticRegistration.RegisteredType, staticRegistration.MappedToType))
                        return next;

                    // Analyse Static Registration
                    foreach (var policy in registration.OfType<IRequireBuildUpPolicy>())
                    {
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
