using System;
using System.Diagnostics;
using Unity.Injection;
using Unity.Lifetime;

namespace Unity.Registration
{
    [DebuggerDisplay("StaticRegistration: Type={RegisteredType?.Name},    Name={Name},    MappedTo={RegisteredType == MappedToType ? string.Empty : MappedToType?.Name ?? string.Empty},    {LifetimeManager?.GetType()?.Name}")]
    public class StaticRegistration : InternalRegistration, 
                                      IContainerRegistration
    {
        #region Constructors

        public StaticRegistration(Type registeredType, string name, Type mappedTo, LifetimeManager lifetimeManager)
            : base(registeredType ?? mappedTo, string.IsNullOrEmpty(name) ? null : name, typeof(ILifetimePolicy), lifetimeManager)
        {
            MappedToType = mappedTo;
            LifetimeManager = lifetimeManager;
            LifetimeManager.InUse = true;
        }

        #endregion


        #region IContainerRegistration

        public Type RegisteredType => Type;

        /// <summary>
        /// The type that this registration is mapped to. If no type mapping was done, the
        /// <see cref="InternalRegistration.Type"/> property and this one will have the same value.
        /// </summary>
        public Type MappedToType { get; }

        /// <summary>
        /// The lifetime manager for this registration.
        /// </summary>
        /// <remarks>
        /// This property will be null if this registration is for an open generic.</remarks>
        public LifetimeManager LifetimeManager { get; }

        #endregion


        #region IPolicySet

        public override void Set(Type policyInterface, object policy)
        {
            if (policy is InjectionFactory && (MappedToType != RegisteredType))
                throw new InvalidOperationException("Registration where both MappedToType and InjectionFactory are set is not supported");

            base.Set(policyInterface, policy);
        }

        #endregion
    }
}
