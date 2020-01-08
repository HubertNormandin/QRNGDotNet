using System;
using System.Collections.Generic;
using System.Threading;
using MathNet.Numerics.Distributions;
using System.Reflection;

namespace QRNGDotNet
{

    //INTERFACE FOR PARALLEL QRNG
    public abstract class QRNG: Random
    { 
        public abstract class QRNGPartition:Random
        {
            public abstract void Reset();
        }
        //Reset the dimension number to the first dimension
        public abstract void ResetDimension();
        public abstract void Reset();
        public abstract QRNGPartition[] GetPartitions();
    }
}