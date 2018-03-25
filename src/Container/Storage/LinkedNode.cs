using System.Diagnostics;

namespace Unity.Container.Storage
{
    [DebuggerDisplay("LinkedNode:  Key={Key},    Value={Value}")]
    public class LinkedNode<TKey, TValue>
    {
        public readonly int Hash;
        public readonly TKey Key;
        public TValue Value;
        public LinkedNode<TKey, TValue> Next;


        public LinkedNode(TKey key)
        {
            Hash = key.GetHashCode();
            Key = key;
        }

        public LinkedNode(TKey key, TValue value)
        {
            Hash = key?.GetHashCode() ?? 0;
            Key = key;
            Value = value;
        }

        public LinkedNode(TKey key, TValue value, LinkedNode<TKey, TValue> next)
        {
            Hash = key?.GetHashCode() ?? 0;
            Key = key;
            Value = value;
            Next = next;
        }

        public LinkedNode(int hash, TKey key, TValue value, LinkedNode<TKey, TValue> next)
        {
            Hash = hash;
            Key = key;
            Value = value;
            Next = next;
        }
    }
}
