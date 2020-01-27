using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QRNGDotNet.SobolSequence;
namespace QRNGDotNetTest
{
    [TestClass]
    public class DirectionLoaderTest
    {
        //This only print the test used to compare with another implementation when wrong number are generated
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

        [TestMethod]
        public void InvalidCriteriaTest()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                //instantiate a DirectionLoader where the criteria parameter
                    () => DirectionLoader.GetDirectionNumberLoader("8")
                );
        }

        private void GetAllDirections(DirectionLoader.DirectionNumberLoader dnl)
        {
            for (int i = 0; i < 21202; i++)
            {
                dnl.Next();
            }
        }

        //Test wether DirectionLoader throw an error when DirectionLoader arrive at the end of the file
        [TestMethod]
        public void NoMoreDirectionTest()
        {
            DirectionLoader.DirectionNumberLoader dnl = DirectionLoader.GetDirectionNumberLoader("5");
            //Test wether DirectionLoader throw an error when it arrive at the endofstream
            Assert.ThrowsException<ArgumentOutOfRangeException>(

                () => GetAllDirections(dnl)
                ) ;
        }
    }
}