using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Unity.Build.Context;
using Unity.Build.Pipeline;
using Unity.Builder;
using Unity.Events;
using Unity.Exceptions;
using Unity.Extension;
using Unity.Lifetime;
using Unity.Registration;
using Unity.Resolution;

namespace Unity
{
    public partial class UnityContainer : IUnityContainer
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
                if (!registeredInfo.IsGenericType &&  !mappedInfo.IsGenericType && !registeredInfo.IsAssignableFrom(mappedInfo))
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                        Constants.TypesAreNotAssignable, registeredType, mappedTo), nameof(registeredType));
                }
            }

            // Register type

            // ReSharper disable once CoVariantArrayConversion
            _staticRegistrationPipeline(_lifetimeContainer, new ExplicitRegistration(registeredType, name, mappedTo, lifetimeManager), injectionMembers);

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
            _instanceRegistrationPipeline(_lifetimeContainer, new ExplicitRegistration(type, name, type, lifetime));

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
                Type resolvingType = type ?? throw new ArgumentNullException(nameof(type)); 
                //VerifyPipeline<Type> Verify = (Type t) => {};
                var registration = GetRegistration(type, name);

                ResolveDependency Resolve = (Type t, string n) =>
                {
                    // Verify if can resolve this type
                    //Verify(t);

                    // Cache old values
                    //VerifyPipeline<Type> parentVerify = Verify;
                    Type parentResolvingType = resolvingType;

                    try
                    {
                        Func<Type, string, Type, object> getMethod = (Type tt, string na, Type i) => _getPolicy(tt, na, i);
                        ResolutionContext context = new ResolutionContext(getMethod, null)
                        {
                            LifetimeContainer = _lifetimeContainer,

                            Registration = GetRegistration(t, n),
                            ImplementationType = t,
                            DeclaringType = resolvingType,
                        };

                        resolvingType = t;
                        //Verify = (Type vT) => { if (ResolvingType == vT) throw new InvalidOperationException(); };

                        return ((IResolveMethod)context.Registration).ResolveMethod(ref context);
                    }
                    finally
                    {
                        //Verify = parentVerify;
                        resolvingType = parentResolvingType;
                    }

                };

                ResolutionContext rootContext = new ResolutionContext
                {
                    LifetimeContainer = _lifetimeContainer,

                    Registration = registration,
                    ImplementationType = type,
                    DeclaringType = null,

                    Resolve = Resolve
                };

                return registration.ResolveMethod?.Invoke(ref rootContext);
            }
            catch (Exception ex)
            {
                throw new ResolutionFailedException(type, name, "// TODO: Bummer!", ex);
            }
        }

        #endregion


        #region BuildUp existing object

        /// <inheritdoc />
        public object BuildUp(Type type, string name, object existing, params ResolverOverride[] resolverOverrides)
        {
            // Verify arguments
            var targetType = type ?? throw new ArgumentNullException(nameof(type));
            if (null != existing) InstanceIsAssignable(targetType, existing, nameof(existing));

            var context = new BuilderContext(this, GetRegistration(targetType, string.IsNullOrEmpty(name) ? null : name), existing, resolverOverrides);

            return BuilUpPipeline(context);
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
