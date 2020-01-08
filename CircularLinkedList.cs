using System;
using System.Collections;
using System.Collections.Generic;


namespace QRNGDotNet
{
    public class CircularLinkedList<T>: IList<T>
    {
        public class CircularLinkedListEnumerator<TEnum> : IEnumerator<TEnum>
        {
            private ArrayList arr;
            private IEnumerator enumerator;
            public object Current => this.enumerator.Current;

            TEnum IEnumerator<TEnum>.Current => (TEnum)this.enumerator.Current;

            public CircularLinkedListEnumerator(ArrayList arr)
            {
                this.arr = arr;
                this.enumerator = arr.GetEnumerator();
            }
            public bool MoveNext()
            {
                //if MoveNext is true 
                bool succeeded = this.enumerator.MoveNext();
                if (!succeeded )
                {
                    this.enumerator.Reset();
                    this.enumerator.MoveNext();
                }
                return true;
            }

            public void Reset()
            {
                this.enumerator.Reset();
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }
        private ArrayList arr;

        public CircularLinkedList()
        {
            this.arr = new ArrayList();
        }
        // TODO : Make not usable
        public object this[int index] { get => this.arr[index]; set => this.arr[index] =value; }

        public bool IsReadOnly => this.arr.IsReadOnly;

        public bool IsFixedSize => this.arr.IsFixedSize;

        public int Count => this.arr.Count;

        public object SyncRoot => this.arr.SyncRoot;

        public bool IsSynchronized => this.arr.IsSynchronized;

        T IList<T>.this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int IndexOf(T item)
        {
            return this.arr.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            this.arr.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            this.arr.Remove(index);
        }

        public void Add(T item)
        {
            this.arr.Add(item);
        }

        public void Clear()
        {
            this.arr.Clear();
        }

        public bool Contains(T item)
        {
            return this.arr.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.arr.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            this.arr.Remove(item);
            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new CircularLinkedListEnumerator<T>(this.arr);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new CircularLinkedListEnumerator<T>(this.arr);
        }
    }
}
