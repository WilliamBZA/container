using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Unity.Builder;
using Unity.Storage;

namespace Unity.Container.Storage
{
    [DebuggerDisplay("PolicySet:  Type={Type?.Name},    Name={Name}")]
    public class PolicySet : IPolicySet, INamedType
    {
        #region Fields

        private readonly int _hash;
        private LinkedNode<Type, object> _next;
        private LinkedNode<Type, object> _foregn;

        #endregion


        #region Constructors

        public PolicySet(Type type, string name)
        {
            Name = name;
            Type = type;

            _hash = (Type?.GetHashCode() ?? 0 + 37) ^ (Name?.GetHashCode() ?? 0 + 17);
        }

        public PolicySet(Type type, string name, Type policyInterface, object policy)
            : this(type, name)
        {
            _next = new LinkedNode<Type, object>(policyInterface, policy);
        }

        #endregion


        #region IPolicySet


        public void Add(Type policyInterface, object value)
        {
            _next = new LinkedNode<Type, object>(policyInterface, value, _next);
        }
        
        public virtual object Get(Type policyInterface)
        {
            for (var node = _next; node != null; node = node.Next)
            {
                if (ReferenceEquals(node.Key, policyInterface))
                    return node.Value;
            }

            return null;
        }

        public object Get(Type type, string name, Type policyInterface)
        {
            if (Type == type && Name == name)
                return Get(policyInterface);

            // TODO: Rethink proper identification
            var hash = (type?.GetHashCode() ?? 0) * 37 + name?.GetHashCode() ?? 0;
            for (var node = _foregn; node != null; node = node.Next)
            {
                if (node.Hash == hash && ReferenceEquals(node.Key, policyInterface))
                    return node.Value;
            }

            return null;
        }

        public virtual void Set(Type policyInterface, object value)
        {
            var hash = policyInterface?.GetHashCode() ?? 0;
            for (var node = _next; node != null; node = node.Next)
            {
                if (node.Hash == hash && node.Key == policyInterface)
                {
                    // Found it
                    node.Value = value;
                    return;
                }
            }

            // Not found, so add a new one
            _next = new LinkedNode<Type, object>(policyInterface, value, _next);
        }

        public void Set(Type type, string name, Type policyInterface, object policy)
        {
            if (Type == type && Name == name)
                Set(policyInterface, policy);
            else
            {
                var hash = (type?.GetHashCode() ?? 0) * 37 + name?.GetHashCode() ?? 0;
                _foregn = new LinkedNode<Type, object>(hash, policyInterface, policy, _next);
            }
        }

        public virtual void Clear(Type policyInterface)
        {
            var last = _next;
            for (var node = _next; node != null; node = node.Next)
            {
                if (ReferenceEquals(node.Key, policyInterface))
                    last.Next = node.Next;

                last = node;
            }
        }

        public void Clear(Type type, string name, Type policyInterface)
        {
            if (Type == type && Name == name)
                Clear(policyInterface);
            else
            {
                var last = _foregn;
                var hash = (type?.GetHashCode() ?? 0) * 37 + name?.GetHashCode() ?? 0;
                for (var node = _foregn; node != null; node = node.Next)
                {
                    if (node.Hash == hash && ReferenceEquals(node.Key, policyInterface))
                        last.Next = node.Next;

                    last = node;
                }
            }
        }

        public IEnumerable<object> OfType<T>(bool exactMatch = false)
        {
            if (exactMatch)
            {
                for (var node = _next; node != null; node = node.Next)
                {
                    if (typeof(T) == node.Key) continue;
                    yield return node.Value;
                }
            }
            else
            {
                var info = typeof(T).GetTypeInfo();
                for (var node = _next; node != null; node = node.Next)
                {
                    if (!info.IsAssignableFrom(node.Key?.GetTypeInfo())) continue;

                    yield return node.Value;
                }
            }
        }

        public IEnumerable<object> PopType<T>(bool exactMatch = false)
        {
            var last = _next;
            if (exactMatch)
            {
                for (var node = _next; node != null; node = node.Next)
                {
                    if (typeof(T) == node.Key)
                    {
                        var value = node.Value;
                        last.Next = node.Next;
                        node = last;

                        yield return value;
                    }

                    last = node;
                }
            }
            else
            {
                var info = typeof(T).GetTypeInfo();
                for (var node = _next; node != null; node = node.Next)
                {
                    if (info.IsAssignableFrom(node.Key?.GetTypeInfo()))
                    {
                        var value = node.Value;
                        last.Next = node.Next;
                        node = last;

                        yield return value;
                    }

                    last = node;
                }
            }
        }


        #endregion


        #region INamedType

        public Type Type { get; }

        public string Name { get; }

        public override bool Equals(object obj)
        {
            return obj is INamedType registration &&
                   ReferenceEquals(Type, registration.Type) &&
                   Name == registration.Name;
        }

        public override int GetHashCode()
        {
            return _hash;
        }

        public static implicit operator NamedTypeBuildKey(PolicySet namedType)
        {
            return new NamedTypeBuildKey(namedType.Type, namedType.Name);
        }

        #endregion
    }
}
