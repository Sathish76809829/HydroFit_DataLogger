using RMS.Service.Abstractions.Models;
using RMS.Service.Abstractions.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace RMS.Service.Abstractions
{
    /// <summary>
    /// Signal Comparer interface to compare <see cref="ISignalModel"/> with other type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISignalComparer<T> 
    {
        int GetHashCode(T item);
        bool Equals(ISignalModel x, T y);
    }

    #region SignalSet
    /// <summary>
    /// HashSet implementation for SignalModel
    /// <seealso cref="https://referencesource.microsoft.com/#System.Core/System/Collections/Generic/HashSet.cs"/>
    /// </summary>
    /// <typeparam name="T">Type of <see cref="ISignalModel"/></typeparam>
    public class SignalSet<T> : ICollection<T>, IReadOnlyCollection<T> where T : ISignalModel
    {
        private const int Lower31BitMask = 0x7FFFFFFF;

        internal struct Slot
        {
            internal int hashCode;      // Lower 31 bits of hash code, -1 if unused
            internal int next;          // Index of next entry, -1 if last
            internal T value;
        }

        private int[] m_buckets;
        private Slot[] m_slots;
        private int m_count;
        private int m_lastIndex;
        private int m_freeList;
        private readonly IEqualityComparer<T> m_comparer;
        private int m_version;

        public SignalSet(IEqualityComparer<T> comparer)
        {
            if (comparer == null)
            {
                comparer = EqualityComparer<T>.Default;
            }

            m_comparer = comparer;
            m_lastIndex = 0;
            m_count = 0;
            m_freeList = -1;
            m_version = 0;
        }

        public SignalSet(int capacity, IEqualityComparer<T> comparer)
           : this(comparer)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException("capacity");
            }

            if (capacity > 0)
            {
                Initialize(capacity);
            }
        }


        /// <summary>
        /// Number of elements in this hashset
        /// </summary>
        public int Count
        {
            get { return m_count; }
        }

        /// <summary>
        /// Whether this is readonly
        /// </summary>
        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        void ICollection<T>.Add(T item)
        {
            AddIfNotPresent(item);
        }

        private void Initialize(int capacity)
        {
            Debug.Assert(m_buckets == null, "Initialize was called but m_buckets was non-null");

            int size = HashHelper.GetPrime(capacity);

            m_buckets = new int[size];
            m_slots = new Slot[size];
        }

        /// <summary>
        /// Expand to new capacity. New capacity is next prime greater than or equal to suggested 
        /// size. This is called when the underlying array is filled. This performs no 
        /// defragmentation, allowing faster execution; note that this is reasonable since 
        /// AddIfNotPresent attempts to insert new elements in re-opened spots.
        /// </summary>
        /// <param name="sizeSuggestion"></param>
        private void IncreaseCapacity()
        {
            Debug.Assert(m_buckets != null, "IncreaseCapacity called on a set with no elements");

            int newSize = HashHelper.ExpandPrime(m_count);
            if (newSize <= m_count)
            {
                throw new ArgumentException("Overflow Signals");
            }

            // Able to increase capacity; copy elements to larger array and rehash
            SetCapacity(newSize, false);
        }

        /// <summary>
        /// Set the underlying buckets array to size newSize and rehash.  Note that newSize
        /// *must* be a prime.  It is very likely that you want to call IncreaseCapacity()
        /// instead of this method.
        /// </summary>
        private void SetCapacity(int newSize, bool forceNewHashCodes)
        {
            Slot[] newSlots = new Slot[newSize];
            if (m_slots != null)
            {
                Array.Copy(m_slots, 0, newSlots, 0, m_lastIndex);
            }

            if (forceNewHashCodes)
            {
                for (int i = 0; i < m_lastIndex; i++)
                {
                    if (newSlots[i].hashCode != -1)
                    {
                        newSlots[i].hashCode = InternalGetHashCode(newSlots[i].value);
                    }
                }
            }

            int[] newBuckets = new int[newSize];
            for (int i = 0; i < m_lastIndex; i++)
            {
                int bucket = newSlots[i].hashCode % newSize;
                newSlots[i].next = newBuckets[bucket] - 1;
                newBuckets[bucket] = i + 1;
            }
            m_slots = newSlots;
            m_buckets = newBuckets;
        }

        public void AddOrUpdate(T value)
        {
            AddOrUpdate(value, DefaultAddFactory, (_) => value);
        }

        static T DefaultAddFactory(T value) => value;

        public void AddOrUpdate(T value, Func<T, T> addValueFactory, Func<T, T> updateValueFactory)
        {
            if (m_buckets == null)
            {
                Initialize(0);
            }

            int hashCode = InternalGetHashCode(value);
            int bucket = hashCode % m_buckets.Length;
            for (int i = m_buckets[bucket] - 1; i >= 0; i = m_slots[i].next)
            {
                if (m_slots[i].hashCode == hashCode && m_comparer.Equals(m_slots[i].value, value))
                {
                    m_slots[i].value = updateValueFactory(m_slots[i].value);
                    m_version++;
                    return;
                }
            }
            int index;
            if (m_freeList >= 0)
            {
                index = m_freeList;
                m_freeList = m_slots[index].next;
            }
            else
            {
                if (m_lastIndex == m_slots.Length)
                {
                    IncreaseCapacity();
                    // this will change during resize
                    bucket = hashCode % m_buckets.Length;
                }
                index = m_lastIndex;
                m_lastIndex++;
            }
            m_slots[index].hashCode = hashCode;
            m_slots[index].value = addValueFactory(value);
            m_slots[index].next = m_buckets[bucket] - 1;
            m_buckets[bucket] = index + 1;
            m_count++;
            m_version++;
        }

        /// <summary>
        /// Adds value to HashSet if not contained already
        /// Returns true if added and false if already present
        /// </summary>
        /// <param name="value">value to find</param>
        /// <returns></returns>
        private bool AddIfNotPresent(T value)
        {
            if (m_buckets == null)
            {
                Initialize(0);
            }

            int hashCode = InternalGetHashCode(value);
            int bucket = hashCode % m_buckets.Length;
            for (int i = m_buckets[hashCode % m_buckets.Length] - 1; i >= 0; i = m_slots[i].next)
            {
                if (m_slots[i].hashCode == hashCode && m_comparer.Equals(m_slots[i].value, value))
                {
                    return false;
                }
            }

            int index;
            if (m_freeList >= 0)
            {
                index = m_freeList;
                m_freeList = m_slots[index].next;
            }
            else
            {
                if (m_lastIndex == m_slots.Length)
                {
                    IncreaseCapacity();
                    // this will change during resize
                    bucket = hashCode % m_buckets.Length;
                }
                index = m_lastIndex;
                m_lastIndex++;
            }
            m_slots[index].hashCode = hashCode;
            m_slots[index].value = value;
            m_slots[index].next = m_buckets[bucket] - 1;
            m_buckets[bucket] = index + 1;
            m_count++;
            m_version++;

            return true;
        }

        /// <summary>
        /// Workaround Comparers that throw ArgumentNullException for GetHashCode(null).
        /// </summary>
        /// <param name="item"></param>
        /// <returns>hash code</returns>
        private int InternalGetHashCode(T item)
        {
            if (item == null)
            {
                return 0;
            }
            return m_comparer.GetHashCode(item) & Lower31BitMask;
        }

        /// <summary>
        /// Add item to this HashSet. Returns bool indicating whether item was added (won't be 
        /// added if already present)
        /// </summary>
        /// <param name="item"></param>
        /// <returns>true if added, false if already present</returns>
        public bool Add(T item)
        {
            return AddIfNotPresent(item);
        }

        /// <summary>
        /// Remove all items from this set. This clears the elements but not the underlying 
        /// buckets and slots array. Follow this call by TrimExcess to release these.
        /// </summary>
        public void Clear()
        {
            if (m_lastIndex > 0)
            {
                Debug.Assert(m_buckets != null, "m_buckets was null but m_lastIndex > 0");

                // clear the elements so that the gc can reclaim the references.
                // clear only up to m_lastIndex for m_slots 
                Array.Clear(m_slots, 0, m_lastIndex);
                Array.Clear(m_buckets, 0, m_buckets.Length);
                m_lastIndex = 0;
                m_count = 0;
                m_freeList = -1;
            }
            m_version++;
        }

        /// <summary>
        /// Checks if this hashset contains the item
        /// </summary>
        /// <param name="item">item to check for containment</param>
        /// <returns>true if item contained; false if not</returns>
        public bool Contains(T item)
        {
            if (m_buckets != null)
            {
                int hashCode = InternalGetHashCode(item);
                // see note at "HashSet" level describing why "- 1" appears in for loop
                for (int i = m_buckets[hashCode % m_buckets.Length] - 1; i >= 0; i = m_slots[i].next)
                {
                    if (m_slots[i].hashCode == hashCode && m_comparer.Equals(m_slots[i].value, item))
                    {
                        return true;
                    }
                }
            }
            // either m_buckets is null or wasn't found
            return false;
        }

        /// <summary>
        /// Copy items in this hashset to array, starting at arrayIndex
        /// </summary>
        /// <param name="array">array to add items to</param>
        /// <param name="arrayIndex">index to start at</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            CopyTo(array, arrayIndex, m_count);
        }

        /// <summary>
        /// Remove item from this hashset
        /// </summary>
        /// <param name="item">item to remove</param>
        /// <returns>true if removed; false if not (i.e. if the item wasn't in the HashSet)</returns>
        public bool Remove(T item)
        {
            if (m_buckets != null)
            {
                int hashCode = InternalGetHashCode(item);
                int bucket = hashCode % m_buckets.Length;
                int last = -1;
                for (int i = m_buckets[bucket] - 1; i >= 0; last = i, i = m_slots[i].next)
                {
                    if (m_slots[i].hashCode == hashCode && m_comparer.Equals(m_slots[i].value, item))
                    {
                        if (last < 0)
                        {
                            // first iteration; update buckets
                            m_buckets[bucket] = m_slots[i].next + 1;
                        }
                        else
                        {
                            // subsequent iterations; update 'next' pointers
                            m_slots[last].next = m_slots[i].next;
                        }
                        m_slots[i].hashCode = -1;
                        m_slots[i].value = default(T);
                        m_slots[i].next = m_freeList;

                        m_count--;
                        m_version++;
                        if (m_count == 0)
                        {
                            m_lastIndex = 0;
                            m_freeList = -1;
                        }
                        else
                        {
                            m_freeList = i;
                        }
                        return true;
                    }
                }
            }
            // either m_buckets is null or wasn't found
            return false;
        }

        /// <summary>
        /// Searches the set for a given value and returns the equal value it finds, if any.
        /// </summary>
        /// <param name="equalValue">The value to search for.</param>
        /// <param name="actualValue">The value from the set that the search found, or the default value of <typeparamref name="T"/> when the search yielded no match.</param>
        /// <returns>A value indicating whether the search was successful.</returns>
        /// <remarks>
        /// This can be useful when you want to reuse a previously stored reference instead of 
        /// a newly constructed one (so that more sharing of references can occur) or to look up
        /// a value that has more complete data than the value you currently have, although their
        /// comparer functions indicate they are equal.
        /// </remarks>
        public bool TryGetValue<U>([DisallowNull] U equalValue, [DisallowNull] ISignalComparer<U> comparer, out T actualValue)
        {
            if (m_buckets != null)
            {
                int i = InternalIndexOf(equalValue, comparer);
                if (i >= 0)
                {
                    actualValue = m_slots[i].value;
                    return true;
                }
            }
            actualValue = default(T);
            return false;
        }


        /// <summary>
        /// Used internally by set operations which have to rely on bit array marking. This is like
        /// Contains but returns index in slots array. 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private int InternalIndexOf<U>(U item, ISignalComparer<U> comparer)
        {
            int hashCode = comparer.GetHashCode(item) & Lower31BitMask;
            for (int i = m_buckets[hashCode % m_buckets.Length] - 1; i >= 0; i = m_slots[i].next)
            {
                if ((m_slots[i].hashCode) == hashCode && comparer.Equals(m_slots[i].value, item))
                {
                    return i;
                }
            }
            // wasn't found
            return -1;
        }


        public void CopyTo(T[] array) { CopyTo(array, 0, m_count); }

        public void CopyTo(T[] array, int arrayIndex, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            // check array index valid index into array
            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException("arrayIndex");
            }

            // also throw if count less than 0
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            // will array, starting at arrayIndex, be able to hold elements? Note: not
            // checking arrayIndex >= array.Length (consistency with list of allowing
            // count of 0; subsequent check takes care of the rest)
            if (arrayIndex > array.Length || count > array.Length - arrayIndex)
            {
                throw new ArgumentException("Arrary too small in CopyTo()");
            }

            int numCopied = 0;
            for (int i = 0; i < m_lastIndex && numCopied < count; i++)
            {
                if (m_slots[i].hashCode >= 0)
                {
                    array[arrayIndex + numCopied] = m_slots[i].value;
                    numCopied++;
                }
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }


        public struct Enumerator : IEnumerator<T>, System.Collections.IEnumerator
        {
            private readonly SignalSet<T> set;
            private int index;
            private int version;
            private T current;

            internal Enumerator(SignalSet<T> set)
            {
                this.set = set;
                index = 0;
                version = set.m_version;
                current = default(T);
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (version != set.m_version)
                {
                    throw new InvalidOperationException("Failed version");
                }

                while (index < set.m_lastIndex)
                {
                    if (set.m_slots[index].hashCode >= 0)
                    {
                        current = set.m_slots[index].value;
                        index++;
                        return true;
                    }
                    index++;
                }
                index = set.m_lastIndex + 1;
                current = default(T);
                return false;
            }

            public T Current
            {
                get
                {
                    return current;
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    if (index == 0 || index == set.m_lastIndex + 1)
                    {
                        throw new InvalidOperationException("Enumeration can't happen");
                    }
                    return Current;
                }
            }

            void System.Collections.IEnumerator.Reset()
            {
                if (version != set.m_version)
                {
                    throw new InvalidOperationException("Failed version");
                }

                index = 0;
                current = default(T);
            }
        }
    } 
    #endregion
}
