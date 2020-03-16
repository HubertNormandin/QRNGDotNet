using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace QRNGDotNet.Utilities
{
    /// <summary>
    /// Lock-free basic mathematical operation like summation and product.
    /// </summary>
    public static class ParallelOperation
    {
        /// <summary>
        ///  Compute summation of 2 number sum and value concurrently using lock free synchronization.
        /// </summary>
        /// <param name="sum"> A reference to a double that will contain the result of the sum. </param>
        /// <param name="value"> The value that will be added to the sum. </param>
        /// <returns></returns>
        public static double Add(ref double sum, double value)
        {
            double newCurrentValue = sum; // non-volatile read, so may be stale
            while (true)
            {
                double currentValue = newCurrentValue;
                double newValue = currentValue + value;
                newCurrentValue = Interlocked.CompareExchange(ref sum, newValue, currentValue);
                if (newCurrentValue == currentValue)
                    return newValue;
            }
        }

        /// <summary>
        ///  Compute summation of 2 number sum and value concurrently using lock free synchronization.
        /// </summary>
        /// <param name="product"> A reference to a double that will contain the result of the product. </param>
        /// <param name="value"> The value that will be added to the sum. </param>
        /// <returns></returns>
        public static double Product(ref double product, double value)
        {
            double newCurrentValue = product; // non-volatile read, so may be stale
            while (true)
            {
                double currentValue = newCurrentValue;
                double newValue = currentValue * value;
                newCurrentValue = Interlocked.CompareExchange(ref product, newValue, currentValue);
                if (newCurrentValue == currentValue)
                    return newValue;
            }
        }
    }

}
