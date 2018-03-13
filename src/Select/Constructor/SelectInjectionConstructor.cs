using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Unity.Attributes;
using Unity.Dependency;

namespace Unity.Select.Constructor
{
    public static class SelectInjectionConstructor
    {
        public static SelectConstructorPipeline SelectConstructorPipelineFactory(SelectConstructorPipeline next)
        {
            // Create Factory Method registration aspect
            return (IUnityContainer container, Type type, string name) =>
            {
                var constructors = type.GetTypeInfo()
                                       .DeclaredConstructors
                                       .Where(c => c.IsStatic == false && c.IsPublic &&
                                                   c.IsDefined(typeof(InjectionConstructorAttribute), true))
                                       .ToArray();
                switch (constructors.Length)
                {
                    case 0:
                        return next?.Invoke(container, type, name);

                    case 1:
                        return new SelectedConstructor(constructors[0]);

                    default:
                        throw new InvalidOperationException(
                            string.Format(CultureInfo.CurrentCulture, Constants.MultipleInjectionConstructors,
                                          type.GetTypeInfo().Name));
                }
            };
        }
    }
}
