using System;
using Unity.Build.Pipeline;
using Unity.Builder;
using Unity.Registration;

namespace Unity
{
    public class ResolveInfo : INamedType
    {
        public Type Type { get; set; }

        public string Name { get; set; }

        public ResolveInfo Previous { get; set; }
    }
    

    public delegate ResolveMethod AspectFactoryDelegate(InternalRegistration registration, ResolveMethod resolveDelegate);

}
