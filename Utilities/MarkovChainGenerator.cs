using System;
using System.Collections.Generic;
using MathNet.Numerics.Distributions;
using System.Reflection;

namespace QRNGDotNet.Utilities
{
    class MarkovChainGenerator<T> where T: IContinuousDistribution
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

        private CircularLinkedList<IContinuousDistribution[]> partitions;
        private IEnumerator<IContinuousDistribution[]> partitions_enum;
        public int d;
        private object _lock = new object();

        /// <summary>
        /// This constructor instantiate 
        /// </summary>
        /// <param name="qrng"> A Type argument, has to inherit ParallelQRNG.</param>
        /// <param name="d"> the number of dimension to be created</param>
        /// <param name="MaxDegreeOfParallelism"> The maximum number of threads that the sequence will be generated with.</param>
        ///  <param name="distribution">The distribution of the the quasi random point. if null The distribution will be uniform </param>
        public MarkovChainGenerator(Type qrng, int d, int MaxDegreeOfParallelism, IDistribution distribution = null)
        {
            //Set The MaxDegreeofParallelism
            this.MaxDegreeOfParallelism = MaxDegreeOfParallelism;
            this.partitions = new CircularLinkedList<IContinuousDistribution[]>();
            // check if qrng inherit from QRNG
            if (qrng.BaseType != typeof(QRNG))
            {
                throw new ArgumentException("The variable qrng does not inherit from ParallelQRNG");
            }
            //set the number of dimension
            this.d = d;

            //Get The Type of the dimension
            Type dist_type = distribution.GetType();

            //Get the PropertyInfo of the distribution
            PropertyInfo[] properties = dist_type.GetProperties();
            //instantiate an array that will hold the properties value
            object[] prop_value = new object[properties.Length];
            for (int i = 0; i < properties.Length; i++)
            {
                //Extract the Properties value from the Property Info and store them in prop_values
                prop_value[i] = properties[i].GetValue(distribution, null);
            }

            //instantiate an array of IDistribution to store the IDistribution to be created
            IContinuousDistribution[][] dimensions = (IContinuousDistribution[][])Array.CreateInstance(dist_type.MakeArrayType(), this.d);
            for (uint i = 0; i < this.d; i++)
            {
                //Instantiate the QRNG with MaxDegreeofParallelism
                QRNG dimension = (QRNG)Activator.CreateInstance(qrng, this.MaxDegreeOfParallelism);
                QRNG.QRNGPartition[] parts = dimension.GetPartitions();

                // instantiate an array of IDistribution
                IContinuousDistribution[] distributions = (IContinuousDistribution[])Array.CreateInstance(dist_type, this.MaxDegreeOfParallelism);
                for (int j = 0; j < this.MaxDegreeOfParallelism; j++)
                {
                    // Instantiate a distribution 
                    IContinuousDistribution dist = (IContinuousDistribution)Activator.CreateInstance(dist_type, prop_value);
                    // Set The RandomSource of the distribution to the QRNGPartitions j
                    dist.RandomSource = parts[j];
                    // Store the distribution just created in the distributions array
                    distributions[j] = dist;
                }
                dimensions[i] = distributions;
            }

            // insert the IDistributions in the partitions CircularLinkedList
            for (int i = 0; i < this.MaxDegreeOfParallelism; i++)
            {
                // Instantiate a new array of IDistribution to temporarily store the partitions store in dimensions
                IContinuousDistribution[] temp = new IContinuousDistribution[this.d];

                // Store all the dimensions associated with a particular partitions i
                for (int k = 0; k < this.d; k++)
                {
                    //store the dimensions store at k, i into temp at k 
                    temp[k] = dimensions[k][i];
                }
                // Insert the IDistributions in the partitions CircularLinkedList
                this.partitions.Add(temp);
            }
        }
        /// <summary>
        /// Next() Generate the next numbers in the sequence for the d dimensions defined during the instanciation. 
        /// </summary>
        /// <returns>A double[] containing the next numbers in the sequence for the d dimensions.</returns>
        public double[] Next()
        {
            // Will hold the partitions that will hold the partitions that will be used by the threads 
            IContinuousDistribution[] parts = null;
            lock (this._lock)
            {
                // go to the next partitions
                this.partitions_enum.MoveNext();
                // copy the partitions into the parts array
                parts = this.partitions_enum.Current;
            }
            // Instantiates an array that will hold the quasi random vector
            double[] value = new double[this.d];

            for (int i = 0; i < this.d; i++)
            {
                // Generate a sample for each partitions
                value[i] = parts[i].Sample();
            }

            return value;
        }
    }
}
