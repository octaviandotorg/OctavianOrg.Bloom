using System;

namespace OctavianOrg.Bloom
{
    /// <summary>
    /// Implementation of an append-only partitioned bloom filter. See "A Case for Partitioned Bloom Filters" by Paulo Sérgio Almeida et al. for details.
    /// https://arxiv.org/abs/2009.11789
    /// </summary>
    public class PartitionedBloomFilter : BloomFilter 
    {
        protected IBitSet[] _bitSets;
        protected HashProvider _hash;
        protected long _count;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parameters">
        /// Initialization parameters for the filter. Must not be null.
        /// </param>
        /// <param name="hashProvider">
        /// HashProvider implementation for mapping keys to bit positions. Must not be null.
        /// </param>
        public PartitionedBloomFilter(BloomFilterParameters parameters, HashProvider hashProvider)
        {
            if (parameters == null) throw new ArgumentNullException("parameters");

            if (hashProvider == null) throw new ArgumentNullException("hashProvider");

            Parameters = parameters;
            _hash = hashProvider;
            _count = 0;
            _bitSets = new BitSet[parameters.HashFunctionCount];

            for (int i = 0; i < parameters.HashFunctionCount; ++i)
                _bitSets[i] = new BitSet(parameters.BitsPerHashFunction);
        }

        protected PartitionedBloomFilter(BloomFilterParameters parameters, HashProvider hashProvider, IBitSet[] bitSets)
        {
            if (parameters == null) throw new ArgumentNullException("parameters");

            if (hashProvider == null) throw new ArgumentNullException("hashProvider");

            Parameters = parameters;
            _hash = hashProvider;
            _count = 0;
            _bitSets = bitSets;
        }

        /// <summary>
        /// Gets the parameters provided via the constructor.
        /// </summary>
        public override BloomFilterParameters Parameters { get; protected set; }

        /// <inheritdoc/>
        public override long Count { get { return _count; } }

        /// <inheritdoc/>
        public override double FalsePositiveRate
        {
            get
            {
                return Math.Pow(1.0 - Math.Pow(1.0 - ((double)Parameters.HashFunctionCount / Parameters.TotalBits), Count), Parameters.HashFunctionCount);
            }
        }

        /// <inheritdoc/>
        public override bool Add(string item, long[] hashValues)
        {
            if (item == null) throw new ArgumentNullException("item");
            if (hashValues == null) throw new ArgumentNullException("hashValues");
            if (hashValues.Length < Parameters.HashFunctionCount) throw new ArgumentException("The hashValues parameter length must be at least Parameters.HashFunctionCount in length.");

            hashValues = _hash.Hash(item, hashValues);
            return AddInternal(hashValues);
        }

        /// <inheritdoc/>
        public override bool Add(byte[] item, long[] hashValues)
        {
            if (item == null) throw new ArgumentNullException("item");
            if (hashValues == null) throw new ArgumentNullException("hashValues");
            if (hashValues.Length < Parameters.HashFunctionCount) throw new ArgumentException("The hashValues parameter length must be at least Parameters.HashFunctionCount in length.");

            hashValues = _hash.Hash(item, hashValues);
            return AddInternal(hashValues);
        }

        /// <inheritdoc/>
        public override bool Add(long[] hashValues)
        {
            return AddInternal(hashValues);
        }

        protected virtual bool AddInternal(long[] hashValues)
        {
            bool collision = true;

            for (int i = 0; i < Parameters.HashFunctionCount; ++i)
            {
                if (_bitSets[i].Set(hashValues[i]))
                    collision = false;
            }

            if (collision == false)
            {
                ++_count;
                return true;
            }
            else
                return false;
        }

        public override bool Contains(string item, long[] hashValues)
        {
            hashValues = _hash.Hash(item, hashValues);
            return ContainsInternal(hashValues);
        }

        public override bool Contains(byte[] item, long[] hashValues)
        {
            hashValues = _hash.Hash(item, hashValues);
            return ContainsInternal(hashValues);
        }

        public override bool Contains(long[] hashValues)
        {
            return ContainsInternal(hashValues);
        }

        protected virtual bool ContainsInternal(long[] hashValues)
        {
            for (int i = 0; i < Parameters.HashFunctionCount; ++i)
            {
                if (_bitSets[i].IsSet(hashValues[i]) == false)
                    return false;
            }

            return true;
        }
    }
}
