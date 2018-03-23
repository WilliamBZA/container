using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Unity.Build.Factory;
using Unity.Build.Pipeline;
using Unity.Lifetime;

namespace Unity.Registration
{
    [DebuggerDisplay("ExplicitRegistration: Type={RegisteredType?.Name},    Name={Name},    MappedTo={RegisteredType == MappedToType ? string.Empty : MappedToType?.Name ?? string.Empty},    {LifetimeManager?.GetType()?.Name}")]
    public class ExplicitRegistration : ImplicitRegistration, 
                                        IContainerRegistration
    {
        #region Fields

        public bool BuildRequired;

        #endregion


        #region Constructors

        public ExplicitRegistration(Type registeredType, string name, LifetimeManager lifetimeManager)
            : base(registeredType, string.IsNullOrEmpty(name) ? null : name)
        {
            MappedToType = registeredType;
            LifetimeManager = lifetimeManager ?? TransientLifetimeManager.Instance;
        }

        public ExplicitRegistration(Type registeredType, string name, Type mappedTo, LifetimeManager lifetimeManager)
            : base(registeredType ?? mappedTo, string.IsNullOrEmpty(name) ? null : name)
        {
            MappedToType = mappedTo;
            LifetimeManager = lifetimeManager ?? TransientLifetimeManager.Instance;
        }

        #endregion


        #region Public Members

        public virtual ResolveMethodFactory<Type> ResolveFactory { get; set; }

        #endregion


        #region IContainerRegistration

        public Type RegisteredType => Type;

        /// <summary>
        /// The type that this registration is mapped to. If no type mapping was done, the
        /// <see cref="ImplicitRegistration.Type"/> property and this one will have the same value.
        /// </summary>
        public Type MappedToType { get; }

        /// <summary>
        /// The lifetime manager for this registration.
        /// </summary>
        /// <remarks>
        /// This property will be null if this registration is for an open generic.</remarks>
        public LifetimeManager LifetimeManager { get; }

        #endregion
    }
}
