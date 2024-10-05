namespace OctavianOrg.Bloom
{
    public abstract class HashProvider
    {
        protected HashProvider() { }

        public abstract long[] Hash(string s, long[] hashResults);

        public abstract long[] Hash(byte[] bytes, long[] hashResults);

        public abstract long[] Hash(byte[] bytes, int offset, int length, long[] hashResults);
    }
}