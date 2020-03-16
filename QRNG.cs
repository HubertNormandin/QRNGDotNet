using System;
using System.Collections.Generic;
using System.Threading;
using MathNet.Numerics.Distributions;
using System.Reflection;

namespace QRNGDotNet
{

    /// <summary>
    /// Base Clase for low-discrepancy sequences.
    /// </summary>
    public abstract class QRNG: Random
    { 
        /// <summary>
        /// Base class for the partition of the low discrepancy sequence.
        /// </summary>
        public abstract class QRNGPartition:Random
        {
            /// <summary>
            /// Reset the Partition to the first number to be generated.
            /// </summary>
            public abstract void Reset();

            public abstract uint GetThreadID();
        }

        /// <summary>
        /// Reset the dimension number to the first dimension.
        /// </summary>
        public abstract void ResetDimension();

        /// <summary>
        /// Reset all partitions for the low-discrepancy sequence.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        ///  if partition doesn't exist for the specific low-discrepancy sequence (sequence not partitionable), it will throw a NotImplementedException.
        /// </summary>
        /// <returns>The partitions for the QRNG. </returns>
        public abstract QRNGPartition[] GetPartitions();
    }
}