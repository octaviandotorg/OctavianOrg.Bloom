using System;

namespace OctavianOrg.Bloom
{
    /// <summary>
    /// A thread-safe bit set implementation for a fixed number of bits.
    /// </summary>
    public class ConcurrentBitSet : IBitSet
    {
        private object[] _locks;
        private long[] _set;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="bitCount">
        /// The number of bits to allocate.
        /// </param>
        /// <param name="concurrencyLevel">
        /// Sets the number of locks to use for concurrency control. If set to -1 the level will be set based on the number of processors.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if concurrencyLevel is <= 0 and not -1.
        /// </exception>
        public ConcurrentBitSet(long bitCount, int concurrencyLevel)
        {
            long itemsNeeded = bitCount >> 6;
            _set = new long[itemsNeeded + 1];

            if (concurrencyLevel <= 0)
            {
                if (concurrencyLevel != -1)
                    throw new ArgumentOutOfRangeException(nameof(concurrencyLevel), "Concurrency level must be greater than 0 or -1 for default.");

                concurrencyLevel = Environment.ProcessorCount;
            }

            _locks = new object[concurrencyLevel];

            for (int i = 0; i < _locks.Length; ++i)
                _locks[i] = new object();
        }

        /// </inheritdoc>
        public bool Set(long bitNumber)
        {
            long arrOffset = bitNumber >> 6;
            int shift = (int)(bitNumber % 64);
            long setMask = 1L << shift;

            lock (_locks[arrOffset % _locks.Length])
            {
                if ((_set[arrOffset] & setMask) == 0)
                {
                    _set[arrOffset] |= setMask;
                    return true;
                }
                else
                    return false;
            }
        }

        /// </inheritdoc>
        public bool IsSet(long bitNumber)
        {
            long arrOffset = bitNumber >> 6;
            int shift = (int)(bitNumber % 64);

            lock (_locks[arrOffset % _locks.Length])
            {
                return (_set[arrOffset] & 1L << shift) != 0;
            }
        }
    }
}
