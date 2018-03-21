using System.Linq;
using Unity.Build.Injection;
using Unity.Registration;

namespace Unity.Build.Selection
{
    public static class SelectInjectionMembers
    {
        public static SelectConstructorPipeline SelectConstructorPipelineFactory(SelectConstructorPipeline next)
        {
            return (IUnityContainer container, InternalRegistration registration) => 
                (IInjectionConstructor)(registration.Get(typeof(IInjectionConstructor)) ?? next?.Invoke(container, registration));
        }


        public static SelectMethodsPipeline SelectMethodsPipelineFactory(SelectMethodsPipeline next)
        {
            return (IUnityContainer container, InternalRegistration registration) =>
                registration.OfType<IInjectionMethod>().Cast<IInjectionMethod>().Concat(next?.Invoke(container, registration) ?? 
                                         Enumerable.Empty<IInjectionMethod>());
        }


        public static SelectPropertiesPipeline SelectPropertiesPipelineFactory(SelectPropertiesPipeline next)
        {
            return (IUnityContainer container, InternalRegistration registration) => 
                registration.OfType<IInjectionProperty>().Cast<IInjectionProperty>().Concat(next?.Invoke(container, registration) ??
                Enumerable.Empty<IInjectionProperty>());
        }
    }
}
