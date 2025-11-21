namespace OctavianOrg.Bloom
{
    /// <summary>
    /// A bit set implementation for a fixed number of bits.
    /// </summary>
    public class BitSet : IBitSet
    {
        protected long[] _set;

        public BitSet(long bitCount)
        {
            long itemsNeeded = bitCount >> 6;
            _set = new long[itemsNeeded + 1];
        }

        /// <inheritdoc/>
        public bool Set(long bitNumber)
        {
            long arrOffset = bitNumber >> 6;
            int shift = (int)(bitNumber % 64);
            long setMask = 1L << shift;

            if ((_set[arrOffset] & setMask) == 0)                          
            {
                _set[arrOffset] |= setMask;
                return true;
            }
            else
                return false;
        }

        /// <inheritdoc/>
        public bool IsSet(long bitNumber)
        {
            long arrOffset = bitNumber >> 6;
            int shift = (int)(bitNumber % 64);
            return (_set[arrOffset] & 1L << shift) != 0;
        }
    }
}
