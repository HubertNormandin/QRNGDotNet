using System;
using System.Collections.Generic;
using System.Threading;

namespace QRNGDotNet
{

    // BASE INTERFACE
    public abstract class QRNG: Random
    {
        public abstract void ResetDimension();
        public abstract void Reset();
    }
    //INTERFACE FOR PARALLEL QRNG
    public abstract class ParallelQRNG: QRNG
    {
        public abstract class QRNGPartition
        {
            public abstract uint Next();
            public abstract void Reset();
        }

        public abstract QRNGPartition[] GetPartitions();
    }

    public class ParallelQRNGGenerator
    {
        private int length;
        public int MaxDegreeOfParallelism
        {
            get => this.length;
            set
            {
                // check whether the value is a power of 2
                if ((value > 0) && ((value & (~value + 1)) == value))
                {
                    this.length = value;
                    //TODO add change in number of partitions
                }
                else
                    throw new ArgumentOutOfRangeException();
            }
        }

        private CircularLinkedList<ParallelQRNG.QRNGPartition[]> partitions;
        private IEnumerator<ParallelQRNG.QRNGPartition[]> partitions_enum;
        public uint d;
        private object _lock = new object();

        /// <summary>
        /// This constructor instantiate 
        /// </summary>
        /// <param name="qrng"> A Type argument, has to inherit ParallelQRNG.</param>
        /// <param name="d"> the number of dimension to be created</param>
        /// <param name="MaxDegreeOfParallelism"> The maximum number of threads that the sequence will be generated with.</param>
        public ParallelQRNGGenerator(Type qrng, uint d, int MaxDegreeOfParallelism)
        {
            this.MaxDegreeOfParallelism = MaxDegreeOfParallelism;
            this.partitions = new CircularLinkedList<ParallelQRNG.QRNGPartition[]>();
            // check if qrng inherit from 
            if (qrng.BaseType != typeof(ParallelQRNG))
            {
                throw new ArgumentException("The variable qrng does not inherit from ParallelQRNG");
            }
            this.d = d;
            ParallelQRNG.QRNGPartition[][] dim = new ParallelQRNG.QRNGPartition[this.d][];
            for (uint i = 0; i < this.d; i++)
            {
                ParallelQRNG dimension = (ParallelQRNG)Activator.CreateInstance(qrng);
                dim[i] = dimension.GetPartitions();

            }

            for(int i = 0; i<this.MaxDegreeOfParallelism; i++)
            {
                var temp = new ParallelQRNG.QRNGPartition[this.d];
                for(int k = 0; k<this.d; k++)
                {
                    temp[k] = dim[k][i];
                }
                this.partitions.Add(temp);
            }

        }

        /// <summary>
        /// Next() Generate the next numbers in the sequence for the d dimensions defined during the instanciation. 
        /// </summary>
        /// <returns>A double[] containing the next numbers in the sequence for the d dimensions.</returns>
        public double[] Next()
        {
            ParallelQRNG.QRNGPartition[] parts = null;
            lock (this._lock)
            {
                this.partitions_enum.MoveNext();
                parts = this.partitions_enum.Current;
            }
            double[] value = new double[this.d];
 
            for (int i = 0; i < this.d; i++)
            {
                value[i] = parts[i].Next();
            }
            
            return value;
        }
    }
}