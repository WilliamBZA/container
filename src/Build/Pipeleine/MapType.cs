using System;

namespace Unity.Build.Pipeleine
{
    public delegate Type MapType(Type[] args);


    public interface IMapType
    {
        MapType MapType { get; set; }
    }

}
