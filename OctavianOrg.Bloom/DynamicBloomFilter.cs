using System;
using System.Collections.Generic;

namespace OctavianOrg.Bloom
{
    /// <summary>
    /// An implementation of a dynamic bloom filter as discussed in:
    /// "Theory and Network Applications of Dynamic Bloom Filters" by Guo, Wu, Chen and Luo.
    /// This filter is append-only.
    /// </summary>
    public class DynamicBloomFilter
    {
        private List<BitSet[]> _bitSets;
        private BitSet[] _currentBitSet;
        private long _currentCount;
        private HashProvider _hash;

        public DynamicBloomFilter(BloomFilterParameters parameters, HashProvider hashProvider)
        {
            Parameters = parameters;
            _hash = hashProvider;
            _bitSets = new List<BitSet[]>();
            CreateNewBitSet();
        }

        public BloomFilterParameters Parameters { get; private set; }

        public long Count { get; private set; }

        public double FalsePositiveRate
        {
            get
            {
                double countRatio = Math.Floor((double)Count / Parameters.MaxElementCount);
                double first = Math.Pow(1.0 - Math.Pow(1.0 - Math.Pow(1.0 / Math.E, (double)Parameters.MaxElementCount * Parameters.HashFunctionCount / Parameters.TotalBits), Parameters.HashFunctionCount), countRatio);
                double second = 1.0 - Math.Pow(1.0 - Math.Pow(1 / Math.E, Parameters.HashFunctionCount * (Count - Parameters.MaxElementCount * countRatio) / Parameters.TotalBits), Parameters.HashFunctionCount);
                return 1.0 - first * second;
            }
        }

        public void Add(string item)
        {
            long[] hashValues = new long[Parameters.HashFunctionCount];
            hashValues = _hash.Hash(item, hashValues);
            bool collision = true;

            for (int i = 0; i < Parameters.HashFunctionCount; ++i)
            {
                if (_currentBitSet[i].Set(hashValues[i] % Parameters.BitsPerHashFunction))
                    collision = false;
            }

            if (collision == false)
            {
                ++_currentCount;

                if (_currentCount >= Parameters.MaxElementCount)
                {
                    CreateNewBitSet();
                }

                ++Count;
            }
        }

        public bool Contains(string item)
        {
            long[] hashValues = new long[Parameters.HashFunctionCount];
            hashValues = _hash.Hash(item, hashValues);

            for (int i = 0; i < _bitSets.Count; ++i)
            {
                int matchCount = 0;

                for (int j = 0; j < Parameters.HashFunctionCount; ++j)
                {
                    if (_bitSets[i][j].IsSet(hashValues[j] % Parameters.BitsPerHashFunction))
                        ++matchCount;
                    else
                        break;
                }

                if (matchCount == Parameters.HashFunctionCount)
                    return true;
            }

            return false;
        }

        private void CreateNewBitSet()
        {
            _currentBitSet = new BitSet[Parameters.HashFunctionCount];

            for (int i = 0; i < Parameters.HashFunctionCount; ++i)
                _currentBitSet[i] = new BitSet(Parameters.BitsPerHashFunction);

            _bitSets.Add(_currentBitSet);
            _currentCount = 0;
        }
    }
}
