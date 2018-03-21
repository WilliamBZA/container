using System.Collections.Generic;
using Unity.Build.Injection;
using Unity.Registration;

namespace Unity.Build.Selection
{
    public delegate IInjectionConstructor           SelectConstructorPipeline(IUnityContainer container, InternalRegistration registration);

    public delegate IEnumerable<IInjectionMethod>   SelectMethodsPipeline(IUnityContainer container,     InternalRegistration registration);

    public delegate IEnumerable<IInjectionProperty> SelectPropertiesPipeline(IUnityContainer container,  InternalRegistration registration);
}
