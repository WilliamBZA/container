using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Unity.Build.Pipeline;
using Unity.Build.Policy;
using Unity.Container.Registration;
using Unity.Events;
using Unity.Exceptions;
using Unity.Extension;
using Unity.Lifetime;
using Unity.Registration;
using Unity.Resolution;

namespace Unity
{
    public partial class UnityContainer
    {
        #region Type Registration

        /// <inheritdoc />
        public IUnityContainer RegisterType(Type registeredType, string name, Type mappedTo, LifetimeManager lifetimeManager, InjectionMember[] injectionMembers)
        {
            // Validate input
            if (null == registeredType) throw new ArgumentNullException(nameof(registeredType));
            if (null != mappedTo)
            {
                var mappedInfo = mappedTo.GetTypeInfo();
                var registeredInfo = registeredType.GetTypeInfo();
                if (!registeredInfo.IsGenericType && !mappedInfo.IsGenericType && !registeredInfo.IsAssignableFrom(mappedInfo))
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                        Constants.TypesAreNotAssignable, registeredType, mappedTo), nameof(registeredType));
                }
            }

            // Register type
            var registration = new ExplicitRegistration(registeredType, name, mappedTo, lifetimeManager);

            // Add injection members policies to the registration
            if (null != injectionMembers && 0 < injectionMembers.Length)
            {
                foreach (var member in injectionMembers)
                {
                    // Validate against ImplementationType with InjectionFactory
                    if (member is InjectionFactory && registration.ImplementationType != registration.Type)  // TODO: Add proper error message
                        throw new InvalidOperationException("Registration where both ImplementationType and InjectionFactory are set is not supported");

                    // Mark as requiring build if any one of the injectors are marked with IRequireBuild
                    if (member is IRequireBuild) registration.BuildRequired = true;

                    // Add policies
                    member.AddPolicies(registration.Type, registration.Name, registration.ImplementationType, registration);
                }
            }

            // Build resolve pipeline
            registration.ResolveMethod = _explicitRegistrationPipeline(_lifetimeContainer, registration);

            // Add to appropriate storage
            StoreRegistration(registration);

            return this;
        }

        #endregion


        #region Instance Registration

        /// <inheritdoc />
        public IUnityContainer RegisterInstance(Type registeredType, string name, object instance, LifetimeManager lifetimeManager)
        {
            // Validate input
            if (null == instance) throw new ArgumentNullException(nameof(instance));

            var type = registeredType ?? instance.GetType();
            var lifetime = lifetimeManager ?? new ContainerControlledLifetimeManager();
            lifetime.SetValue(instance);

            // Register instance
            var registration = new ExplicitRegistration(type, name, type, lifetime);
            registration.ResolveMethod = _instanceRegistrationPipeline(_lifetimeContainer, registration);

            return this;
        }

        #endregion


        #region Check Registration

        /// <inheritdoc />
        public bool IsRegistered(Type type, string name) => IsTypeRegistered(type, name);

        private bool IsTypeRegisteredLocally(Type type, string name)
        {
            var hashCode = (type?.GetHashCode() ?? 0) & 0x7FFFFFFF;
            var targetBucket = hashCode % _registrations.Buckets.Length;
            for (var i = _registrations.Buckets[targetBucket]; i >= 0; i = _registrations.Entries[i].Next)
            {
                if (_registrations.Entries[i].HashCode != hashCode ||
                    _registrations.Entries[i].Key != type)
                {
                    continue;
                }

                return null != _registrations.Entries[i].Value?[name] ||
                       (_parent?.IsTypeRegistered(type, name) ?? false);
            }

            return _parent?.IsTypeRegistered(type, name) ?? false;
        }


        #endregion


        #region Getting objects

        /// <inheritdoc />
        public object Resolve(Type type, string nameToBuild, params ResolverOverride[] resolverOverrides)
        {
            // Verify arguments
            var name = string.IsNullOrEmpty(nameToBuild) ? null : nameToBuild;

            try
            {
                var getParentContextMethod = GetContextFactoryMethod();
                ref var root = ref getParentContextMethod();

                object ResolveDelegate(Type dependencyType, string dependencyName)
                {
                    var getNewContextMethod = GetContextFactoryMethod();

                    // New and parent contexts
                    ref var context = ref getNewContextMethod();
                    ref var parent = ref getParentContextMethod();

                    // Initialize local context
                    context.Parent = getParentContextMethod;
                    context.LifetimeContainer = parent.LifetimeContainer;
                    context.Registration = GetRegistration(dependencyType, dependencyName);
                    context.Resolve = parent.Resolve;

                    // Setup recursion
                    getParentContextMethod = getNewContextMethod;

                    // Resolve
                    return ((IResolveMethod)context.Registration).ResolveMethod(ref context);
                }

                root.Registration = GetRegistration(type, name);
                root.LifetimeContainer = _lifetimeContainer;
                root.Resolve = ResolveDelegate;

                return ((ImplicitRegistration)root.Registration).ResolveMethod(ref root);
            }
            catch (Exception ex)
            {
                throw new ResolutionFailedException(type, name, "// TODO: Bummer!", ex);
            }
        }

        #endregion


        #region Extension Management

        /// <inheritdoc />
        public IUnityContainer AddExtension(UnityContainerExtension extension)
        {
            lock (_lifetimeContainer)
            {
                if (null == _extensions)
                    _extensions = new List<UnityContainerExtension>();

                _extensions.Add(extension ?? throw new ArgumentNullException(nameof(extension)));
            }
            extension.InitializeExtension(_extensionContext);

            return this;
        }

        /// <inheritdoc />
        public object Configure(Type configurationInterface)
        {
            return _extensions?.FirstOrDefault(ex => configurationInterface.GetTypeInfo()
                                                                          .IsAssignableFrom(ex.GetType()
                                                                          .GetTypeInfo()));
        }

        #endregion


        #region Child container management

        /// <inheritdoc />
        public IUnityContainer CreateChildContainer()
        {
            var child = new UnityContainer(this);
            ChildContainerCreated?.Invoke(this, new ChildContainerCreatedEventArgs(child._extensionContext));
            return child;
        }

        /// <inheritdoc />
        public IUnityContainer Parent => _parent;

        #endregion


        #region IDisposable

        /// <summary>
        /// Dispose this container instance.
        /// </summary>
        /// <remarks>
        /// Disposing the container also disposes any child containers,and 
        /// disposes any instances whose lifetimes are managed by the 
        /// container.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
