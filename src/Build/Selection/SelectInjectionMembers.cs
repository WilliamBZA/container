using System.Linq;
using Unity.Build.Selected;
using Unity.Build.Selection.Constructor;
using Unity.Registration;

namespace Unity.Select.Constructor
{
    public static class SelectInjectionMembers
    {
        public static SelectConstructorPipeline SelectConstructorPipelineFactory(SelectConstructorPipeline next)
        {
            return (IUnityContainer container, InternalRegistration registration) =>
            {
                return (SelectedConstructor)(registration.Get(typeof(SelectedConstructor)) ?? next?.Invoke(container, registration));
            };
        }


        public static SelectMethodsPipeline SelectMethodsPipelineFactory(SelectMethodsPipeline next)
        {
            return (IUnityContainer container, InternalRegistration registration) =>
            {
                return Enumerable.Concat(registration.OfType<SelectedMethod>().Cast<SelectedMethod>(), 
                                         next?.Invoke(container, registration) ?? 
                                         Enumerable.Empty<SelectedMethod>());
            };
        }


        public static SelectPropertiesPipeline SelectPropertiesPipelineFactory(SelectPropertiesPipeline next)
        {
            return (IUnityContainer container, InternalRegistration registration) =>
            {
                return Enumerable.Concat(registration.OfType<SelectedProperty>().Cast<SelectedProperty>(),
                                         next?.Invoke(container, registration) ??
                                         Enumerable.Empty<SelectedProperty>());
            };
        }
    }
}
