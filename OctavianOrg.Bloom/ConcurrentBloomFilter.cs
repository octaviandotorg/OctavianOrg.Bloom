

using System;
using System.Threading;

namespace OctavianOrg.Bloom
{
    public class ConcurrentBloomFilter : PartitionedBloomFilter
    {
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
        public ConcurrentBloomFilter(BloomFilterParameters parameters, HashProvider hashProvider, int concurrencyLevel) : base(parameters, hashProvider, null)
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
            _bitSets = new ConcurrentBitSet[parameters.HashFunctionCount];

            for (int i = 0; i < parameters.HashFunctionCount; ++i)
                _bitSets[i] = new ConcurrentBitSet(parameters.BitsPerHashFunction, concurrencyLevel);
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

                if (_bitSets[i].Set(bitIndex))
                    collision = false;
            }

            if (collision == false)
            {
                Interlocked.Increment(ref _count);
                return true;
            }
            else
                return false;
        }


        /// <inheritdoc/>
        protected override bool ContainsInternal(long[] hashValues)
        {
            for (int i = 0; i < Parameters.HashFunctionCount; ++i)
            {
                long bitIndex = hashValues[i] % Parameters.BitsPerHashFunction;

                if (_bitSets[i].IsSet(bitIndex) == false)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the count if items in the filter. Note that this value is a bit fuzzy - if we add a value
        /// to the filter but it sets bits that are already set (a collision) then that doesn't really count
        /// since it doesn't impact the true false positive rate. This implementation only increments the item
        /// count if at least one bit is changed from 0 to 1 during addition. Also note that the count can be
        /// very misleading in a concurrent scenario where multiple bit set operations are interleaved.
        /// </summary>
        public override long Count
        {
            get { return Interlocked.Read(ref _count); }
        }
    }
}
