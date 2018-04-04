using System;
using Unity.Events;
using Unity.Extension;
using Unity.Lifetime;
using Unity.Policy;
using Unity.Storage;

namespace Unity
{
    public partial class UnityContainer
    {
        // TODO: Temporary, replace with permanent extension solution
        public class UnityContainerConfigurator : IUnityContainerExtensionConfigurator
        {
            private readonly UnityContainer _container;

            internal UnityContainerConfigurator(UnityContainer container)
            {
                _container = container;
            }

            /// <summary>
            /// Get IUnityContainer
            /// </summary>
            public IUnityContainer Container => _container;

            /// <summary>
            /// Get registration
            /// </summary>
            /// <param name="type">Registered Type</param>
            /// <param name="name">Registered Name</param>
            /// <returns>Returns registration</returns>
            public IPolicySet Get(Type type, string name) 
                => _container.Get(type, name);

            /// <summary>
            /// Get registered policy
            /// </summary>
            /// <param name="type">Registered Type</param>
            /// <param name="name">Registered Name</param>
            /// <param name="requestedType">Type of the policy to return</param>
            /// <returns>Requested policy or null if nothing found</returns>
            public object Get(Type type, string name, Type requestedType) 
                => _container.Get(type, name, requestedType);

            /// <summary>
            /// Set policy
            /// </summary>
            /// <param name="type">Registered Type</param>
            /// <param name="name">Registered Name</param>
            /// <param name="policyInterface">Type of the policy</param>
            /// <param name="policy">The Policy</param>
            public void Set(Type type, string name, Type policyInterface, object policy) 
                => _container.Set(type, name, policyInterface, policy);

            /// <summary>
            /// Clear specific policy
            /// </summary>
            /// <param name="type">Registered Type</param>
            /// <param name="name">Registered Name</param>
            /// <param name="policyInterface">Type of the policy</param>
            public void Clear(Type type, string name, Type policyInterface)
                => _container.Clear(type, name, policyInterface);
        }



        /// <summary>
        /// Abstraction layer between container and extensions
        /// </summary>
        /// <remarks>
        /// Implemented as a nested class to gain access to  
        /// container that would otherwise be inaccessible.
        /// </remarks>
        private class ContainerExtensionContext : ExtensionContext, IPolicyList 
        {
            #region Fields

            private readonly object syncRoot = new object();
            private readonly UnityContainer _container;

            #endregion


            #region Constructors

            public ContainerExtensionContext(UnityContainer container)
            {
                _container = container ?? throw new ArgumentNullException(nameof(container));
                Policies = this;
            }

            #endregion


            #region ExtensionContext

            public override IUnityContainer Container => _container;

            public override IPolicyList Policies { get; }

            public override ILifetimeContainer Lifetime => _container._lifetimeContainer;

            public override event EventHandler<RegisterEventArgs> Registering
            {
                add => _container.Registering += value;
                remove => _container.Registering -= value;
            }

            public override event EventHandler<RegisterInstanceEventArgs> RegisteringInstance
            {
                add => _container.RegisteringInstance += value;
                remove => _container.RegisteringInstance -= value;
            }

            public override event EventHandler<ChildContainerCreatedEventArgs> ChildContainerCreated
            {
                add => _container.ChildContainerCreated += value;
                remove => _container.ChildContainerCreated -= value;
            }

            #endregion


            #region IPolicyList

            public virtual void ClearAll()
            {
            }

            public virtual IBuilderPolicy Get(Type type, string name, Type policyInterface, out IPolicyList list) 
                => _container.GetPolicyList(type, name, policyInterface, out list);

            public virtual void Set(Type type, string name, Type policyInterface, IBuilderPolicy policy)
                => _container.SetPolicy(type, name, policyInterface, policy);

            public virtual void Clear(Type type, string name, Type policyInterface)
            {
            }

            #endregion
        }
    }
}
