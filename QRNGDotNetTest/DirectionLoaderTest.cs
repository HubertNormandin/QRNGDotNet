using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QRNGDotNet.SobolSequence;
namespace QRNGDotNetTest
{
    [TestClass]
    public class DirectionLoaderTest
    {
        //This only print the test
        [TestMethod]
        public void DirectionTest()
        {
            DirectionLoader.DirectionNumberLoader dnl = DirectionLoader.GetDirectionNumberLoader("6");
            for (int i = 0; i < 10; i++)
            {
                Direction dir = dnl.Next();
                Console.Write(dir.d + " " + dir.s + " " + dir.a + " ");

                for (int k = 0; k < dir.v.Length; k++)
                {
                    Console.Write(dir.v[k] + " ");
                }
                Console.WriteLine();
            }
        }
    }
}