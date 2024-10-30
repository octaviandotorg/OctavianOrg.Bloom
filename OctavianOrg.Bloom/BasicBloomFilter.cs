using System;

namespace OctavianOrg.Bloom
{
    /// <summary>
    /// Implementation of an append-only bloom filter.
    /// </summary>
    public class BasicBloomFilter : BloomFilter
    {
        protected BitSet[] _bitSets;
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
        public BasicBloomFilter(BloomFilterParameters parameters, HashProvider hashProvider)
        {
            if (parameters == null) throw new ArgumentNullException("parameters");

            if (hashProvider == null) throw new ArgumentNullException("hashProvider");

            Parameters = parameters;
            _hash = hashProvider;
            _bitSets = new BitSet[Parameters.HashFunctionCount];

            for (int i = 0; i < Parameters.HashFunctionCount; ++i)
            {
                _bitSets[i] = new BitSet(Parameters.BitsPerHashFunction);
            }

            _count = 0;
        }

        /// <summary>
        /// Gets the parameters provided via the constructor.
        /// </summary>
        public override BloomFilterParameters Parameters { get; protected set; }

        /// <summary>
        /// Gets the number of keys stored in the filter.
        /// </summary>
        public override long Count { get { return _count; } }

        /// <summary>
        /// Gets the current expected false positive rate based on the initialization parameters and number of keys in filter.
        /// </summary>
        public override double FalsePositiveRate
        {
            get
            {
                return Math.Pow(1.0 - Math.Pow(1.0 / Math.E, (double)Count * Parameters.HashFunctionCount / Parameters.TotalBits), Parameters.HashFunctionCount);
            }
        }

        /// <summary>
        /// Adds a string key to the filter. The count is always incremented when this method is called, even if there is a collision.
        /// </summary>
        /// <param name="item">
        /// The string key to add. Must not be null.
        /// </param>
        /// <param name="hahValues">
        /// Array for storing function hashes. Must have a length at least equal to <see cref="Parameters.HashFunctionCount"/>.
        /// </param>
        public override void Add(string item, long[] hashValues)
        {
            if (item == null) throw new ArgumentNullException("item");
            if (hashValues == null) throw new ArgumentNullException("hashValues");
            if (hashValues.Length < Parameters.HashFunctionCount) throw new ArgumentException("The hashValues parameter length must be at least Parameters.HashFunctionCount in length.");

            hashValues = _hash.Hash(item, hashValues);
            AddInternal(hashValues);
        }

        public override void Add(byte[] item, long[] hashValues)
        {
            if (item == null) throw new ArgumentNullException("item");
            if (hashValues == null) throw new ArgumentNullException("hashValues");
            if (hashValues.Length < Parameters.HashFunctionCount) throw new ArgumentException("The hashValues parameter length must be at least Parameters.HashFunctionCount in length.");

            hashValues = _hash.Hash(item, hashValues);
            AddInternal(hashValues);
        }

        public override void Add(long[] hashValues)
        {
            AddInternal(hashValues);
        }

        protected virtual void AddInternal(long[] hashValues)
        {
            for (int i = 0; i < Parameters.HashFunctionCount; ++i)
            {
                _bitSets[i].Set(hashValues[i] % Parameters.BitsPerHashFunction);
            }

            ++_count;
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
                if (_bitSets[i].IsSet(hashValues[i] % Parameters.BitsPerHashFunction) == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
