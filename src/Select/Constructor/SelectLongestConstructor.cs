using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Unity.Builder.Selection;
using Unity.Utility;

namespace Unity.Select.Constructor
{
    public static class SelectLongestConstructor
    {
        public static SelectConstructorPipeline SelectConstructorPipelineFactory(SelectConstructorPipeline next)
        {
            // Create Factory Method registration aspect
            return (IUnityContainer container, Type type, string name) =>
            {
                // TODO: Could be optimized for performance
                ConstructorInfo[] constructors = type.GetTypeInfo()
                                                     .DeclaredConstructors
                                                     .Where(c => c.IsStatic == false && c.IsPublic)
                                                     .ToArray();
                Array.Sort(constructors, new ConstructorLengthComparer());

                switch (constructors.Length)
                {
                    case 0:
                        return next?.Invoke(container, type, name);

                    case 1:
                        return new SelectedConstructor(constructors[0]);

                    default:
                        int paramLength = constructors[0].GetParameters().Length;
                        if (constructors[1].GetParameters().Length == paramLength)
                        {
                            throw new InvalidOperationException(
                                string.Format(CultureInfo.CurrentCulture, Constants.AmbiguousInjectionConstructor,
                                              type.GetTypeInfo().Name, paramLength));
                        }
                        return new SelectedConstructor(constructors[0]);
                }
            };
        }
    }
}
