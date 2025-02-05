namespace OctavianOrg.Bloom
{
    public class BitSet
    {
        private long[] _set;

        public BitSet(long bitCount)
        {
            BitCount = bitCount;
            long itemsNeeded = bitCount >> 6;
            _set = new long[itemsNeeded + 1];
        }

        public long BitCount { get; private set; }

        /// <summary>
        /// Sets the specified bit and returns whether the bit was changed.
        /// </summary>
        /// <param name="index">The index of the bit to set.</param>
        /// <returns>True if the bit was 0 then set to 1, false otherwise.</returns>
        public bool Set(long index)
        {
            long item = index >> 6;
            int shift = (int)(index % 64);
            long setMask = 1L << shift;

            if ((_set[item] & setMask) == 0)                          
            {
                _set[item] |= setMask;
                return true;
            }
            else
                return false;
        }

        public bool IsSet(long index)
        {
            long item = index >> 6;
            int shift = (int)(index % 64);
            return (_set[item] & 1L << shift) != 0;
        }
    }
}
