using System;
using System.Runtime.CompilerServices;

namespace CsQRNG.SobolSequence
{
    public class SobolDimension: ParallelQRNG
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

            public SobolPartition(uint init,  uint[] V, uint threads, uint thread_id)
            {
                this.X = new uint[this.length];
                this.X[0] = init;
                this.V = V;
                this.threads = threads;
                this.p =  (byte)Math.Log(threads, 2);
                this.thread_id = thread_id;
            }

            public override double Next()
            {
                uint yn;

                yn = this.X[k - 1];
                uint m = this.V[p];

                // EQUIVALENT OF K FOR THE DIRECTION NUMBER
                uint position = this.thread_id + this.k * this.threads;
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
                
                return yn / Two32;
            }

        }
        private int length = 2;
        private const double Two32 = 4294967296;
        private DirectionVectorLoader dvl = new DirectionVectorLoader('6');
        private Direction dir;
        private uint[] X;

        public SobolDimension()
        {
            this.dvl.MoveNext();
            this.dir = this.dvl.Current;
            this.X = new uint[this.length];
            this.X[0] = 0;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        public override double Next(uint k)
        {
            uint yn = this.X[k];
            uint m = this.dir.V[RightmostZeroBit(k)];
            if (this.length <= k + 1)
            {
                ResizeArray(ref this.X);
                this.length *= 2;
            }
            this.X[k+1] = yn ^ m;
            return this.X[k] / Two32;
        }

        public override QRNGPartition[] GetPartitions(int nb_partitions)
        {
            // CHECK IF NB OF PARTITION IS VALID
            if ((nb_partitions > 0) && ((nb_partitions & (~nb_partitions + 1)) != nb_partitions))
            {
                throw new ArgumentOutOfRangeException();
            }
       
            // GENERATE ALL THE PARTITIONS
            QRNGPartition[] partitions = new QRNGPartition[nb_partitions];
            for (uint i = 0; i < nb_partitions; i++)
            {
                // GENERATE THE FIRST NUMBER IN THE SEQUENCE
                this.Next(i);
                // COPY THE DIRECTION NUMBERS INTO A NEW ARRAY
                uint[] v = new uint[this.dir.V.Length];
                Array.Copy(this.dir.V, v, v.Length);
                // INSTANTIATE THE PARTITION
                partitions[i] = new SobolPartition(this.X[i], v, (uint) nb_partitions, i); 
            }

            return partitions;
        }
    }
}
