using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace OctavianOrg.Bloom
{
    public class DoubleHashProvider : HashProvider
    {
        private static readonly long MaxPrime = 3037000493;
        private int _hashFunctionCount;
        private int _bitCount;
        private long[] _a;
        private long[] _b;

        public DoubleHashProvider(BloomFilterParameters parameters)
        {
            _hashFunctionCount = parameters.HashFunctionCount;
            _bitCount = (int)parameters.BitsPerHashFunction;
            _a = new long[2];
            _b = new long[2];

            using (RandomNumberGenerator rand = RandomNumberGenerator.Create())
            {
                byte[] randBytes = new byte[8];

                for (int i = 0; i < 2; ++i)
                {
                    rand.GetBytes(randBytes);
                    long rand1 = BitConverter.ToInt64(randBytes, 0);
                    rand.GetBytes(randBytes);
                    long rand2 = BitConverter.ToInt64(randBytes, 0);
                    rand1 &= long.MaxValue;
                    rand2 &= long.MaxValue;
                    _a[i] = rand1 % MaxPrime;
                    _b[i] = rand2 % MaxPrime;
                    _a[i] = _a[i] == 0 ? 1 : _a[i];
                }
            }
        }

        public override long[] Hash(byte[] bytes, long[] hashResults)
        {
            return Hash(bytes, 0, bytes.Length, hashResults);
        }

        public override long[] Hash(byte[] bytes, int offset, int length, long[] hashResults)
        {
            // polynomial rolling hash function
            long prime = 257;
            long power = 1;
            long result = 0;
            int maxLen = offset + length;

            for (int i = offset; i < maxLen; i++)
            {
                result = (result + bytes[i] * power) % MaxPrime;
                power = power * prime % MaxPrime;
            }

            long a = (result * _a[0] + _b[0]) % MaxPrime;
            long b = (result * _a[1] + _b[1]) % MaxPrime;
            hashResults[0] = a % _bitCount;

            for (int i = 1; i < _hashFunctionCount; ++i)
            {
                a = (a + b) % _bitCount;
                b = (b + i) % _bitCount;
                hashResults[i] = a;
            }

            return hashResults;
        }

        public override long[] Hash(string s, long[] hashResults)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(s);
            return Hash(bytes, 0, bytes.Length, hashResults);
        }
    }
}
