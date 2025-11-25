// <copyright file="ConcurrentSet.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// https://github.com/dotnet/roslyn/blob/6da1274c9d24c2f90a48290394a951b23617f2a3/src/Compilers/Core/Portable/InternalUtilities/ConcurrentSet.cs#L16
// namespace Roslyn.Utilities
namespace PuppeteerSharp.Helpers
{
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// A concurrent, simplified HashSet.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    internal sealed class ConcurrentSet<T> : ICollection<T>
    {
        /// <summary>
        /// The default concurrency level is 2. That means the collection can cope with up to two
        /// threads making simultaneous modifications without blocking.
        /// Note ConcurrentDictionary's default concurrency level is dynamic, scaling according to
        /// the number of processors.
        /// </summary>
        private const int DefaultConcurrencyLevel = 2;

        /// <summary>
        /// Taken from ConcurrentDictionary.DEFAULT_CAPACITY.
        /// </summary>
        private const int DefaultCapacity = 31;

        /// <summary>
        /// The backing dictionary. The values are never used; just the keys.
        /// </summary>
        private readonly ConcurrentDictionary<T, byte> dictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentSet{T}"/> class.
        /// Constructs a concurrent set with the default concurrency level.
        /// </summary>
        public ConcurrentSet()
        {
            this.dictionary = new ConcurrentDictionary<T, byte>(DefaultConcurrencyLevel, DefaultCapacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentSet{T}"/> class.
        /// Constructs a concurrent set using the specified equality comparer.
        /// </summary>
        /// <param name="equalityComparer">The equality comparer for values in the set.</param>
        public ConcurrentSet(IEqualityComparer<T> equalityComparer)
        {
            this.dictionary = new ConcurrentDictionary<T, byte>(DefaultConcurrencyLevel, DefaultCapacity, equalityComparer);
        }

        /// <summary>
        /// Gets obtain the number of elements in the set.
        /// </summary>
        /// <returns>The number of elements in the set.</returns>
        public int Count => this.dictionary.Count;

        /// <summary>
        /// Gets a value indicating whether determine whether the set is empty.</summary>
        /// <returns>true if the set is empty; otherwise, false.</returns>
        public bool IsEmpty => this.dictionary.IsEmpty;

        public bool IsReadOnly => false;

        /// <summary>
        /// Determine whether the given value is in the set.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>true if the set contains the specified value; otherwise, false.</returns>
        public bool Contains(T value)
        {
            return this.dictionary.ContainsKey(value);
        }

        /// <summary>
        /// Attempts to add a value to the set.
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <returns>true if the value was added to the set. If the value already exists, this method returns false.</returns>
        public bool Add(T value)
        {
            return this.dictionary.TryAdd(value, 0);
        }

        public void AddRange(IEnumerable<T> values)
        {
            if (values != null)
            {
                foreach (var v in values)
                {
                    this.Add(v);
                }
            }
        }

        /// <summary>
        /// Attempts to remove a value from the set.
        /// </summary>
        /// <param name="value">The value to remove.</param>
        /// <returns>true if the value was removed successfully; otherwise false.</returns>
        public bool Remove(T value)
        {
            return this.dictionary.TryRemove(value, out _);
        }

        /// <summary>
        /// Clear the set.
        /// </summary>
        public void Clear()
        {
            this.dictionary.Clear();
        }

        /// <summary>
        /// Obtain an enumerator that iterates through the elements in the set.
        /// </summary>
        /// <returns>An enumerator for the set.</returns>
        public KeyEnumerator GetEnumerator()
        {
            // PERF: Do not use dictionary.Keys here because that creates a snapshot
            // of the collection resulting in a List<T> allocation. Instead, use the
            // KeyValuePair enumerator and pick off the Key part.
            return new KeyEnumerator(this.dictionary);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.GetEnumeratorImpl();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumeratorImpl();
        }

        void ICollection<T>.Add(T item)
        {
            this.Add(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            // PERF: Do not use dictionary.Keys here because that creates a snapshot
            // of the collection resulting in a List<T> allocation.
            // Instead, enumerate the set and copy over the elements.
            foreach (var element in this)
            {
                array[arrayIndex++] = element;
            }
        }

        private IEnumerator<T> GetEnumeratorImpl()
        {
            // PERF: Do not use dictionary.Keys here because that creates a snapshot
            // of the collection resulting in a List<T> allocation. Instead, use the
            // KeyValuePair enumerator and pick off the Key part.
            foreach (var kvp in this.dictionary)
            {
                yield return kvp.Key;
            }
        }

        public struct KeyEnumerator
        {
            private readonly IEnumerator<KeyValuePair<T, byte>> kvpEnumerator;

            internal KeyEnumerator(IEnumerable<KeyValuePair<T, byte>> data)
            {
                this.kvpEnumerator = data.GetEnumerator();
            }

            public T Current => this.kvpEnumerator.Current.Key;

            public bool MoveNext()
            {
                return this.kvpEnumerator.MoveNext();
            }

            public void Reset()
            {
                this.kvpEnumerator.Reset();
            }
        }
    }
}
