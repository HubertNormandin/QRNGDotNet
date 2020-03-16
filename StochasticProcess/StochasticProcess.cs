using System;
using System.Collections.Generic;
using QRNGDotNet.Utilities;
using MathNet.Numerics.Distributions;


namespace QRNGDotNet.StochasticProcess
{
    /// <summary>
    /// 
    /// </summary>
    /// <member name="x0"></member>
    public abstract class StochasticProcessBase: ICloneable
    {
        protected double x0;
        protected int step;
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x0">The first value in the markov chain. </param>
        /// <param name="step">The number of step in the simulation. </param>
        protected StochasticProcessBase(double x0, int step)
        {
            this.x0 = x0;
            this.step = step;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x0">The first value in the markov chain. </param>
        /// <param name="step">The number of step in the simulation. </param>
        /// <param name="partitions">An array of QRNGPartition for the random variate of the stochastic processes. </param>
        protected StochasticProcessBase(double x0, int step,  QRNG.QRNGPartition[] partitions): this(x0, step)
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="partitions"></param>
        /// <returns></returns>
        public abstract StochasticProcessBase Clone(QRNG.QRNGPartition[] partitions);
        
            
       

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract double[] Next();


    }
}
