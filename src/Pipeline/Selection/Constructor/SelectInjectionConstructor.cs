using System;
using System.Globalization;
using System.Reflection;
using Unity.Attributes;
using Unity.Build.Pipeline;
using Unity.Build.Selected;

namespace Unity.Select.Constructor
{
    public static class SelectInjectionConstructor
    {
        public static SelectConstructor SelectConstructorPipelineFactory(SelectConstructor next)
        {
            // Create Factory Method registration aspect
            return (Type type) =>
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
                    return new SelectedConstructor(constructor);

                return next?.Invoke(type);
            };
        }
    }
}
