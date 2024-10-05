using System;
using System.Security.Cryptography;

namespace OctavianOrg.Bloom
{
    public class BasicHashProvider : HashProvider
    {
        private static readonly long MaxPrime = 3037000493;
        private int _hashFunctionCount;
        private long[] _a;
        private long[] _b;

        public BasicHashProvider(BloomFilterParameters parameters)
        {
            _hashFunctionCount = parameters.HashFunctionCount;
            _a = new long[parameters.HashFunctionCount];
            _b = new long[parameters.HashFunctionCount];

            using (RandomNumberGenerator rand = RandomNumberGenerator.Create())
            {
                byte[] randBytes = new byte[8];

                for (int i = 0; i < parameters.HashFunctionCount; ++i)
                {
                    rand.GetBytes(randBytes);
                    long rand1 = BitConverter.ToInt64(randBytes, 0);
                    rand.GetBytes(randBytes);
                    long rand2 = BitConverter.ToInt64(randBytes, 0);
                    rand1 = rand1 < 0 ? rand1 ^ 1L << 63 : rand1;
                    rand2 = rand2 < 0 ? rand2 ^ 1L << 63 : rand2;
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
            long hashValue = 0;
            int maxLen = offset + length;

            for (int i = offset; i < maxLen; i++)
            {
                hashValue = (hashValue + bytes[i] * power) % MaxPrime;
                power = power * prime % MaxPrime;
            }

            for (int i = 0; i < _hashFunctionCount; ++i)
            {
                hashResults[i] = UniqueHash(hashValue, i);
            }

            return hashResults;
        }

        public override long[] Hash(string s, long[] hashResults)
        {
            // polynomial rolling hash function
            long prime = 65537;
            long power = 1;
            long hashValue = 0;

            foreach (char c in s)
            {
                hashValue = (hashValue + c * power) % MaxPrime;
                power = power * prime % MaxPrime;
            }

            for (int i = 0; i < _hashFunctionCount; ++i)
            {
                hashResults[i] = UniqueHash(hashValue, i);
            }

            return hashResults;
        }

        private long UniqueHash(long l, int functionIndex)
        {
            return (_a[functionIndex] * l + _b[functionIndex]) % MaxPrime;
        }
    }
}
