

using System;
using System.Threading;

namespace OctavianOrg.Bloom
{
    public class ConcurrentBloomFilter : BasicBloomFilter
    {
        private object[,] _locks;

        /// <summary>
        /// Creates a concurrent append-only BloomFilter.
        /// </summary>
        /// <param name="parameters">
        /// The BloomFilterParameters for the filter.
        /// </param>
        /// <param name="hashProvider">
        /// The hash provider for the filter.
        /// </param>
        /// <param name="concurrencyLevel">
        /// The concurrency level for the filter. If set to -1 the level will be set based on the number of processors.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if concurrencyLevel is <= 0 and not -1.
        /// </exception>
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

        /// <summary>
        /// Gets the concurrency level set for the filter.
        /// </summary>
        public int ConcurrencyLevel { get; private set; }

        protected override bool AddInternal(long[] hashValues)
        {
            bool collision = true;

            for (int i = 0; i < Parameters.HashFunctionCount; ++i)
            {
                long bitIndex = hashValues[i] % Parameters.BitsPerHashFunction;
                object chunkLock = _locks[i, (bitIndex >> 6) % ConcurrencyLevel];

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
                object chunkLock = _locks[i, (bitIndex >> 6) % ConcurrencyLevel];

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

        /// <inheritdoc/>
        public override long Count
        {
            get { return Interlocked.Read(ref _count); }
        }
    }
}
