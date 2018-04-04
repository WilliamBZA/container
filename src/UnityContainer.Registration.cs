using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Unity.Build.Context;
using Unity.Build.Pipeline;
using Unity.Container.Registration;
using Unity.Container.Storage;
using Unity.Lifetime;
using Unity.Policy;
using Unity.Registration;
using Unity.Storage;

namespace Unity
{
    public partial class UnityContainer
    {
        #region Constants

        private const int ContainerInitialCapacity = 37;
        private const int ListToHashCutoverPoint = 8;

        #endregion


        #region Registration Aspects

        public static RegisterPipeline DynamicRegistrationAspectFactory(RegisterPipeline next)
        {
            // Analise registration and generate mappings
            return (ILifetimeContainer lifetimeContainer, IPolicySet set, object[] args) =>
            {
                var registration = (ImplicitRegistration)set;
                var info = registration.Type.GetTypeInfo();

                // Generic types require implementation
                if (info.IsGenericType)
                {
                    // Find implementation for this type
                    var unity = (UnityContainer)lifetimeContainer.Container;
                    var definition = info.GetGenericTypeDefinition();
                    var registry = unity._getType(definition) ??
                                   throw new InvalidOperationException("No such type"); // TODO: Add proper error message

                    // This registration must be present to proceed
                    var target = (null == registration.Name
                               ? registry[null]
                               : registry[registration.Name] ?? registry[null])
                               ?? throw new InvalidOperationException("No such type");    // TODO: Add proper error message

                    // Build rest of pipeline
                    return next?.Invoke(lifetimeContainer, registration, target);
                }

                // Build rest of pipeline
                return next?.Invoke(lifetimeContainer, registration);
            };
        }

        #endregion


        #region Registrations Collection

        /// <summary>
        /// GetOrDefault a sequence of <see cref="IContainerRegistration"/> that describe the current state
        /// of the container.
        /// </summary>
        public IEnumerable<IContainerRegistration> Registrations
        {
            get
            {
                var types = GetRegisteredTypes(this);
                foreach (var type in types)
                {
                    var registrations = GetRegisteredType(this, type);
                    foreach (var registration in registrations)
                        yield return registration;
                }
            }
        }

        private ISet<Type> GetRegisteredTypes(UnityContainer container)
        {
            var set = null == container._parent ? new HashSet<Type>()
                                                : GetRegisteredTypes(container._parent);

            if (null == container._registrations) return set;

            var types = container._registrations.Keys;
            foreach (var type in types)
            {
                if (null == type) continue;
                set.Add(type);
            }

            return set;
        }

        private IEnumerable<IContainerRegistration> GetRegisteredType(UnityContainer container, Type type)
        {
            MiniHashSet<IContainerRegistration> set;

            if (null != container._parent)
                set = (MiniHashSet<IContainerRegistration>)GetRegisteredType(container._parent, type);
            else
                set = new MiniHashSet<IContainerRegistration>();

            if (null == container._registrations) return set;

            var section = container.Get(type)?.Values;
            if (null == section) return set;

            foreach (var namedType in section)
            {
                if (namedType is IContainerRegistration registration)
                    set.Add(registration);
            }

            return set;
        }

        #endregion


        #region Entire Type of named registrations

        private IRegistry<string, IPolicySet> Get(Type type)
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

                return _registrations.Entries[i].Value;
            }

            return null;
        }

        #endregion


        #region Registration manipulation

        private void StoreRegistration(ExplicitRegistration registration)
        {
            // Add to appropriate storage
            var unity = registration.LifetimeManager is ISingletonLifetimePolicy
                      ? _root : this;

            // Add or replace if exists 
            var previous = unity._register(registration);
            if (previous is ExplicitRegistration old && old.LifetimeManager is IDisposable disposableOld)
            {
                // Dispose replaced lifetime manager
                unity._lifetimeContainer.Remove(disposableOld);
                disposableOld.Dispose();
            }

            // If Disposable add to container's lifetime
            if (registration.LifetimeManager is IDisposable)
            {
                unity._lifetimeContainer.Add(registration.LifetimeManager);
            }
        }

        private ImplicitRegistration AddOrUpdate(ExplicitRegistration registration)
        {
            var collisions = 0;
            var hashCode = (registration.Type?.GetHashCode() ?? 0) & 0x7FFFFFFF;
            var targetBucket = hashCode % _registrations.Buckets.Length;
            lock (_syncRoot)
            {
                for (var i = _registrations.Buckets[targetBucket]; i >= 0; i = _registrations.Entries[i].Next)
                {
                    if (_registrations.Entries[i].HashCode != hashCode ||
                        _registrations.Entries[i].Key != registration.Type)
                    {
                        collisions++;
                        continue;
                    }

                    var existing = _registrations.Entries[i].Value;
                    if (existing.RequireToGrow)
                    {
                        existing = existing is HashRegistry<string, IPolicySet> registry
                                 ? new HashRegistry<string, IPolicySet>(registry)
                                 : new HashRegistry<string, IPolicySet>(LinkedRegistry.ListToHashCutoverPoint * 2,
                                                                       (LinkedRegistry)existing)
                                 { [string.Empty] = null };

                        _registrations.Entries[i].Value = existing;
                    }

                    return (ImplicitRegistration)existing.SetOrReplace(registration.Name, registration);
                }

                if (_registrations.RequireToGrow || ListToHashCutoverPoint < collisions)
                {
                    _registrations = new HashRegistry<Type, IRegistry<string, IPolicySet>>(_registrations);
                    targetBucket = hashCode % _registrations.Buckets.Length;
                }

                _registrations.Entries[_registrations.Count].HashCode = hashCode;
                _registrations.Entries[_registrations.Count].Next = _registrations.Buckets[targetBucket];
                _registrations.Entries[_registrations.Count].Key = registration.Type;
                _registrations.Entries[_registrations.Count].Value = new LinkedRegistry(registration.Name, registration);
                _registrations.Buckets[targetBucket] = _registrations.Count;
                _registrations.Count++;

                return null;
            }
        }

        private ImplicitRegistration GetOrAdd(Type type, string name)
        {
            var collisions = 0;
            var hashCode = (type?.GetHashCode() ?? 0) & 0x7FFFFFFF;
            var targetBucket = hashCode % _registrations.Buckets.Length;

            for (var i = _registrations.Buckets[targetBucket]; i >= 0; i = _registrations.Entries[i].Next)
            {
                if (_registrations.Entries[i].HashCode != hashCode ||
                    _registrations.Entries[i].Key != type)
                {
                    continue;
                }

                var policy = _registrations.Entries[i].Value?[name];
                if (null != policy) return (ImplicitRegistration)policy;
            }

            ImplicitRegistration registration = null;

            try
            {
                lock (_syncRoot)
                {
                    for (var i = _registrations.Buckets[targetBucket]; i >= 0; i = _registrations.Entries[i].Next)
                    {
                        if (_registrations.Entries[i].HashCode != hashCode ||
                            _registrations.Entries[i].Key != type)
                        {
                            collisions++;
                            continue;
                        }

                        var existing = _registrations.Entries[i].Value;
                        if (existing.RequireToGrow)
                        {
                            existing = existing is HashRegistry<string, IPolicySet> registry
                                     ? new HashRegistry<string, IPolicySet>(registry)
                                     : new HashRegistry<string, IPolicySet>(LinkedRegistry.ListToHashCutoverPoint * 2,
                                                                           (LinkedRegistry)existing)
                                     { [string.Empty] = null };

                            _registrations.Entries[i].Value = existing;
                        }

                        registration = (ImplicitRegistration)existing.GetOrAdd(name, () => CreateRegistration(type, name));
                        break;
                    }

                    if (null == registration)
                    {
                        if (_registrations.RequireToGrow || ListToHashCutoverPoint < collisions)
                        {
                            _registrations = new HashRegistry<Type, IRegistry<string, IPolicySet>>(_registrations);
                            targetBucket = hashCode % _registrations.Buckets.Length;
                        }

                        registration = CreateRegistration(type, name);
                        _registrations.Entries[_registrations.Count].HashCode = hashCode;
                        _registrations.Entries[_registrations.Count].Next = _registrations.Buckets[targetBucket];
                        _registrations.Entries[_registrations.Count].Key = type;
                        _registrations.Entries[_registrations.Count].Value = new LinkedRegistry(name, registration);
                        _registrations.Buckets[targetBucket] = _registrations.Count;
                        _registrations.Count++;
                    }

                }

                // Build resolve pipeline
                registration.ResolveMethod = _dynamicRegistrationPipeline(_lifetimeContainer, registration);
            }
            finally 
            {
                // Release lock on the registration
                Monitor.Exit(registration);
            }

            return registration;
        }

        private ImplicitRegistration CreateRegistration(Type type, string name)
        {
            // Create registration
            var registration = new ImplicitRegistration(type, name);

            // Create stub method in case it is requested before pipeline
            // has been created. It waits for the initialization to complete
            registration.ResolveMethod = (ref ResolutionContext context) =>
            {
                // Once lock is acquired the pipeline has been built
                lock (Registrations)
                {
                    return registration.ResolveMethod(ref context);
                }
            };

            // Acquire lock on the registration 
            // until it is initialized
            Monitor.Enter(registration);

            return registration;
        }

        private IPolicySet Get(Type type, string name)
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

                return _registrations.Entries[i].Value?[name];
            }

            return null;
        }

        #endregion


        #region Local policy manipulation

        private IBuilderPolicy Get(Type type, string name, Type policyInterface, out IPolicyList list)
        {
            list = null;
            IBuilderPolicy policy = null;
            var hashCode = (type?.GetHashCode() ?? 0) & 0x7FFFFFFF;
            var targetBucket = hashCode % _registrations.Buckets.Length;
            for (var i = _registrations.Buckets[targetBucket]; i >= 0; i = _registrations.Entries[i].Next)
            {
                if (_registrations.Entries[i].HashCode != hashCode ||
                    _registrations.Entries[i].Key != type)
                {
                    continue;
                }

                policy = (IBuilderPolicy)_registrations.Entries[i].Value?[name]?.Get(policyInterface);
                break;
            }

            if (null != policy)
            {
                list = _extensionContext.Policies;
                return policy;
            }

            return _parent?.GetPolicyList(type, name, policyInterface, out list);
        }

        private object Get(Type type, string name, Type requestedType)
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

                return _registrations.Entries[i].Value?[name]?.Get(requestedType) ??
                       _parent?._getPolicy(type, name, requestedType);
            }

            return _parent?._getPolicy(type, name, requestedType);
        }

        private void Set(Type type, string name, Type policyInterface, object policy)
        {
            var collisions = 0;
            var hashCode = (type?.GetHashCode() ?? 0) & 0x7FFFFFFF;
            var targetBucket = hashCode % _registrations.Buckets.Length;
            lock (_syncRoot)
            {
                for (var i = _registrations.Buckets[targetBucket]; i >= 0; i = _registrations.Entries[i].Next)
                {
                    if (_registrations.Entries[i].HashCode != hashCode ||
                        _registrations.Entries[i].Key != type)
                    {
                        collisions++;
                        continue;
                    }

                    var existing = _registrations.Entries[i].Value;
                    var policySet = existing[name];
                    if (null != policySet)
                    {
                        policySet.Set(policyInterface, policy);
                        return;
                    }

                    if (existing.RequireToGrow)
                    {
                        existing = existing is HashRegistry<string, IPolicySet> registry
                                 ? new HashRegistry<string, IPolicySet>(registry)
                                 : new HashRegistry<string, IPolicySet>(LinkedRegistry.ListToHashCutoverPoint * 2,
                                                                       (LinkedRegistry)existing)
                                 { [string.Empty] = null };

                        _registrations.Entries[i].Value = existing;
                    }

                    existing.GetOrAdd(name, () => CreateRegistration(type, name, policyInterface, policy));
                    return;
                }

                if (_registrations.RequireToGrow || ListToHashCutoverPoint < collisions)
                {
                    _registrations = new HashRegistry<Type, IRegistry<string, IPolicySet>>(_registrations);
                    targetBucket = hashCode % _registrations.Buckets.Length;
                }

                var registration = CreateRegistration(type, name, policyInterface, policy);
                _registrations.Entries[_registrations.Count].HashCode = hashCode;
                _registrations.Entries[_registrations.Count].Next = _registrations.Buckets[targetBucket];
                _registrations.Entries[_registrations.Count].Key = type;
                _registrations.Entries[_registrations.Count].Value = new LinkedRegistry(name, registration);
                _registrations.Buckets[targetBucket] = _registrations.Count;
                _registrations.Count++;
            }
        }

        private IPolicySet CreateRegistration(Type type, string name, Type policyInterface, object policy)
        {
            // Create registration and set provided policy
            var registration = new ImplicitRegistration(type, name, policyInterface, policy);

            // Create stub method in case registration is ever resolved
            registration.ResolveMethod = (ref ResolutionContext context) =>
            {
                // Once lock is acquired the pipeline has been built
                lock (Registrations)
                {
                    // Build resolve pipeline and replace the stub
                    registration.ResolveMethod = _dynamicRegistrationPipeline(_lifetimeContainer, registration);

                    // Resolve using built pipeline
                    return registration.ResolveMethod(ref context);
                }
            };

            return registration;
        }

        private void Clear(Type type, string name, Type policyInterface)
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

                _registrations.Entries[i].Value?[name]?.Clear(policyInterface);
                return;
            }
        }

        #endregion

    }
}
