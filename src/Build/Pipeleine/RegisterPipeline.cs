using Unity.Policy;

namespace Unity.Build.Pipeleine
{
    public delegate void RegisterPipeline(IUnityContainer container, IPolicySet registration, params object[] args);
}
