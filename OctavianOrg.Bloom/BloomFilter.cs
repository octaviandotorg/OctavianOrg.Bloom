

namespace OctavianOrg.Bloom
{
    public abstract class BloomFilter
    {
        protected BloomFilter() { }

        /// <summary>
        /// Adds a string key to the filter. If at least one bit is changed from 0 to 1 the <see cref="BloomFilter.Count"/> will be incremented.
        /// </summary>
        /// <param name="item">
        /// The string key to add. Must not be null.
        /// </param>
        /// <param name="hashValues">
        /// Array for storing function hashes. Must have a length at least equal to <see cref="BloomFilterParameters.HashFunctionCount"/>.
        /// </param>
        /// <returns>True if at least one bit was changed during addition, false otherwise.</returns>
        public abstract bool Add(string item, long[] hashValues);

        /// <summary>
        /// Adds a byte array key to the filter. If at least one bit is changed from 0 to 1 the <see cref="BloomFilter.Count"/> will be incremented.
        /// </summary>
        /// <param name="item">
        /// The byte array key to add. Must not be null.
        /// </param>
        /// <param name="hashValues">
        /// Array for storing function hashes. Must have a length at least equal to <see cref="BloomFilterParameters.HashFunctionCount"/>.
        /// </param>
        /// <returns>True if at least one bit was changed during addition, false otherwise.</returns>
        public abstract bool Add(byte[] item, long[] hashValues);

        /// <summary>
        /// Adds an array of hash values the filter. If at least one bit is changed from 0 to 1 the <see cref="BloomFilter.Count"/> will be incremented.
        /// </summary>
        /// <param name="hashValues">
        /// The hash values to add. Must not be null. The number of hashes must be equal to <see cref="BloomFilterParameters.HashFunctionCount"/>.
        /// </param>
        /// <returns>True if at least one bit was changed during addition, false otherwise.</returns>
        public abstract bool Add(long[] hashValues);

        /// <summary>
        /// Returns whether the supplied item is contained in the filter, which may be a false positive. False positives
        /// occur when two distinct items has to the same bit positions.
        /// </summary>
        /// <param name="item">
        /// The item to check.
        /// </param>
        /// <param name="hashValues">
        /// Array for storing function hashes. Must have a length at least equal to <see cref="BloomFilterParameters.HashFunctionCount"/>.
        /// </param>
        /// <returns>True if bits were set for item, false otherwise.</returns>
        public abstract bool Contains(string item, long[] hashValues);

        /// <summary>
        /// Returns whether the supplied item is contained in the filter, which may be a false positive. False positives
        /// occur when two distinct items has to the same bit positions.
        /// </summary>
        /// <param name="item">
        /// The item to check.
        /// </param>
        /// <param name="hashValues">
        /// Array for storing function hashes. Must have a length at least equal to <see cref="BloomFilterParameters.HashFunctionCount"/>.
        /// </param>
        /// <returns>True if bits were set for item, false otherwise.</returns>
        public abstract bool Contains(byte[] item, long[] hashValues);

        /// <summary>
        /// Returns whether the supplied item is contained in the filter, which may be a false positive. False positives
        /// occur when two distinct items has to the same bit positions.
        /// </summary>
        /// <param name="hashValues">
        /// The hash values to check. Must not be null. The number of hashes must be equal to <see cref="BloomFilterParameters.HashFunctionCount"/>.
        /// </param>
        /// <returns>True if bits were set for item, false otherwise.</returns>
        public abstract bool Contains(long[] hashValues);

        /// <summary>
        /// Gets the BloomFilterParameters supplied to the constructor.
        /// </summary>
        public abstract BloomFilterParameters Parameters { get; protected set; }

        /// <summary>
        /// Returns the count if items in the filter. Note that this value is a bit fuzzy - if we add a value
        /// to the filter but it sets bits that are already set (a collision) then that doesn't really count
        /// since it doesn't impact the true false positive rate. This implementation only increments the item
        /// count if at least one bit is changed from 0 to 1 during addition.
        /// </summary>
        public abstract long Count { get; }

        /// <summary>
        /// Returns the current expected false positive rate based on the capacity of the filter and <see cref="BloomFilter.Count"/>.
        /// </summary>
        public abstract double FalsePositiveRate { get; }
    }
}