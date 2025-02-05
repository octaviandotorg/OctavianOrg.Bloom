

using System;
using System.Threading;

namespace OctavianOrg.Bloom
{
    public class ConcurrentBloomFilter : BasicBloomFilter
    {
        private object[,] _locks;

        public ConcurrentBloomFilter(BloomFilterParameters parameters, HashProvider hashProvider, int concurrencyLevel) : base(parameters, hashProvider)
        {
            if (concurrencyLevel <= 0)
            {
                if (concurrencyLevel != -1)
                {
                    throw new ArgumentOutOfRangeException(nameof(concurrencyLevel), "Concurrency level must be greater than 0 or -1 for default.");
                }

                concurrencyLevel = Environment.ProcessorCount;
            }

            ConcurrencyLevel = concurrencyLevel;

            _locks = new object[Parameters.HashFunctionCount, concurrencyLevel];
            
            for (int i = 0; i < _locks.GetLength(0); ++i)
            {
                for (int j = 0; j < _locks.GetLength(1); ++j)
                {
                    _locks[i, j] = new object();
                }
            }
        }

        public int ConcurrencyLevel { get; private set; }

        protected override bool AddInternal(long[] hashValues)
        {
            bool collision = true;

            for (int i = 0; i < Parameters.HashFunctionCount; ++i)
            {
                long bitIndex = hashValues[i] % Parameters.BitsPerHashFunction;
                object chunkLock = _locks[i, bitIndex % ConcurrencyLevel];

                lock (chunkLock)
                {
                    if (_bitSets[i].Set(bitIndex))
                        collision = false;
                }
            }

            if (collision == false)
            {
                Interlocked.Increment(ref _count);
                return true;
            }
            else
                return false;
        }

        protected override bool ContainsInternal(long[] hashValues)
        {
            for (int i = 0; i < Parameters.HashFunctionCount; ++i)
            {
                long bitIndex = hashValues[i] % Parameters.BitsPerHashFunction;
                object chunkLock = _locks[i, bitIndex % ConcurrencyLevel];

                lock (chunkLock)
                {
                    if (_bitSets[i].IsSet(bitIndex) == false)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
