using System;
using System.Collections;
using System.Collections.Generic;


namespace QRNGDotNet
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CircularList<T>: IList<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        public class CircularListEnumerator<TEnum> : IEnumerator<TEnum>
        {
            private object SyncRoot;
            private ArrayList arr;
            private IEnumerator enumerator;
            /// <summary>
            /// The current element
            /// </summary>
            public object Current => this.enumerator.Current;

            TEnum IEnumerator<TEnum>.Current => (TEnum)this.enumerator.Current;
            /// <summary>
            /// 
            /// </summary>
            /// <param name="arr"></param>
            public CircularListEnumerator(ArrayList arr)
            {
                this.arr = arr;
                this.enumerator = arr.GetEnumerator();
                this.SyncRoot = arr.SyncRoot;

            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public bool MoveNext()
            {
                lock (this.SyncRoot) {
                    //if MoveNext is true 
                    bool succeeded = this.enumerator.MoveNext();
                    if (!succeeded)
                    {   
                        this.enumerator.Reset();
                        this.enumerator.MoveNext();
                    }
                }
                return true;
            }
            /// <summary>
            /// 
            /// </summary>
            public void Reset()
            {
                this.enumerator.Reset();
            }
            /// <summary>
            /// 
            /// </summary>
            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }
        private ArrayList arr;

        /// <summary>
        /// Constructor for CircularList
        /// </summary>
        public CircularList()
        {
            this.arr = new ArrayList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public object this[int index] { get => this.arr[index]; set => this.arr[index] =value; }

        /// <summary>
        /// True if the CircularList is readonly, false otherwise.
        /// </summary>
        public bool IsReadOnly => this.arr.IsReadOnly;

        /// <summary>
        /// True if the CircularList size is  fixed, false otherwise.
        /// </summary>
        public bool IsFixedSize => this.arr.IsFixedSize;

        /// <summary>
        /// The number of item in the CircularLinkedList
        /// </summary>
        public int Count => this.arr.Count;

        /// <summary>
        /// Return The SyncRoot see Array documentation
        /// </summary>
        public object SyncRoot => this.arr.SyncRoot;

        /// <summary>
        /// Return True if the CircularLinkedList is Synchronized, False otherwise.
        /// </summary>
        public bool IsSynchronized => this.arr.IsSynchronized;

        ///<summary>
        /// Not Implemented!!!
        ///</summary>
        T IList<T>.this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item">An object of type T</param>
        /// <returns>The index of the item</returns>
        public int IndexOf(T item)
        {
            return this.arr.IndexOf(item);
        }


        /// <summary>
        /// Insert an item at the index specified.
        /// </summary>
        /// <param name="index">Index where the item will be inserted.</param>
        /// <param name="item">An object of type T that needs to be inserted.</param>
        public void Insert(int index, T item)
        {
            this.arr.Insert(index, item);
        }

        /// <summary>
        /// Remove an element in the array at the index specified.
        /// </summary>
        /// <param name="index">The index where we will remove the element. </param>
        public void RemoveAt(int index)
        {
            this.arr.Remove(index);
        }

        /// <summary>
        /// insert an item at the end of the array.
        /// </summary>
        /// <param name="item">An item of generic type T to be inserted.</param>
        public void Add(T item)
        {
            this.arr.Add(item);
        }

        /// <summary>
        /// Remove all the element in the array.
        /// </summary>
        public void Clear()
        {
            this.arr.Clear();
        }

        /// <summary>
        /// return true if the array contains item.
        /// </summary>
        /// <param name="item"> An item of generic type T that we wnat to find. </param>
        /// <returns> return true if the CircularLinkedList contains</returns>
        public bool Contains(T item)
        {
            return this.arr.Contains(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"> An empty array of type T where we will copy the the content of the circularLinkedList. </param>
        /// <param name="arrayIndex"> The index at which we will start copying. </param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            this.arr.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns>True if it was able to remove the item in the array.</returns>

        public bool Remove(T item)
        {
            this.arr.Remove(item);
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns> The enumerator for the CircularLinkedList object</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new CircularListEnumerator<T>(this.arr);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns> The enumerator for the CircularLinkedList object.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new CircularListEnumerator<T>(this.arr);
        }
    }
}
