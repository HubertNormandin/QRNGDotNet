using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QRNGDotNet;

namespace QRNGDotNetTest
{
    [TestClass]
    public class CircularLinkedListTest
    {

        [TestMethod]
        public void MoveNextTest()
        {
            int[] arr = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
            CircularLinkedList<int> list = new CircularLinkedList<int>();

            foreach (int a in arr)
            {
                list.Add(a);
            }
            IEnumerator enumerator = list.GetEnumerator();

            for (int i = 0; i < arr.Length * 2; i++)
            {
                enumerator.MoveNext();

                if (i >= arr.Length)
                    Assert.AreEqual(arr[i - arr.Length], (int)enumerator.Current);
                else
                    Assert.AreEqual(arr[i], (int)enumerator.Current);

            }
        }
    }
}
