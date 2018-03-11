using System;
using System.Collections.Generic;
using Unity.Builder;
using Unity.Builder.Selection;
using Unity.Registration;
using Unity.Resolution;

namespace Unity
{

    public class ResolveInfo : INamedType
    {
        public Type Type { get; set; }

        public string Name { get; set; }

        public ResolveInfo Previous { get; set; }
    }





    public delegate ResolvePipeline AspectFactoryDelegate(InternalRegistration registration, ResolvePipeline resolveDelegate);

    public interface ITypeBuildInfo
    {
        SelectedConstructor Constructor { get; set; }

        IEnumerable<SelectedProperty> Properties { get; set; }

        IEnumerable<SelectedMethod> Methods { get; set; }
    }

}
