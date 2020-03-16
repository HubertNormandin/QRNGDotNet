using System;
using System.Collections.Generic;
using MathNet.Numerics.Distributions;

namespace QRNGDotNet.StochasticProcess
{
    /// <summary>
    /// The Base Class for the brownian motion
    /// </summary>
    public abstract class BrownianMotion: StochasticProcessBase
    {
        /// <summary>
        /// The drift of the geometric brownian motion.
        /// </summary>
        protected double drift;

        /// <summary>
        /// The volatility of the geometric brownian motion.
        /// </summary>
        protected double vol;

        /// <summary>
        /// The length in time that the brownian motion take.
        /// </summary>
        protected double t;

        /// <summary>
        /// The change in time for each step in the simulation
        /// </summary>
        protected double h;

        /// <summary>
        /// A normal distribution use to get the random Variate.
        /// </summary>
        protected Normal[] dz;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x0">The first value in the markov chain.</param>
        /// <param name="step">The number of step in the simulation.</param>
        /// <param name="drift">The length in time that the brownian motion take.</param>
        /// <param name="vol">The volatility term of the brownian motion.</param>
        /// <param name="t">The length in time that the brownian motion take.</param>
        public BrownianMotion(double x0, int step, double drift, double vol, double t) : base(x0, step)
        {
            this.drift = drift;
            this.vol = vol;
            this.t = t;
            this.h = t / step;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x0">The first value in the markov chain.</param>
        /// <param name="step">The number of step in the simulation.</param>
        /// <param name="drift">The length in time that the brownian motion take.</param>
        /// <param name="vol">The volatility term of the brownian motion.</param>
        /// <param name="t">The length in time that the brownian motion take.</param>
        /// <param name="partitions">The partition of a low-discrepancy sequences (QRNG.QRNGPartition)that will be used to generate the random variates. </param>
        public BrownianMotion(double x0, int step, double drift, double vol, double t, QRNG.QRNGPartition[] partitions) : this(x0, step, drift, vol, t)
        {
            this.drift = drift;
            this.vol = vol;
            this.t = t;
            this.dz = new Normal[this.step];
            for(int i = 0; i<this.step; i++)
            {
                this.dz[i] = new Normal(randomSource:partitions[i]);
            }
        }

        /// <summary>
        /// Clone the brownian into another identical BrownianMotion with the random variate generated from partitions.
        /// </summary>
        /// <returns>The new BrownianMotion with Random variate partitions. </returns>
        public override StochasticProcessBase Clone(QRNG.QRNGPartition[] partitions)
        {
            return new GeometricBrownianMotion(this.x0, this.step, this.drift, this.vol, this.t, partitions);
        }
    }
}
