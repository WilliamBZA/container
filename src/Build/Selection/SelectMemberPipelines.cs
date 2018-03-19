using System.Collections.Generic;
using Unity.Build.Selected;
using Unity.Registration;

namespace Unity.Build.Selection.Constructor
{
    public delegate SelectedConstructor SelectConstructorPipeline(IUnityContainer container, InternalRegistration registration);

    public delegate IEnumerable<SelectedMethod> SelectMethodsPipeline(IUnityContainer container, InternalRegistration registration);

    public delegate IEnumerable<SelectedProperty> SelectPropertiesPipeline(IUnityContainer container, InternalRegistration registration);
}
