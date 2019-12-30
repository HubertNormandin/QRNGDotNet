using System;
using System.Threading;

namespace CsQRNG
{
    public class CircularLinkedList<T>
    {
        
        internal class Node
        {

            public T Item;

            public Node Next;
            private SpinLock _lock = new SpinLock(true);
            private object lock_taken;

            public Node(T item)
            {
                this.Item = item;
                this.lock_taken = false;
            }

            public void Lock()
            {
                Monitor.Enter(this.lock_taken);
                
            }

            public void Unlock()
            {
                Monitor.Exit(this.lock_taken);

            }
        }

        private Node head;
        private Node tail;
        private Node current;
        private Node previous = null;
        public int Size;
        public object _lock = new object();

        public CircularLinkedList()
        {
            this.Size = 0;
            this.current = this.head;
            this.previous = this.head;
        }

        /// <summary>
        /// Add the item at the end of the circular list.
        /// </summary>
        /// <param name="item"> </param>
        /// <returns></returns>
        public bool Add(T item)
        {
            // CREATE NEW EMPTY NODE
            Node n = new Node(item);
            if (this.tail == null || this.head == null)
            {
                lock (this._lock)
                {
                    n.Next = n;
                    this.head = n;
                    this.tail = this.head;
                }
            }
            else
            {
                this.tail.Lock();
                Node curr = this.tail;
                try
                {
                    this.tail.Next = n;
                    n.Next = this.head;
                    this.tail = n;
                }
                finally
                {
                    curr.Unlock();
                }
            }
            this.Size++;
            return true;
        }

        /// <summary>
        /// Delete the current item and go to the next node.
        /// </summary>
        /// <returns>True if the operation was successful, false otherwise. </returns>
        public bool DeleteCurrent()
        {
            if (this.current == null || this.previous == null)
            {
                this.current = this.head;
                this.previous = this.tail;
            }
            
            this.previous.Lock();
            Node curr = this.current;
            curr.Lock();
            try
            {
                this.previous.Next = this.current.Next;
                this.current = current.Next;
                curr.Unlock();
                this.Size--;
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                this.previous.Lock();
            }
        }

        /// <summary>
        /// Delete the item specify by the parameter item. 
        /// </summary>
        /// <param name="item">The item to delete.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public bool Delete(T item)
        {
            if ((object)item == (object)this.current.Item)
            {
                return DeleteCurrent();
            }

            return false;
        }

        /// <summary>
        /// Goes to the next node and return the next item
        /// </summary>
        /// <returns>The next Item in the linked list</returns>
        public T Next()
        {
            T node = default(T);
            try
            {
                if (this.current == null)
                {
                    this.previous = tail;
                    this.current = head;
                }
                this.current.Lock();
                try
                {
                    node = this.current.Item;
                    this.previous = this.current;
                    // ATOMIC?
                    this.current = this.current.Next;
                }
                finally
                {
                    this.previous.Unlock();
                }
            }
            catch (NullReferenceException)
            {
                throw new NullReferenceException("The CircularLinkedList is empty.");
            }



            return node;

        }
    }
}