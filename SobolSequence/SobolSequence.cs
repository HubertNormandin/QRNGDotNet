using System;
using System.Threading;
using System.Collections.Generic;


namespace QRNGDotNet.SobolSequence
{
    public class SobolSequence : QRNG
    {
        public class SobolPartition : QRNGPartition
        {
            private byte p;
            private readonly uint threads;
            private uint[] V;
            private uint k = 1;
            private readonly uint thread_id;
            private uint[] X;
            private int length = 2;
            private Mutex mutex = new Mutex();

            public override void Reset()
            {
                this.k = 1;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="init"></param>
            /// <param name="V"></param>
            /// <param name="threads"></param>
            /// <param name="thread_id"></param>
            public SobolPartition(uint init, uint[] V, uint threads, uint thread_id)
            {
                this.X = new uint[this.length];
                this.X[0] = init;
                this.V = V;
                this.threads = threads;
                this.p = (byte)Math.Log(threads, 2);
                this.thread_id = thread_id;
            }

            #region Random Implementation
            /// <summary>
            /// Generate the next uint in the sequences up to UInt32.MaxValue.
            /// </summary>
            /// <returns>The next uint in Sobol's sequence.</returns>
            public uint NextUInt()
            {
                uint yn = this.X[this.k - 1];
                uint m = this.V[this.p];

                // EQUIVALENT OF K FOR THE DIRECTION NUMBER
                uint position = (uint)(this.thread_id + this.k * this.threads);
                uint r = RightmostZeroBit((position - this.threads) | (this.threads - 1));
                uint v = this.V[r];

                // DOUBLE THE SIZE OF THE ARRAY
                mutex.WaitOne();
                    if (this.length <= (k + 1))
                    {
                        ResizeArray(ref this.X);
                        this.length *= 2;
                    }
                mutex.ReleaseMutex();
                this.X[k] = yn ^ m ^ v;
                this.k++;
                return yn;
            }

            public override uint GetThreadID()
            {
                return this.thread_id;
            }
            /// <summary>
            /// Generate the next uint in the sequences up to maxValue.
            /// </summary>
            /// <param name="maxValue">The maximum value that can be generated.</param>
            /// <returns>The next uint in Sobol's sequence, subject to upper bound maxValue.</returns>
            public uint NextUInt(long maxValue)
            {
                if (maxValue < 0)
                {
                    throw new ArgumentOutOfRangeException("maxValue", "maxValue is negative");
                }
                return (uint)(this.NextDouble() * maxValue);
            }

            /// <summary>
            /// Generate the next uint in the sequences, between the bounds minValue-MaxValue.
            /// </summary>
            /// <param name="minValue">The minimum value that can be generated.</param>
            /// <param name="maxValue">The maximum value that can be generated.</param>
            /// <returns>The next uint in Sobol's sequence, subject to bounds minValue-maxValue.</returns>
            public uint NextUInt(long minValue, long maxValue)
            {
                if (minValue < maxValue)
                {
                    throw new ArgumentOutOfRangeException("maxValue", "maxValue is negative");
                }
                return (this.NextUInt() * (uint)(maxValue - minValue + 1)) << 0;
            }

            /// <summary>
            /// Generate the next int in the sequences up to Int32.MaxValue.
            /// </summary>
            /// <returns>The next int in Sobol's sequence.</returns>
            public override int Next()
            {
                return (int)(this.NextDouble() * Int32.MaxValue);
            }

            /// <summary>
            /// Generate the next int in the sequences up to maxValue.
            /// </summary>
            /// <param name="maxValue">The maximum value that can be generated.</param>
            /// <returns>The next int in Sobol's sequence, subject to upper bound maxValue.</returns>
            public override int Next(int maxValue)
            {
                if (maxValue < 0)
                {
                    throw new ArgumentOutOfRangeException("maxValue", "maxValue is negative");
                }
                return (this.Next() * (maxValue + 1)) << 0;
            }

            /// <summary>
            /// Generate the next int in the sequences, between the bounds minValue-MaxValue.
            /// </summary>
            /// <param name="minValue">The minimum value that can be generated.</param>
            /// <param name="maxValue">The maximum value that can be generated.</param>
            /// <returns>The next int in Sobol's sequence, subject to upper bound maxValue.</returns>
            public override int Next(int minValue, int maxValue)
            {
                if (minValue > maxValue)
                {
                    throw new ArgumentOutOfRangeException("minValue", "minValue is smaller than maxValue");
                }
                return (this.Next() * (maxValue - minValue + 1)) << 0;
            }
            /// <summary>
            /// Not implemented!!!
            /// </summary>
            /// <param name="buffer"></param>
            public new void NextBytes(byte[] buffer)
            {
                throw new NotImplementedException();
            }
            /// <summary>
            /// Generate the next int in the sequences up to Int32.MaxValue.
            /// </summary>
            /// <returns>The next double in Sobol's sequence.</returns>
            public override double NextDouble()
            {
                return this.NextUInt() / TWO32;
            }
            #endregion
        }
        private const double TWO32 = 4294967296;


        private CircularList<SobolPartition> partitions;
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
                if (((value > 0) && ((value & (~value + 1)) == value)) || value == 1)
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
            this.partitions = new CircularList<SobolPartition>();

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

        /// <summary>
        /// Reset every partitions to the first number
        /// </summary>
        public override void Reset()
        {
            lock (this._lock)
            {
                for (int i = 0; i < this.MaxDegreeOfParallelism; i++)
                {
                    this.partitions_enum.MoveNext();
                    this.partitions_enum.Reset();
                }
            }
        }

        /// <summary>
        /// Reset the dimension to the first
        /// </summary>
        public override void ResetDimension()
        {
            this.dvl.Reset();
        }

        /// <summary>
        /// Return the position of the rightmost zero bit of the number n
        /// </summary>
        /// <param name="n">a number</param>
        /// <returns>The rightmost zero bit.</returns>
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
            T[] temp = new T[x.Length * 2];
            for (int i = 0; i < x.Length; i++)
            {
                temp[i] = x[i];
            }
            x = temp;
        }

        #region RANDOM_INTERFACE
        /// <summary>
        /// Generate the next uint in the sequences up to UInt32.MaxValue.
        /// </summary>
        /// <returns>The next uint in Sobol's sequence.</returns>
        public uint NextUInt()
        {
            SobolPartition parts = null;
            lock (this._lock)
            {
                this.partitions_enum.MoveNext();
                parts = this.partitions_enum.Current;
            }
            return parts.NextUInt();
        }

        /// <summary>
        /// Generate the next uint in the sequences up to maxValue.
        /// </summary>
        /// <param name="maxValue">The maximum value that can be generated.</param>
        /// <returns>The next uint in Sobol's sequence, subject to upper bound maxValue.</returns>
        public uint NextUInt(long maxValue)
        {
            if (maxValue < 0)
            {
                throw new ArgumentOutOfRangeException("maxValue", "maxValue is negative");
            }
            return (uint)(this.NextDouble() * maxValue);
        }

        /// <summary>
        /// Generate the next uint in the sequences, between the bounds minValue-MaxValue.
        /// </summary>
        /// <param name="minValue">The minimum value that can be generated.</param>
        /// <param name="maxValue">The maximum value that can be generated.</param>
        /// <returns>The next uint in Sobol's sequence, subject to bounds minValue-maxValue.</returns>
        public uint NextUInt(long minValue, long maxValue)
        {
            if (minValue < maxValue)
            {
                throw new ArgumentOutOfRangeException("maxValue", "maxValue is negative");
            }
            return (this.NextUInt() * (uint)(maxValue - minValue + 1)) << 0;
        }

        /// <summary>
        /// Generate the next int in the sequences up to Int32.MaxValue.
        /// </summary>
        /// <returns>The next int in Sobol's sequence.</returns>
        public override int Next()
        {
            return (int)(this.NextDouble() * Int32.MaxValue);
        }

        /// <summary>
        /// Generate the next int in the sequences up to maxValue.
        /// </summary>
        /// <param name="maxValue">The maximum value that can be generated.</param>
        /// <returns>The next int in Sobol's sequence, subject to upper bound maxValue.</returns>
        public override int Next(int maxValue)
        {
            if (maxValue < 0)
            {
                throw new ArgumentOutOfRangeException("maxValue", "maxValue is negative");
            }
            return (this.Next() * (maxValue + 1)) << 0;
        }

        /// <summary>
        /// Generate the next int in the sequences, between the bounds minValue-MaxValue.
        /// </summary>
        /// <param name="minValue">The minimum value that can be generated.</param>
        /// <param name="maxValue">The maximum value that can be generated.</param>
        /// <returns>The next int in Sobol's sequence, subject to upper bound maxValue.</returns>
        public override int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException("minValue", "minValue is smaller than maxValue");
            }
            return (this.Next() * (maxValue - minValue + 1)) << 0;
        }

        /// <summary>
        /// Not implemented!!!
        /// </summary>
        /// <param name="buffer"></param>
        public new void NextBytes(byte[] buffer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Generate the next int in the sequences up to Int32.MaxValue.
        /// </summary>
        /// <returns>The next double in Sobol's sequence.</returns>
        public override double NextDouble()
        {
            return this.NextUInt() / TWO32;
        }
        #endregion

        /// <summary>
        /// Return a copy of the partitions used by this instance of QRNG
        /// </summary>
        /// <returns> A QRNGPartition[] containing all the partition of the Sobol Sequences. </returns>
        public override QRNGPartition[] GetPartitions()
        {
            SobolPartition[] a = new SobolPartition[this.partitions.Count];
            this.partitions.CopyTo(a, 0);
            return a;
        }
    }
}
