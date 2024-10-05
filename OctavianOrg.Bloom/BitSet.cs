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

        public void Set(long index)
        {
            long item = index >> 6;
            int shift = (int)(index % 64);
            _set[item] |= 1L << shift;
        }

        public bool IsSet(long index)
        {
            long item = index >> 6;
            int shift = (int)(index % 64);
            return (_set[item] & 1L << shift) != 0;
        }
    }
}
