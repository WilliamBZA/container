using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Build.Pipeline;
using Unity.Builder;
using Unity.Builder.Strategy;
using Unity.Storage;

namespace Unity.Registration
{
    [DebuggerDisplay("ImplicitRegistration:  Type={Type?.Name},    Name={Name}")]
    public class ImplicitRegistration : IPolicySet,
                                        INamedType,
                                        IResolveMethod
    {
        #region Fields

        private readonly int _hash;
        private LinkedNode<Type, object> _next;
        private LinkedNode<Type, object> _foregn;

        #endregion


        #region Constructors

        public ImplicitRegistration(Type type, string name)
        {
            Name = name;
            Type = type;

            _hash = (Type?.GetHashCode() ?? 0 + 37) ^ (Name?.GetHashCode() ?? 0 + 17);
        }

        public ImplicitRegistration(Type type, string name, Type policyInterface, object policy)
        {
            Name = name;
            Type = type;

            _hash = (Type?.GetHashCode() ?? 0 + 37) ^ (Name?.GetHashCode() ?? 0 + 17);
            _next = new LinkedNode<Type, object>(policyInterface, policy);
        }

        #endregion


        #region Public Members

        public ResolveMethod ResolveMethod { get; set; }

        public virtual IList<BuilderStrategy> BuildChain { get; set; }

        public bool EnableOptimization { get; set; } = true;

        #endregion


        #region IPolicySet

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

        public virtual void Set(Type policyInterface, object policy)
        {
            _next = new LinkedNode<Type, object>(policyInterface, policy, _next);
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
            LinkedNode<Type, object> node;
            LinkedNode<Type, object> last = _next;

            for (node = _next; node != null; node = node.Next)
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
                LinkedNode<Type, object> node;
                LinkedNode<Type, object> last = _foregn;

                var hash = (type?.GetHashCode() ?? 0) * 37 + name?.GetHashCode() ?? 0;
                for (node = _foregn; node != null; node = node.Next)
                {
                    if (node.Hash == hash && ReferenceEquals(node.Key, policyInterface))
                        last.Next = node.Next;

                    last = node;
                }
            }
        }

        #endregion


        #region Enumerable

        public IEnumerable<object> OfType<T>()
        {
            for (var node = _next; node != null; node = node.Next)
            {
                if (typeof(T) != node.Key) continue;

                yield return node.Value;
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

        public static implicit operator NamedTypeBuildKey(ImplicitRegistration namedType)
        {
            return new NamedTypeBuildKey(namedType.Type, namedType.Name);
        }

        #endregion

    }
}
