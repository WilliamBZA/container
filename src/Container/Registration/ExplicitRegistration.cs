using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Unity.Build.Pipeline;
using Unity.Build.Policy;
using Unity.Lifetime;
using Unity.Registration;

namespace Unity.Container.Registration
{
    [DebuggerDisplay("ExplicitRegistration: Type={Type?.Name},  Name={Name},  MappedTo={Type == ImplementationType ? string.Empty : ImplementationType?.Name ?? string.Empty},  {LifetimeManager?.GetType()?.Name}")]
    public class ExplicitRegistration : ImplicitRegistration, 
                                        IContainerRegistration,
                                        IPipelineFactory<Type>
    {
        #region Constructors

        public ExplicitRegistration(Type registeredType, string name, LifetimeManager lifetimeManager)
            : this(registeredType, name, registeredType, lifetimeManager)
        {}

        public ExplicitRegistration(Type registeredType, string name, Type mappedTo, LifetimeManager lifetimeManager)
            : base(registeredType, name)
        {
            LifetimeManager = lifetimeManager ?? TransientLifetimeManager.Instance;
            if (null != mappedTo) ImplementationType = mappedTo;
        }

        #endregion


        #region Public Members

        public PipelineFactory<Type, ResolveMethod> CreateActivator { get; set; }

        public PipelineFactory<Type, Expression> CreateExpression => throw new NotImplementedException();

        #endregion


        #region IContainerRegistration

        Type IContainerRegistration.RegisteredType => Type;

        Type IContainerRegistration.MappedToType => ImplementationType;

        public LifetimeManager LifetimeManager { get; }

        #endregion
    }
}
