using System;

namespace OctavianOrg.Bloom
{
    /// <summary>
    /// Implementation of an append-only bloom filter.
    /// </summary>
    public class BloomFilter
    {
        private BitSet[] _bitSets;
        private HashProvider _hash;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parameters">
        /// Initialization parameters for the filter. Must not be null.
        /// </param>
        /// <param name="hashProvider">
        /// HashProvider implementation for mapping keys to bit positions. Must not be null.
        /// </param>
        public BloomFilter(BloomFilterParameters parameters, HashProvider hashProvider)
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
        }

        /// <summary>
        /// Gets the parameters provided via the constructor.
        /// </summary>
        public BloomFilterParameters Parameters { get; private set; }

        /// <summary>
        /// Gets the number of keys stored in the filter.
        /// </summary>
        public long Count { get; private set; }

        /// <summary>
        /// Gets the current expected false positive rate based on the initialization parameters and number of keys in filter.
        /// </summary>
        public double FalsePositiveRate
        {
            get
            {
                return Math.Pow(1.0 - Math.Pow(1.0 / Math.E, (double)Count * Parameters.HashFunctionCount / Parameters.TotalBits), Parameters.HashFunctionCount);
            }
        }

        /// <summary>
        /// Adds a string key to the filter. Note that this method will allocate an array of long for storing
        /// the function hashes. To avoid this overhead use the method where an array can be provided as an argument. The count
        /// is always incremented when this method is called, even if there is a collision.
        /// </summary>
        /// <param name="item">
        /// The string key to add. Must not be null.
        /// </param>
        public void Add(string item)
        {
            if (item  == null) throw new ArgumentNullException("item");

            long[] hashValues = new long[Parameters.HashFunctionCount];
            Add(item, hashValues);
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
        public void Add(string item, long[] hashValues)
        {
            if (item == null) throw new ArgumentNullException("item");
            if (hashValues == null) throw new ArgumentNullException("hashValues");
            if (hashValues.Length < Parameters.HashFunctionCount) throw new ArgumentException("The hashValues parameter length must be at least Parameters.HashFunctionCount in length.");

            hashValues = _hash.Hash(item, hashValues);

            for (int i = 0; i < Parameters.HashFunctionCount; ++i)
            {
                _bitSets[i].Set(hashValues[i] % Parameters.BitsPerHashFunction);
            }

            ++Count;
        }

        public void Add(byte[] item)
        {
            if (item == null) throw new ArgumentNullException("item");

            long[] hashValues = new long[Parameters.HashFunctionCount];
            Add(item, hashValues);
        }

        public void Add(byte[] item, long[] hashValues)
        {
            if (item == null) throw new ArgumentNullException("item");
            if (hashValues == null) throw new ArgumentNullException("hashValues");
            if (hashValues.Length < Parameters.HashFunctionCount) throw new ArgumentException("The hashValues parameter length must be at least Parameters.HashFunctionCount in length.");

            hashValues = _hash.Hash(item, hashValues);

            for (int i = 0; i < Parameters.HashFunctionCount; ++i)
            {
                _bitSets[i].Set(hashValues[i] % Parameters.BitsPerHashFunction);
            }

            ++Count;
        }

        public bool Contains(string item)
        {
            long[] hashValues = new long[Parameters.HashFunctionCount];
            return Contains(item, hashValues);
        }

        public bool Contains(string item, long[] hashValues)
        {
            hashValues = _hash.Hash(item, hashValues);
            return Contains(hashValues);
        }

        public bool Contains(byte[] item)
        {
            long[] hashValues = new long[Parameters.HashFunctionCount];
            return Contains(item, hashValues);
        }

        public bool Contains(byte[] item, long[] hashValues)
        {
            hashValues = _hash.Hash(item, hashValues);
            return Contains(hashValues);
        }

        public bool Contains(long[] hashValues)
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
