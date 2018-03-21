using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Unity.Attributes;
using Unity.Build.Injection;
using Unity.Registration;

namespace Unity.Build.Selection
{
    public static class SelectAttributedMembers
    {
        public static SelectConstructorPipeline SelectConstructorPipelineFactory(SelectConstructorPipeline next)
        {
            return (IUnityContainer container, InternalRegistration registration) =>
            {
                var type = registration.Type;
                ConstructorInfo constructor = null;
                foreach (var ctor in type.GetTypeInfo().DeclaredConstructors)
                {
                    if (ctor.IsStatic || !ctor.IsPublic) continue;

                    if (ctor.IsDefined(typeof(InjectionConstructorAttribute), true))
                    {
                        if (null != constructor)
                            throw new InvalidOperationException(
                                string.Format(CultureInfo.CurrentCulture, Constants.MultipleInjectionConstructors,
                                              type.GetTypeInfo().Name));

                        constructor = ctor;
                    }
                }

                if (null != constructor)
                    return new InjectionConstructor(constructor);

                return next?.Invoke(container, registration);
            };
        }

        public static SelectMethodsPipeline SelectMethodsPipelineFactory(SelectMethodsPipeline next)
        {
            return (IUnityContainer container, InternalRegistration registration) =>
            {
                return registration.Type.GetTypeInfo()
                                   .DeclaredMethods
                                   .Where(method => method.IsDefined(typeof(InjectionMethodAttribute), true))
                                   .Select(method => new InjectionMethod(method))
                                   .Concat(next?.Invoke(container, registration) ?? Enumerable.Empty<IInjectionMethod>());
            };
        }

        public static SelectPropertiesPipeline SelectPropertiesPipelineFactory(SelectPropertiesPipeline next)
        {
            return (IUnityContainer container, InternalRegistration registration) =>
            {
                return registration.Type.GetTypeInfo()
                                   .DeclaredProperties
                                   .Where(property => property.IsDefined(typeof(DependencyAttribute), true))
                                   .Select(property => new InjectionProperty(property))
                                   .Concat(next?.Invoke(container, registration) ?? Enumerable.Empty<IInjectionProperty>());
            };
        }



    }
}
