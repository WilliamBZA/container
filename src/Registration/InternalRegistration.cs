using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Builder;
using Unity.Builder.Strategy;
using Unity.Injection;
using Unity.Policy;
using Unity.Storage;

namespace Unity.Registration
{
    [DebuggerDisplay("InternalRegistration:  Type={Type?.Name},    Name={Name}")]
    public class InternalRegistration : IPolicySet, IEnumerable<object>,
                                        INamedType
    {
        #region Fields

        private readonly int _hash;
        private LinkedNode<Type, object> _next;

        #endregion


        #region Constructors

        public InternalRegistration(Type type, string name)
        {
            Name = name;
            Type = type;

            _hash = (Type?.GetHashCode() ?? 0 + 37) ^ (Name?.GetHashCode() ?? 0 + 17);
        }

        public InternalRegistration(Type type, string name, Type policyInterface, object policy)
        {
            Name = name;
            Type = type;

            _next = new LinkedNode<Type, object>
            {
                Key = policyInterface,
                Value = policy
            };

            _hash = (Type?.GetHashCode() ?? 0 + 37) ^ (Name?.GetHashCode() ?? 0 + 17);
        }

        #endregion


        #region Public Members

        public ResolveDelegate Resolve { get; set; }

        public Func<IUnityContainer, Type, string, object> Factory { get; private set; }

        public virtual IList<BuilderStrategy> BuildChain { get; set; }

        public bool EnableOptimization { get; set; } = true;

        #endregion


        #region IPolicySet

        public virtual object Get(Type policyInterface)
        {
            if (typeof(IInjectionFactory).Equals(policyInterface))
                return Factory;
            else
            {
                for (var node = _next; node != null; node = node.Next)
                {
                    if (ReferenceEquals(node.Key, policyInterface))
                        return node.Value;
                }
            }

            return null;
        }

        public virtual void Set(Type policyInterface, object policy)
        {
            if (typeof(IInjectionFactory).Equals(policyInterface))
            {
                Factory = (Func<IUnityContainer, Type, string, object>)policy;
            }
            else
            {
                _next = new LinkedNode<Type, object>
                {
                    Key = policyInterface,
                    Value = policy,
                    Next = _next
                };
            }
        }

        public virtual void Clear(Type policyInterface)
        {
            LinkedNode<Type, object> node;
            LinkedNode<Type, object> last = _next;

            for (node = _next; node != null; node = node.Next)
            {
                if (ReferenceEquals(node.Key, policyInterface))
                {
                    last.Key = node.Next?.Key;
                    last.Value = node.Next?.Value;
                    last.Next = node.Next?.Next;
                }

                last = node;
            }
        }

        #endregion


        #region IEnumerable


        public IEnumerator<object> GetEnumerator()
        {
            for (var node = _next; node != null; node = node.Next)
            {
                yield return node.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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

        public static implicit operator NamedTypeBuildKey(InternalRegistration namedType)
        {
            return new NamedTypeBuildKey(namedType.Type, namedType.Name);
        }

        #endregion
    }
}
