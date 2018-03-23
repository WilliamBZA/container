using System;
using System.Collections.Generic;
using Unity.Registration;

namespace Unity.Build.Selection
{
    public delegate InjectionConstructor           SelectConstructorPipeline(IUnityContainer container, Type type);

    public delegate IEnumerable<InjectionMember>   InjectionMembersPipeline(IUnityContainer container, Type type);
}
