using System;
using System.Security.Cryptography;

namespace OctavianOrg.Bloom
{
    public class DoubleHashProvider : HashProvider
    {
        private static readonly long MaxPrime = 3037000493;
        private int _hashFunctionCount;
        private long[] _a;
        private long[] _b;

        public DoubleHashProvider(BloomFilterParameters parameters)
        {
            _hashFunctionCount = parameters.HashFunctionCount;
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
            long result = 0;
            int maxLen = offset + length;

            for (int i = offset; i < maxLen; i++)
            {
                result = (result + bytes[i] * power) % MaxPrime;
                power = power * prime % MaxPrime;
            }

            long hashValue1 = UniqueHashA(result);
            long hashValue2 = UniqueHashB(result);

            for (int i = 0; i < _hashFunctionCount; ++i)
            {
                hashResults[i] = CombinedHash(hashValue1, hashValue2, i);
            }

            return hashResults;
        }

        public override long[] Hash(string s, long[] hashResults)
        {
            // polynomial rolling hash function
            long prime = 65537;
            long power = 1;
            long result = 0;

            foreach (char c in s)
            {
                result = (result + c * power) % MaxPrime;
                power = power * prime % MaxPrime;
            }

            long hashValue1 = UniqueHashA(result);
            long hashValue2 = UniqueHashB(result);

            for (int i = 0; i < _hashFunctionCount; ++i)
            {
                hashResults[i] = CombinedHash(hashValue1, hashValue2, i);
            }

            return hashResults;
        }

        private long UniqueHashA(long l)
        {
            return (_a[0] * l + _b[0]) % MaxPrime;
        }

        private long UniqueHashB(long l)
        {
            return (_a[1] * l + _b[1]) % MaxPrime;
        }

        private long CombinedHash(long hashValue1, long hashValue2, int functionIndex)
        {
            return (hashValue1 + functionIndex * hashValue2) % MaxPrime;
        }
    }
}
