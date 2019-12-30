using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Concurrent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CsQRNG.SobolSequence;
using CsQRNG;

namespace CsQRNGTest
{
    [TestClass]
    public class DirectionLoaderTest
    {
        [TestMethod]
        public void SerializeDeserializeTest()
        {
            DirectionVectorLoader dvl = new DirectionVectorLoader('6');
            int nb_d = 10;
            int i = 0;
            ConcurrentDictionary<int, Direction> directions1 = new ConcurrentDictionary<int, Direction>();
            while (dvl.MoveNext()&&i<nb_d)
            {
                Direction d = dvl.Current;
                directions1.TryAdd(i,d);
                i++;
            }
            dvl.Serialize();
            string number_dir = Configuration.Default.DirectionNumber;
            string filename = "dir-number-6";
            BinaryFormatter formatter = new BinaryFormatter();

            Stream number_reader = new FileStream(number_dir + "\\" + filename+".dat", FileMode.Open, FileAccess.ReadWrite);
            // Deserialize the file 
            ConcurrentDictionary<int, Direction> directions2 = (ConcurrentDictionary<int, Direction>)formatter.Deserialize(number_reader);

            for (int k = 0; k < nb_d; k++)
            {

                directions1.TryGetValue(k, out var value1);
                
                Console.Write(value1.D + " " + value1.S + " " + value1.A);
                foreach (uint t in value1.V)
                {
                    Console.Write(" " + t);
                }

                Console.WriteLine();
                
                directions2.TryGetValue(k, out var value2);
                Console.Write(value2.D + " " + value2.S + " " + value2.A);
                foreach (uint t in value2.V)
                {
                    Console.Write(" " + t);
                }
                Console.WriteLine();
                
                bool equals = value1.Equals(value1, value2);
                if (!equals)
                {
                    throw new AssertFailedException();
                }
                
            }
        }
    }
}
