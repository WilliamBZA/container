using Unity.Build.Context;
using Unity.Build.Pipeleine;
using Unity.Build.Policy;
using Unity.Lifetime;
using Unity.Registration;
using Unity.Storage;

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

                LifetimeManager lifetime;
                if (set is ExplicitRegistration staticRegistration)
                {
                    lifetime = staticRegistration.LifetimeManager;
                    if (lifetime is IRequireBuild) staticRegistration.BuildRequired = true;
                }
                else
                {
                    lifetime = (LifetimeManager)set.Get(typeof(ILifetimePolicy));
                }


                // No lifetime management if null or Transient
                if (null == lifetime || lifetime is TransientLifetimeManager) return;

                // Add aspect
                var pipeline = ((ImplicitRegistration)set).ResolveMethod;
                if (null == pipeline)
                    ((ImplicitRegistration)set).ResolveMethod = (ref ResolutionContext context) => lifetime.GetValue(context.LifetimeContainer);
                else
                    ((ImplicitRegistration)set).ResolveMethod = (ref ResolutionContext context) =>
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
