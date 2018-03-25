using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Storage;

namespace Unity.Container.Storage
{
    [DebuggerDisplay("LinkedRegistry:  Count={_count}")]
    internal class LinkedRegistry : LinkedNode<string, IPolicySet>, 
                                    IRegistry<string, IPolicySet>
    {
        #region Fields

        private int _count;
        public const int ListToHashCutoverPoint = 8;

        #endregion


        #region Constructors

        public LinkedRegistry(string key, IPolicySet value) 
            : base(key, value)
        {
            _count = 1;
        }

    #endregion


    #region IRegistry

    public IPolicySet this[string key]
        {
            get
            {
                var hash = key?.GetHashCode() ?? 0;
                for (var node = (LinkedNode<string, IPolicySet>)this; node != null; node = node.Next)
                {
                    if (node.Hash == hash && node.Key == key)
                        return node.Value;
                }

                return default(IPolicySet);
            }
            set
            {
                var hash = key?.GetHashCode() ?? 0;
                LinkedNode<string, IPolicySet> node;
                LinkedNode<string, IPolicySet> last = null;

                for (node = this; node != null; node = node.Next)
                {
                    if (node.Hash == hash && node.Key == key)
                    {
                        // Found it
                        node.Value = value;
                        return;
                    }
                    last = node;
                }

                // Not found, so add a new one
                last.Next = new LinkedNode<string, IPolicySet>(key, value);

                _count++;
            }
        }

        public bool RequireToGrow => ListToHashCutoverPoint < _count;

        public IEnumerable<string> Keys
        {
            get
            {
                for (LinkedNode<string, IPolicySet> node = this; node != null; node = node.Next)
                {
                    yield return node.Key;
                }
            }
        }

        public IEnumerable<IPolicySet> Values
        {
            get
            {
                for (LinkedNode<string, IPolicySet> node = this; node != null; node = node.Next)
                {
                    yield return node.Value;
                }
            }
        }

        public IPolicySet GetOrAdd(string key, Func<IPolicySet> factory)
        {
            var hash = key?.GetHashCode() ?? 0;
            LinkedNode<string, IPolicySet> node;
            LinkedNode<string, IPolicySet> last = null;

            for (node = this; node != null; node = node.Next)
            {
                if (node.Hash == hash && node.Key == key)
                {
                    return node.Value ?? (node.Value = factory());
                }
                last = node;
            }

            // Not found, so add a new one
            last.Next = new LinkedNode<string, IPolicySet>(key, factory());
            _count++;

            return last.Next.Value;
        }

        public IPolicySet SetOrReplace(string key, IPolicySet value)
        {
            var hash = key?.GetHashCode() ?? 0;
            LinkedNode<string, IPolicySet> node;
            LinkedNode<string, IPolicySet> last = null;

            for (node = this; node != null; node = node.Next)
            {
                if (node.Hash == hash && node.Key == key)
                {
                    var old = node.Value;
                    node.Value = value;
                    return old;
                }
                last = node;
            }

            // Not found, so add a new one
            last.Next = new LinkedNode<string, IPolicySet>(key, value);
            _count++;

            return null;
        }


        #endregion
    }
}
