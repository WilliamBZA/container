using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Unity.Attributes;
using Unity.Registration;

namespace Unity.Build.Selection
{
    public static class SelectAttributedMembers
    {
        public static SelectConstructorPipeline SelectConstructorPipelineFactory(SelectConstructorPipeline next)
        {
            return (IUnityContainer container, Type type) =>
            {
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

                return next?.Invoke(container, type);
            };
        }

        public static InjectionMembersPipeline SelectMethodsPipelineFactory(InjectionMembersPipeline next)
        {
            return (IUnityContainer container, Type type) =>
            {
                return type.GetTypeInfo()
                           .DeclaredMethods
                           .Where(method => method.IsDefined(typeof(InjectionMethodAttribute), true))
                           .Select(method => new InjectionMethod(method))
                           .Concat(next?.Invoke(container, type) ?? Enumerable.Empty<InjectionMethod>());
            };
        }

        public static InjectionMembersPipeline SelectPropertiesPipelineFactory(InjectionMembersPipeline next)
        {
            return (IUnityContainer container, Type type) =>
            {
                return type.GetTypeInfo()
                           .DeclaredProperties
                           .Where(property => property.IsDefined(typeof(DependencyAttribute), true))
                           .Select(property => new InjectionProperty(property))
                           .Concat(next?.Invoke(container, type) ?? Enumerable.Empty<InjectionProperty>());
            };
        }



    }
}
