using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using QRNGDotNet;
using QRNGDotNet.SobolSequence;
using QRNGDotNet.StochasticProcess;
using QRNGDotNet.Utilities;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;

namespace AsianOption
{
    class OptionMethod
    {
        
    }
    class AsianOption
    {
        public struct AsianOptionParameter
        {
            public double s;
            public double k;
            public double ttm;
            public int N;
            public double h;
            public double rf;
            public double vol;
            public double div;
        }

        

        private object gen_lock = new object();
        private AsianOptionParameter param;
        private ContinuousMarkovChainGenerator mc;

        public AsianOption(AsianOptionParameter param, int MaxDegreeOfParallelism)
        {

            this.param = param;
  
            //SET THE DRIFT OF THE PROCESS
            double drift = (param.rf - param.div - 0.5 * param.vol * param.vol);
            // the parameter of the Sequence
            object[] par = { "6" };
            // set the stochastic process
            this.mc = new ContinuousMarkovChainGenerator(typeof(SobolSequence), new GeometricBrownianMotion(param.s, param.N, drift, param.vol, param.ttm), param.N, MaxDegreeOfParallelism, par);
        }

        public delegate double Payoff(AsianOptionParameter param, double[] markov_chain);
        public static double CallPricePayoff(AsianOptionParameter param, double[] markov_chain)
        {
            
            return Math.Exp(-param.rf * param.ttm) * Math.Max(ArrayStatistics.Mean(markov_chain) - param.k, 0);
        }

        public static double CallStrikePayoff(AsianOptionParameter param, double[] markov_chain)
        {

            return Math.Exp(-param.rf * param.ttm) * Math.Max(ArrayStatistics.Mean(markov_chain) - markov_chain[markov_chain.Length], 0);
        }

        public static double PutPricePayoff(AsianOptionParameter param, double[] markov_chain)
        {
            return Math.Exp(-param.rf * param.ttm) * Math.Max(param.k - ArrayStatistics.Mean(markov_chain), 0);
        }
        public static double PutStrikePayoff(AsianOptionParameter param, double[] markov_chain)
        {

            return Math.Exp(-param.rf * param.ttm) * Math.Max(markov_chain[markov_chain.Length]-ArrayStatistics.Mean(markov_chain), 0);
        }

        public double Run(int iter, Payoff payoff)
        {
            int it = iter;
            ParallelOptions parallel_options = new ParallelOptions { MaxDegreeOfParallelism = 16 };
            double sum = 0.0 ;

            
            Parallel.For(0, iter, parallel_options, (i) =>
            {
               
                ParallelOperation.Add(ref sum, payoff(this.param, this.mc.Next()));


            });

            return sum/iter;
        }
    }

    class Program
    {

        public static void Main(string[] args)
        {
            double s = 100;
            double k = 100;
            double ttm = 1;
            int N = 12;
            double h = (double)ttm / N;
            double rf = 0.03;
            double vol = 0.3;
            double div = 0;
            // TO ENSURE EQUIDISTRIBUTION OF THE RANDOM VARIABLE, THE NUMBER OF POINT NEED TO BE 2^N-1
            int nb_path = 16383;
            AsianOption.AsianOptionParameter param = new AsianOption.AsianOptionParameter { s = s, k = k, ttm = ttm, N = N, h = h, rf = rf, vol = vol, div = div };
            AsianOption option = new AsianOption(param, 16);
            AsianOption.Payoff payoff = new AsianOption.Payoff(AsianOption.CallPricePayoff);
            Console.WriteLine("option value: " + option.Run(nb_path, payoff));
            Console.WriteLine();
        }
    }
}
