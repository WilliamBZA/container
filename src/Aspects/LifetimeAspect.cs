using Unity.Build.Context;
using Unity.Build.Pipeleine;
using Unity.Lifetime;
using Unity.Policy;
using Unity.Registration;

namespace Unity.Aspects
{
    public static class LifetimeAspect
    {
        public static RegisterPipeline LifetimeAspectFactory(RegisterPipeline next)
        {
            // Create Lifetime registration aspect
            return (IUnityContainer container, IPolicySet set, object[] args) =>
            {
                // Build rest of pipeline
                next?.Invoke(container, set, args);

                // Create aspect
                var lifetime = set is StaticRegistration staticRegistration
                             ? staticRegistration.LifetimeManager
                             : (LifetimeManager)set.Get(typeof(ILifetimePolicy));

                // No lifetime management if null or Transient
                if (null == lifetime || lifetime is TransientLifetimeManager) return;

                // Add aspect
                var pipeline = ((InternalRegistration)set).ResolveMethod;
                if (null == pipeline)
                    ((InternalRegistration)set).ResolveMethod = (ref ResolutionContext context) => lifetime.GetValue(context.LifetimeContainer);
                else
                    ((InternalRegistration)set).ResolveMethod = (ref ResolutionContext context) =>
                    {
                        var value = lifetime.GetValue(context.LifetimeContainer);
                        if (null != value) return value;
                        value = pipeline(ref context);
                        lifetime.SetValue(value, context.LifetimeContainer);
                        return value;
                    };
            };
        }
    }
}
