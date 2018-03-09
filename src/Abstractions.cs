using System;
using Unity.Builder;
using Unity.Lifetime;
using Unity.Policy;
using Unity.Registration;
using Unity.Resolution;
using Unity.Storage;

namespace Unity
{
    public ref struct ResolutionContext
    {
        public IUnityContainer Container;

        public ILifetimeContainer LifetimeContainer;

        public IPolicySet Registration;

        public ResolverOverride[] Overrides;

        public LinkedNode<Type, IPolicySet> Target;
    }

    public ref struct RegistrationData
    {
        public INamedType Registration;

        public Func<IUnityContainer, Type, string, object> Factory;

        public object Instance;

        public InjectionMember[] InjectionMembers;
    }

    public delegate object ResolveDelegate(ref ResolutionContext context);

    public delegate void RegisterDelegate(IUnityContainer container, ref RegistrationData data);

    public delegate RegisterDelegate RegistrationFactoryDelegate(RegisterDelegate next);

    public delegate ResolveDelegate AspectFactoryDelegate(InternalRegistration registration, ResolveDelegate resolveDelegate);
}
