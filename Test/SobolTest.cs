using System;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using CsQRNG;
using CsQRNG.SobolSequence;

namespace CsQRNGTest
{
    [TestClass]
    public class SobolTest
    {
        private uint d;
        private uint n;
        private double[][] reference;


        public void InitReference()
        {
            FileStream f = null;

            try
            {
                f = File.OpenRead("sobol_test.txt");
                StreamReader stream = new StreamReader(f);
                int k = 0;
                while (!stream.EndOfStream)
                {
                    string line = stream.ReadLine();
                    if (line != null)
                    {
                        string[] numbers = line.Split();
                        double[] value = new double[numbers.Length];
                        for (int i = 0; i < numbers.Length; i++)
                        {
                            value[i] = double.Parse(numbers[i]);
                        }

                        this.reference[k] = value;
                    }

                    k++;
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("sobol_test.txt does not exist");
            }

            finally
            {
                f?.Close();
            }

        }

        [TestInitialize]
        public void Initialize()
        {
            this.n = 1000;
            this.d = 10;
            this.reference = new double[this.n][];
            InitReference();

        }

        [TestMethod]
        public void SobolSequentialTest()
        {
            SequentialQRNGGenerator[] qrng = new SequentialQRNGGenerator[this.d];
            for (int i = 0; i < this.d; i++)
            {
                qrng[i] = new SequentialQRNGGenerator(new SobolDimension());
            }

            for (int i = 0; i < this.n; i++)
            {
                double[] value = new double[this.d];
                for (int j = 0; j < this.d; j++)
                {
                    value[j] = qrng[j].Next();
                    Assert.AreEqual(this.reference[i][j], value[j]);
                }
            }
        }

        [TestMethod]
        public void SobolPseudoParallelTest()
        {
            ParallelQRNGGenerator generator = new ParallelQRNGGenerator(typeof(SobolDimension), this.d, 8);
            for (int i = 0; i < this.n; i++)
            {
                double[] vector = generator.Next();
                for (int j = 0; j < vector.Length; j++)
                {
                    Console.Write(vector[j]+" ");
                    Assert.AreEqual(this.reference[i][j], vector[j]);
                }
                Console.WriteLine();
            }
        }
    }

    [TestClass]
    public class ParallelSobolTest
    {
        private uint n=1000;
        private uint d=10;
        private double[][] reference;
        private int n_task = -1;

        [TestInitialize]
        public void Initialize()
        {
            this.n = 1000;
            this.d = 10;
            this.reference = new double[this.n][];
            InitReference();
        }
        public void InitReference()
        {
            FileStream f = null;

            try
            {
                f = File.OpenRead("sobol_test.txt");
                StreamReader stream = new StreamReader(f);
                int k = 0;
                while (!stream.EndOfStream)
                {
                    string line = stream.ReadLine();
                    if (line != null)
                    {
                        string[] numbers = line.Split();
                        double[] val = new double[numbers.Length];
                        for (int i = 0; i < numbers.Length; i++)
                        {
                            val[i] = double.Parse(numbers[i]);
                        }

                        this.reference[k] = val;
                    }

                    k++;
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("sobol_test.txt does not exist");
            }

            finally
            {
                f?.Close();
            }

        }
        /*
        [TestMethod]
        public void SobolFixedParallelismThreadPoolTest()
        {
            
            this.done = new ManualResetEvent(false);
            
            var gen = new ParallelQRNGGenerator(typeof(SobolDimension), this.d, 8);
            ThreadPool.SetMaxThreads(8, 0);
            for (int i = 0; i < this.n; i++)
            {
                ThreadPool.QueueUserWorkItem(ThreadProc);
            }

            done.WaitOne();
            for (int i = 0; i <this.n; i++)
            {
                for (int j = 0; j < this.d; j++)
                {
                    //Assert.AreEqual(this.reference[i][j], value[i][j]);
                }
                Console.WriteLine();
            }

        }

        public void ThreadProc(object stateInfo)
        {
            
            if (Interlocked.Increment(ref this.n_task) == this.n)
            {
                this.done.Set();
                //this.value[this.n_task] = this.gen.Next();
            }
            else
            {
                Interlocked.Increment(ref this.n_task);
                Console.WriteLine(this.n_task);
            }
        }
        */
        [TestMethod]
        public void SobolParallelForTest()
        {
            ParallelOptions options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 16
            };

            var gen = new ParallelQRNGGenerator(typeof(SobolDimension), this.d, 16);
            double[][] value = new double[this.n][];
            object lock_value = new object();
            Parallel.For(0, (int)this.n, options, i =>
                {
                    value[i] = gen.Next();
                });

            for (int i = 0; i < this.n; i++)
            {
                for (int j = 0; j < this.d; j++)
                {
                    if(value[i] != null)
                        Console.Write(value[i][j]+" ");
                    else
                        Console.Write("null ");
                }
                Console.WriteLine();
            }
        }
    }
}
