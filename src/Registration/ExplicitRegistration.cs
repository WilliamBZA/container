using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Unity.Build.Factory;
using Unity.Build.Pipeline;
using Unity.Build.Policy;
using Unity.Lifetime;

namespace Unity.Registration
{
    [DebuggerDisplay("ExplicitRegistration: Type={RegisteredType?.Name},    Name={Name},    MappedTo={RegisteredType == MappedToType ? string.Empty : MappedToType?.Name ?? string.Empty},    {LifetimeManager?.GetType()?.Name}")]
    public class ExplicitRegistration : ImplicitRegistration, 
                                        IContainerRegistration,
                                        IResolve<Type>
    {
        #region Constructors

        public ExplicitRegistration(Type registeredType, string name, LifetimeManager lifetimeManager)
            : this(registeredType, name, registeredType, lifetimeManager)
        {}

        public ExplicitRegistration(Type registeredType, string name, Type mappedTo, LifetimeManager lifetimeManager)
            : base(registeredType, name)
        {
            LifetimeManager = lifetimeManager ?? TransientLifetimeManager.Instance;
            if (null != mappedTo) MappedToType = mappedTo;
        }

        #endregion


        #region Public Members

        public PipelineFactory<Type, ResolveMethod> Resolver { get; set; }

        public PipelineFactory<Type, Expression> Expression => throw new NotImplementedException();

        #endregion


        #region IContainerRegistration

        Type IContainerRegistration.RegisteredType => Type;

        Type IContainerRegistration.MappedToType => MappedToType;

        public LifetimeManager LifetimeManager { get; }

        #endregion
    }
}
