using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace QRNGDotNet.SobolSequence
{
    public class SobolSequence: ParallelQRNG
    {
        public class SobolPartition: QRNGPartition
        {
            private byte p;
            private readonly uint threads;
            private uint[] V;
            private uint k=1;
            private readonly uint thread_id;
            private uint[] X;
            private int length = 2;
            private object _lock = new object();

            public override void Reset()
            {
                this.k = 1;
            }
            public SobolPartition(uint init,  uint[] V, uint threads, uint thread_id)
            {
                this.X = new uint[this.length];
                this.X[0] = init;
                this.V = V;
                this.threads = threads;
                this.p =  (byte)Math.Log(threads, 2);
                this.thread_id = thread_id;
            }

            public override uint Next()
            {

                uint yn = this.X[this.k - 1];
                uint m = this.V[this.p];

                // EQUIVALENT OF K FOR THE DIRECTION NUMBER
                uint position = (uint)(this.thread_id + this.k * this.threads);
                uint r = RightmostZeroBit((position - this.threads) | (this.threads - 1));
                uint v = this.V[r];

                // DOUBLE THE SIZE OF THE ARRAY
                lock (this._lock)
                {
                    if (this.length <= (k + 1))
                    {
                        ResizeArray(ref this.X);
                        this.length *= 2;
                    }
                }
                this.X[k] = yn ^ m ^ v;
                this.k++;
                return yn;
            }

        }
        private const double TWO32 = 4294967296;


        private CircularLinkedList<SobolPartition> partitions;
        private IEnumerator<SobolPartition> partitions_enum;
        private DirectionLoader.DirectionNumberLoader dvl;
        private Direction dir;
        private object _lock = new object();

        private int length;
        public int MaxDegreeOfParallelism
        {
            get => this.length;
            set
            {
                // check whether the value is a power of 2
                if (((value > 0) && ((value & (~value + 1)) == value))||value==1)
                {
                    this.length = value;
                }
                else
                    throw new ArgumentOutOfRangeException();
            }
        }

        public SobolSequence(string criteria = "5", int MaxDegreeOfParallelism = 1)
        {
            this.dvl = DirectionLoader.GetDirectionNumberLoader(criteria);
            //Get the Next Directions
            this.dir = this.dvl.Next();

            // Set the number of partition
            this.MaxDegreeOfParallelism = MaxDegreeOfParallelism;

            // Instantiates partitions
            this.partitions = new CircularLinkedList<SobolPartition>();
            
            //Generate all the partitions
            uint x = 0;
            uint[] v = this.dir.V();
            for (uint k = 0; k < this.MaxDegreeOfParallelism; k++)
            {
                this.partitions.Add(new SobolPartition(x, v, (uint)this.MaxDegreeOfParallelism, k));
                uint yn = x;
                uint m = v[RightmostZeroBit(k)];
                x ^= m;
            }
            // Get the Enumerator for the circularlinkedlist
            this.partitions_enum = this.partitions.GetEnumerator();
        }
        public override void Reset()
        {
            lock (this._lock)
            {
                for(int i =0; i < this.MaxDegreeOfParallelism; i++)
                {
                    this.partitions_enum.MoveNext();
                    this.partitions_enum.Reset();
                }
            }
        }
        public override void ResetDimension()
        {
            this.dvl.Reset();
        }

        protected static uint RightmostZeroBit(uint n)
        {
            uint i = 1;
            while ((n & 1) == 1)
            {
                n >>= 1;
                ++i;
            }
            return i;
        }

        private static void ResizeArray<T>(ref T[] x)
        {
            T[] temp = new T[x.Length*2];
            for (int i = 0; i<x.Length; i++)
            {
                temp[i] = x[i];
            }
            x = temp;
        }

        #region RANDOM_INTERFACE
        public uint NextUInt()
        {
            SobolPartition parts = null;
            lock (this._lock)
            {
                this.partitions_enum.MoveNext();
                parts = this.partitions_enum.Current;
            }
            return parts.Next();
        }

        public uint NextUInt(long maxValue)
        {
            if (maxValue < 0)
            {
                throw new ArgumentOutOfRangeException("maxValue", "maxValue is negative");
            }
            return (uint)(this.NextDouble() * maxValue);
        }

        public uint NextUInt(long minValue, long maxValue)
        {
            if (minValue < maxValue)
            {
                throw new ArgumentOutOfRangeException("maxValue", "maxValue is negative");
            }
            return (this.NextUInt() * (uint)(maxValue - minValue + 1)) << 0;
        }

        public override int Next()
        {
            return (int)(this.NextDouble() * Int32.MaxValue);
        }
        public override int Next(int maxValue)
        {
            if (maxValue < 0)
            {
                throw new ArgumentOutOfRangeException("maxValue", "maxValue is negative");
            }
            return (this.Next() * (maxValue + 1)) << 0;
        }

        public override int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException("minValue", "minValue is smaller than maxValue");
            }
            return (this.Next() * (maxValue - minValue + 1)) << 0;
        }

        public new void NextBytes(byte[] buffer)
        {
            throw new NotImplementedException();
        }

        public override double NextDouble()
        {
            return this.NextUInt() / TWO32;
        }
        #endregion
        public override QRNGPartition[] GetPartitions()
        {
            SobolPartition[] a = new SobolPartition[this.partitions.Count];
            this.partitions.CopyTo(a, 0);
            return a;
        }
    }
}
