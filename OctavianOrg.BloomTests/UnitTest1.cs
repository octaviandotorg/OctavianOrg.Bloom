using OctavianOrg.Bloom;
using System.Text;

namespace OctavianOrg.BloomTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        [DeploymentItem("data1.txt")]
        [DeploymentItem("data2.txt")]
        public void TestBasicHashProvider()
        {
            BloomFilterParameters bloomFilterParameters = new BloomFilterParameters(320344, 0.02);
            BasicBloomFilter bloomFilter = new BasicBloomFilter(bloomFilterParameters, new BasicHashProvider(bloomFilterParameters));
            long[] hashValues = new long[bloomFilterParameters.HashFunctionCount];
            BloomTestRun(bloomFilter, true, hashValues);
        }

        [TestMethod]
        [DeploymentItem("data1.txt")]
        [DeploymentItem("data2.txt")]
        public void TestDoubleHashProvider()
        {
            BloomFilterParameters bloomFilterParameters = new BloomFilterParameters(320344, 0.02);
            BasicBloomFilter bloomFilter = new BasicBloomFilter(bloomFilterParameters, new DoubleHashProvider(bloomFilterParameters));
            long[] hashValues = new long[bloomFilterParameters.HashFunctionCount];
            BloomTestRun(bloomFilter, true, hashValues);
        }

        [TestMethod]
        [DeploymentItem("data1.txt")]
        [DeploymentItem("data2.txt")]
        public void TestBasicHashProviderAddAsBytes()
        {
            BloomFilterParameters bloomFilterParameters = new BloomFilterParameters(320344, 0.02);
            BasicBloomFilter bloomFilter = new BasicBloomFilter(bloomFilterParameters, new BasicHashProvider(bloomFilterParameters));
            long[] hashValues = new long[bloomFilterParameters.HashFunctionCount];
            BloomTestRun(bloomFilter, false, hashValues);
        }

        [TestMethod]
        [DeploymentItem("data1.txt")]
        [DeploymentItem("data2.txt")]
        public void TestDoubleHashProviderAddAsBytes()
        {
            BloomFilterParameters bloomFilterParameters = new BloomFilterParameters(320344, 0.02);
            BasicBloomFilter bloomFilter = new BasicBloomFilter(bloomFilterParameters, new DoubleHashProvider(bloomFilterParameters));
            long[] hashValues = new long[bloomFilterParameters.HashFunctionCount];
            BloomTestRun(bloomFilter, false, hashValues);
        }

        [TestMethod]
        [DeploymentItem("data1.txt")]
        [DeploymentItem("data2.txt")]
        [DeploymentItem("data3.txt")]
        public void DynamicTestBasicHashProvider()
        {
            BloomFilterParameters bloomFilterParameters = new BloomFilterParameters(320344, 0.02);
            DynamicBloomFilter bloomFilter = new DynamicBloomFilter(bloomFilterParameters, new DoubleHashProvider(bloomFilterParameters));
            DynamicBloomTestRun(bloomFilter);
        }

        private void BloomTestRun(BloomFilter bloomFilter, bool addAsString, long[] hashValues)
        {
            using (StreamReader reader = new StreamReader("data1.txt"))
            {
                string? line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (addAsString)
                    {
                        bloomFilter.Add(line, hashValues);
                    }
                    else
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(line);
                        bloomFilter.Add(bytes, hashValues);
                    }
                }
            }

            using (StreamReader reader = new StreamReader("data1.txt"))
            {
                string? line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (addAsString)
                    {
                        Assert.IsTrue(bloomFilter.Contains(line, hashValues));
                    }
                    else
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(line);
                        Assert.IsTrue(bloomFilter.Contains(bytes, hashValues));
                    }
                }
            }

            int totalCount = 0;
            int fpCount = 0;

            using (StreamReader reader = new StreamReader("data2.txt"))
            {
                string? line;

                while ((line = reader.ReadLine()) != null)
                {
                    ++totalCount;

                    if (addAsString)
                    {
                        if (bloomFilter.Contains(line, hashValues))
                        {
                            ++fpCount;
                        }
                    }
                    else
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(line);

                        if (bloomFilter.Contains(bytes, hashValues))
                        {
                            ++fpCount;
                        }
                    }
                }
            }

            double fpRate = ((double)fpCount / totalCount) * 100;
            Console.WriteLine(bloomFilter.FalsePositiveRate);
            Console.WriteLine(fpRate);
            Assert.IsTrue(fpRate < 2.2);
        }

        private void DynamicBloomTestRun(DynamicBloomFilter bloomFilter)
        {
            using (StreamReader reader = new StreamReader("data1.txt"))
            {
                string? line;

                while ((line = reader.ReadLine()) != null)
                {
                    bloomFilter.Add(line);
                }
            }

            using (StreamReader reader = new StreamReader("data1.txt"))
            {
                string? line;

                while ((line = reader.ReadLine()) != null)
                {
                    Assert.IsTrue(bloomFilter.Contains(line));
                }
            }

            int totalCount = 0;
            int fpCount = 0;

            using (StreamReader reader = new StreamReader("data2.txt"))
            {
                string? line;

                while ((line = reader.ReadLine()) != null)
                {
                    ++totalCount;

                    if (bloomFilter.Contains(line))
                    {
                        ++fpCount;
                    }
                }
            }

            double fpRate = ((double)fpCount / totalCount) * 100;
            Console.WriteLine(bloomFilter.FalsePositiveRate);
            Console.WriteLine(fpRate);
            Assert.IsTrue(fpRate < 2.1);

            using (StreamReader reader = new StreamReader("data2.txt"))
            {
                string? line;

                while ((line = reader.ReadLine()) != null)
                {
                    bloomFilter.Add(line);
                }
            }

            totalCount = 0;
            fpCount = 0;

            using (StreamReader reader = new StreamReader("data3.txt"))
            {
                string? line;

                while ((line = reader.ReadLine()) != null)
                {
                    ++totalCount;

                    if (bloomFilter.Contains(line))
                    {
                        ++fpCount;
                    }
                }
            }

            fpRate = ((double)fpCount / totalCount) * 100;
            Console.WriteLine(bloomFilter.FalsePositiveRate);
            Console.WriteLine(fpRate);

            // False positive rate roughly doubles each time the initial capacity is reached, which is much better
            // than if a single filter was used. TODO - add some graphs showing this.
            Assert.IsTrue(fpRate < 4.2);
        }
    }
}