using System;
using System.Collections.Generic;



namespace QRNGDotNet.StochasticProcess
{
    class ArithmeticBrownianMotion
    {
        /// <summary>
        ///  A class that generate a geometric brownian motion. 
        /// </summary>
        public class GeometricBrownianMotion : BrownianMotion
        {

            /// <summary>
            /// Constructor for ArithmeticBrownianMotion.
            /// </summary>
            /// <param name="x0">The first value in the markov chain.</param>
            /// <param name="step">The number of step in the simulation.</param>
            /// <param name="drift">The length in time that the brownian motion take.</param>
            /// <param name="vol">The volatility term of the brownian motion.</param>
            /// <param name="t">The length in time that the brownian motion take.</param>
            /// <param name="partitions">The partition of a low-discrepancy sequences (QRNG.QRNGPartition)that will be used to generate the random variates. </param>
            public GeometricBrownianMotion(double x0, int step, double drift, double vol, double t, QRNG.QRNGPartition[] partitions) : base(x0, step, drift, vol, t, partitions){}

            /// <summary>
            /// Create a single path of an arithmetic brownian motion.
            /// </summary>
            /// <returns>An IEnumerable containing the values of the arithmetic brownian motion. </returns>
            public override double[] Next()
            {
                double[] X = new double[this.step];
                X[0] = x0;
                for (int i = 1; i < this.step; i++)
                {
                    X[i] = X[i-1] + (this.drift*this.h + this.vol * Math.Sqrt(this.h) * dz[i].Sample());
                    
                }
                return X;
            }
        }
    }
}
