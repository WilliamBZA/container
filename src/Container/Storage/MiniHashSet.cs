using System;
using System.Collections;
using System.Collections.Generic;

namespace Unity.Container.Storage
{
    internal class MiniHashSet<T> : IList<T>
    {
        private struct Slot
        {
            internal int HashCode;      // Lower 31 bits of hash code, 0 if unused
            internal T Value;
            internal int Next;          // Index of next entry, 0 if last
        }

        #region Fields

        private int[] _buckets;
        private Slot[] _slots;
        private int _count;

        #endregion


        #region Constructors

        public MiniHashSet()
        {
            _count = 0;
            _buckets = new int[37];
            _slots   = new Slot[37];
        }

        #endregion


        #region IList<T>

        public int Count => _count;

        public bool IsReadOnly => true;

        public T this[int index] { get => _slots[index].Value; set => throw new NotImplementedException(); }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        #endregion


        #region ReverseHashSet methods

        /// <summary>
        /// Add item to this HashSet. Later value replaces previosly set value
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            var hashCode = item?.GetHashCode() & 0x7FFFFFFF ?? 0 ;
            var bucket = hashCode % _buckets.Length;
            var collisionCount = 0;

            for (int i = _buckets[bucket]; --i >= 0; i = _slots[i].Next)
            {
                if (_slots[i].HashCode == hashCode && Equals(_slots[i].Value, item))
                {
                    _slots[i].Value = item;
                    return;
                }
                collisionCount++;
            }

            if (_count == _slots.Length || 6 < collisionCount)
            {
                IncreaseCapacity();
                bucket = hashCode % _buckets.Length;
            }

            _slots[_count].HashCode = hashCode;
            _slots[_count].Value = item;
            _slots[_count].Next = _buckets[bucket];
            _count++;
            _buckets[bucket] = _count;
        }

        public void Clear()
        {
            for (var i = 0; i < _count; i++)
            {
                _buckets[i] = 0;
                _slots[_count].HashCode = 0;
                _slots[_count].Value = default(T);
                _slots[_count].Next = 0;
            }

            _count = 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for(var i = 0; i < _count; i++)
                yield return _slots[i].Value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion


        #region Helper methods


        private void IncreaseCapacity()
        {
            int newSize = ExpandPrime(_count * 2);

            var newSlots = new Slot[newSize];
            Array.Copy(_slots, newSlots, _count);

            var newBuckets = new int[newSize];
            for (var i = 0; i < _count; i++)
            {
                var bucket = newSlots[i].HashCode % newSize;
                newSlots[i].Next = newBuckets[bucket];
                newBuckets[bucket] = i + 1;
            }

            _slots = newSlots;
            _buckets = newBuckets;
        }

        private static int ExpandPrime(int oldSize)
        {
            int newSize = 2 * oldSize;

            // Allow the hash tables to grow to maximum possible size (~2G elements) before encountering capacity overflow.
            // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
            if ((uint)newSize > Constants.MaxPrimeArrayLength && Constants.MaxPrimeArrayLength > oldSize)
            {
                return Constants.MaxPrimeArrayLength;
            }

            return GetPrime(newSize);
        }

        private static int GetPrime(int min)
        {
            if (min < 0)
                throw new ArgumentException("Capacity Overflow");

            for (int i = 0; i < Constants.Primes.Length; i++)
            {
                int prime = Constants.Primes[i];
                if (prime >= min) return prime;
            }

            //outside of our predefined table. 
            //compute the hard way. 
            for (int i = (min | 1); i < Int32.MaxValue; i += 2)
            {
                if (IsPrime(i) && ((i - 1) % Constants.HashPrime != 0))
                    return i;
            }
            return min;
        }

        private static bool IsPrime(int candidate)
        {
            if ((candidate & 1) != 0)
            {
                int limit = (int)Math.Sqrt(candidate);
                for (int divisor = 3; divisor <= limit; divisor += 2)
                {
                    if ((candidate % divisor) == 0)
                        return false;
                }
                return true;
            }
            return (candidate == 2);
        }

        #endregion
    }
}
