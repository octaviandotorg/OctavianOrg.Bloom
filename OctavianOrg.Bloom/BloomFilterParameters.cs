using System;

namespace OctavianOrg.Bloom
{
    public class BloomFilterParameters
    {
        public BloomFilterParameters(long maxElementCount, double falsePositiveRate)
        {
            this.MaxElementCount = maxElementCount;
            this.FalsePositiveRate = falsePositiveRate;
            double bitCount = Math.Ceiling((-Math.Log(this.FalsePositiveRate) * this.MaxElementCount) / (Math.Pow(Math.Log(2), 2)));
            this.HashFunctionCount = (int)((bitCount * Math.Log(2)) / this.MaxElementCount);
            this.BitsPerHashFunction = (long)(bitCount / this.HashFunctionCount);
            this.TotalBits = this.BitsPerHashFunction * this.HashFunctionCount;
        }

        public long MaxElementCount { get; private set; }

        public double FalsePositiveRate { get; private set; }

        public int HashFunctionCount { get; private set; }

        public long TotalBits { get; private set; }

        public long BitsPerHashFunction { get; private set; }
    }
}
