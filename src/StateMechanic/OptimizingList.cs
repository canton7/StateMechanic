using System;
using System.Collections.Generic;
using System.Linq;

namespace StateMechanic
{
    /// <summary>
    /// List wrapper which is optimized for a single item
    /// </summary>
    /// <remarks>
    /// <see cref="EventInner{TEvent, TTransition}"/> usually only has one transition per state,
    /// in which case allocating an entire list to hold that single item is a waste. This optimizes the case
    /// of storing a single item, only expanding to a list when necessary.
    /// </remarks>
    internal struct OptimizingList<T> where T : class
    {
        private T singleInstance;
        private List<T> multipleInstances;

        public void Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (this.singleInstance == null)
            {
                this.singleInstance = item;
            }
            else if (this.multipleInstances == null)
            {
                this.multipleInstances = new List<T>()
                {
                    this.singleInstance,
                    item
                };
                this.singleInstance = null;
            }
            else
            {
                this.multipleInstances.Add(item);
            }
        }

        /// <summary>
        /// Get an IEnumerable for the item(s) held by this OptimizingList
        /// </summary>
        /// <remarks>
        /// This OptimizingList is not itself IEnumerable to avoid unnecessary boxing.
        /// </remarks>
        public IEnumerable<T> GetEnumerable()
        {
            if (this.singleInstance != null)
            {
                return this.YieldSingleInstance();
            }
            if (this.multipleInstances != null)
            {
                return this.multipleInstances;
            }
            return Enumerable.Empty<T>();
        }

        private IEnumerable<T> YieldSingleInstance()
        {
            yield return this.singleInstance;
        }

        public T FirstOrDefault(Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            if (this.singleInstance != null)
            {
                return predicate(this.singleInstance) ? this.singleInstance : null;
            }
            if (this.multipleInstances != null)
            {
                return this.multipleInstances.FirstOrDefault(predicate);
            }
            return null;
        }
    }
}
