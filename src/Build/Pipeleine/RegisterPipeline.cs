using Unity.Registration;

namespace Unity.Build.Pipeleine
{
    public delegate void RegisterPipeline(IUnityContainer container, ImplicitRegistration registration, params object[] args);
}
