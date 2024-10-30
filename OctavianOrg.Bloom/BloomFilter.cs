

namespace OctavianOrg.Bloom
{
    public abstract class BloomFilter
    {
        protected BloomFilter() { }
        public abstract void Add(string item, long[] hashValues);
        public abstract void Add(byte[] item, long[] hashValues);
        public abstract void Add(long[] hashValues);
        public abstract bool Contains(string item, long[] hashValues);
        public abstract bool Contains(byte[] item, long[] hashValues);
        public abstract bool Contains(long[] hashValues);
        public abstract BloomFilterParameters Parameters { get; protected set; }
        public abstract long Count { get; }
        public abstract double FalsePositiveRate { get; }
    }
}