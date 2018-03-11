using System;
using System.Collections.Generic;
using System.Reflection;

namespace Unity.Utility
{
    internal class ConstructorLengthComparer : IComparer<ConstructorInfo>
    {
        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="y">The second object to compare.</param>
        /// <param name="x">The first object to compare.</param>
        /// <returns>
        /// Value Condition Less than zero is less than y. Zero equals y. Greater than zero is greater than y.
        /// </returns>
        public int Compare(ConstructorInfo x, ConstructorInfo y)
        {
            return (y ?? throw new ArgumentNullException(nameof(y))).GetParameters().Length - (x ?? throw new ArgumentNullException(nameof(x))).GetParameters().Length;
        }
    }
}
