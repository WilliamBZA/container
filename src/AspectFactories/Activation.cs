using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Attributes;
using Unity.Builder.Selection;
using Unity.Policy;
using Unity.Registration;
using Unity.Resolution;

namespace Unity
{
    public partial class UnityContainer
    {
        public static class Activation
        {
            public static RegisterPipeline RegistrationAspectFactory(RegisterPipeline next)
            {
                // Create Factory Method registration aspect
                return (IUnityContainer container, IPolicySet registration, Type type, string name) =>
                {
                    // Build rest of pipeline
                    next?.Invoke(container, registration, type, name);

                    // Activate
                    var pipeline = ((InternalRegistration)registration).Resolve;
                    ((InternalRegistration)registration).Resolve = (ref ResolutionContext context) =>
                    {
                        if (null != context.Existing) return pipeline?.Invoke(ref context);

                        SelectedConstructor constructor = ((ITypeBuildInfo)registration).Constructor ??
                                                                           registration.Get<SelectConstructorPipeline>()?
                                                                                       .Invoke(context.Container, context.Type,
                                                                                           ((InternalRegistration)context.Registration).Name);
                        var args = new List<object>();

                        ResolutionContext paramContext = new ResolutionContext
                        {
                            Container = context.Container,
                        }; 

                        foreach (ParameterInfo parameter in constructor.Constructor.GetParameters())
                        {
                            var attribute = (DependencyResolutionAttribute)parameter.GetCustomAttribute(typeof(DependencyResolutionAttribute));

                            Type parameterType = parameter.ParameterType;
                            string parameterName = attribute?.Name;

                            //context.Existing = context.Resolve(parameterType, parameterName, attribute is OptionalDependencyAttribute);

                            //var value = context.Resolve();

                            //var reg = ((UnityContainer)context.Container).GetRegistration(parameter.ParameterType, name);

                            //return registration.Resolve(ref context);

                        }

                        context.Existing = Activator.CreateInstance(context.Type, args.ToArray());

                        return pipeline?.Invoke(ref context) ?? context.Existing;
                    };
                };
            }

        }
    }
}
