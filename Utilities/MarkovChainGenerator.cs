using System;
using System.Collections.Generic;
using MathNet.Numerics.Distributions;
using System.Reflection;
using System.Threading;
using QRNGDotNet.StochasticProcess;
using QRNGDotNet;


namespace QRNGDotNet.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class MarkovChainGenerator
    {
        private int length;
        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        protected Semaphore sem;

        protected object SyncRoot;
        /// <summary>
        /// 
        /// </summary>
        protected CircularList<StochasticProcessBase> partitions;

        /// <summary>
        /// 
        /// </summary>
        protected IEnumerator<StochasticProcessBase> partitions_enum;

        /// <summary>
        /// 
        /// </summary>
        protected int d;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="qrng"></param>
        /// <param name="d"></param>
        /// <param name="MaxDegreeOfParallelism"></param>
        protected MarkovChainGenerator(Type qrng, int d, int MaxDegreeOfParallelism)
        {
        
            //Set The MaxDegreeofParallelism
            this.MaxDegreeOfParallelism = MaxDegreeOfParallelism;

            
            // check if qrng inherit from QRNG
            if (qrng.BaseType != typeof(QRNG))
            {
                throw new ArgumentException("The variable qrng does not inherit from QRNG");
            }
            this.partitions = new CircularList<StochasticProcessBase>();
            this.SyncRoot = this.partitions.SyncRoot;
            

            //set the number of dimension
            this.d = d;
            this.sem = new Semaphore(0, this.MaxDegreeOfParallelism);
            
        }
    }

    /// <summary>
    /// Generate a vector of quasi-random numbers with the continuous distribution T.
    /// </summary>
    public class ContinuousMarkovChainGenerator:MarkovChainGenerator
    {
        
        /// <summary>
        /// This constructor instantiate 
        /// </summary>
        /// <param name="qrng"> A Type argument, has to inherit ParallelQRNG.</param>
        /// <param name="process"></param>
        /// <param name="d"> the number of dimension to be created</param>
        /// <param name="MaxDegreeOfParallelism"> The maximum number of threads that the sequence will be generated with.</param>
        /// <param name ="param">The param</param>
        public ContinuousMarkovChainGenerator(Type qrng, StochasticProcessBase process, int d, int MaxDegreeOfParallelism, object[] param):base(qrng, d, MaxDegreeOfParallelism)
        {

            //instantiate an array of QRNGPartition to store the IDistribution to be created
            QRNG.QRNGPartition[][] dimensions = new QRNG.QRNGPartition[this.d][];
            for (uint i = 0; i < this.d; i++)
            {
                //Instantiate the QRNG with MaxDegreeofParallelism
                QRNG dimension = (QRNG)Activator.CreateInstance(qrng, param[0], this.MaxDegreeOfParallelism);

                dimensions[i] = dimension.GetPartitions();
            }

            // insert the IDistributions in the partitions CircularLinkedList
            for (int i = 0; i < this.MaxDegreeOfParallelism; i++)
            {
                // Instantiate a new array of QRNGPartition to temporarily store the partitions store in dimensions
                QRNG.QRNGPartition[] temp = new QRNG.QRNGPartition[this.d];

                // Store all the dimensions associated with a particular partitions i
                for (int k = 0; k < this.d; k++)
                {
                    //store the dimensions store at k, i into temp at k 
                    temp[k] = dimensions[k][i];
                }
                // Insert the IDistributions in the partitions CircularLinkedList
                this.partitions.Add(process.Clone(temp));
                this.partitions_enum = this.partitions.GetEnumerator();
            }
        }

        /// <summary>
        /// Next() Generate the next numbers in the sequence for the d dimensions defined during the instanciation. 
        /// </summary>
        /// <returns>A double[] containing the next numbers in the sequence for the d dimensions.</returns>
        public double[] Next()
        {
            // go to the next partitions
            this.partitions_enum.MoveNext();
            // copy the partitions into the parts array
            StochasticProcessBase parts = this.partitions_enum.Current;
            
            // Instantiates an array that will hold the quasi random vector
            double[] value = (double[])parts.Next();

            return value;
        }
    }
}
