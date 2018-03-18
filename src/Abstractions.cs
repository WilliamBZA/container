using System;
using System.Collections.Generic;
using Unity.Build.Pipeline;
using Unity.Build.Selected;
using Unity.Builder;
using Unity.Dependency;
using Unity.Registration;

namespace Unity
{

    public class ResolveInfo : INamedType
    {
        public Type Type { get; set; }

        public string Name { get; set; }

        public ResolveInfo Previous { get; set; }
    }





    public delegate Resolve AspectFactoryDelegate(InternalRegistration registration, Resolve resolveDelegate);

    public interface ITypeBuildInfo
    {
        SelectedConstructor Constructor { get; set; }

        IEnumerable<SelectedProperty> Properties { get; set; }

        IEnumerable<SelectedMethod> Methods { get; set; }
    }

}
