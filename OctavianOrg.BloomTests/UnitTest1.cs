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
        public void TestBasicHashProviderNoValuesArray()
        {
            BloomFilterParameters bloomFilterParameters = new BloomFilterParameters(320344, 0.02);
            BloomFilter bloomFilter = new BloomFilter(bloomFilterParameters, new BasicHashProvider(bloomFilterParameters));
            BloomTestRun(bloomFilter, true, null);
        }

        [TestMethod]
        [DeploymentItem("data1.txt")]
        [DeploymentItem("data2.txt")]
        public void TestBasicHashProviderWithValuesArray()
        {
            BloomFilterParameters bloomFilterParameters = new BloomFilterParameters(320344, 0.02);
            BloomFilter bloomFilter = new BloomFilter(bloomFilterParameters, new BasicHashProvider(bloomFilterParameters));
            long[] hashValues = new long[bloomFilterParameters.HashFunctionCount];
            BloomTestRun(bloomFilter, true, hashValues);
        }

        [TestMethod]
        [DeploymentItem("data1.txt")]
        [DeploymentItem("data2.txt")]
        public void TestDoubleHashProviderNoValuesArray()
        {
            BloomFilterParameters bloomFilterParameters = new BloomFilterParameters(320344, 0.02);
            BloomFilter bloomFilter = new BloomFilter(bloomFilterParameters, new DoubleHashProvider(bloomFilterParameters));
            BloomTestRun(bloomFilter, true, null);
        }

        [TestMethod]
        [DeploymentItem("data1.txt")]
        [DeploymentItem("data2.txt")]
        public void TestDoubleHashProviderWithValuesArray()
        {
            BloomFilterParameters bloomFilterParameters = new BloomFilterParameters(320344, 0.02);
            BloomFilter bloomFilter = new BloomFilter(bloomFilterParameters, new DoubleHashProvider(bloomFilterParameters));
            long[] hashValues = new long[bloomFilterParameters.HashFunctionCount];
            BloomTestRun(bloomFilter, true, hashValues);
        }

        [TestMethod]
        [DeploymentItem("data1.txt")]
        [DeploymentItem("data2.txt")]
        public void TestBasicHashProviderAddAsBytesNoValuesArray()
        {
            BloomFilterParameters bloomFilterParameters = new BloomFilterParameters(320344, 0.02);
            BloomFilter bloomFilter = new BloomFilter(bloomFilterParameters, new BasicHashProvider(bloomFilterParameters));
            BloomTestRun(bloomFilter, false, null);
        }

        [TestMethod]
        [DeploymentItem("data1.txt")]
        [DeploymentItem("data2.txt")]
        public void TestBasicHashProviderAddAsBytesWithValuesArray()
        {
            BloomFilterParameters bloomFilterParameters = new BloomFilterParameters(320344, 0.02);
            BloomFilter bloomFilter = new BloomFilter(bloomFilterParameters, new BasicHashProvider(bloomFilterParameters));
            long[] hashValues = new long[bloomFilterParameters.HashFunctionCount];
            BloomTestRun(bloomFilter, false, hashValues);
        }

        [TestMethod]
        [DeploymentItem("data1.txt")]
        [DeploymentItem("data2.txt")]
        public void TestDoubleHashProviderAddAsBytesNoValuesArray()
        {
            BloomFilterParameters bloomFilterParameters = new BloomFilterParameters(320344, 0.02);
            BloomFilter bloomFilter = new BloomFilter(bloomFilterParameters, new DoubleHashProvider(bloomFilterParameters));
            BloomTestRun(bloomFilter, false, null);
        }

        [TestMethod]
        [DeploymentItem("data1.txt")]
        [DeploymentItem("data2.txt")]
        public void TestDoubleHashProviderAddAsBytesWithValuesArray()
        {
            BloomFilterParameters bloomFilterParameters = new BloomFilterParameters(320344, 0.02);
            BloomFilter bloomFilter = new BloomFilter(bloomFilterParameters, new DoubleHashProvider(bloomFilterParameters));
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

        private void BloomTestRun(BloomFilter bloomFilter, bool addAsString, long[]? hashValues)
        {
            using (StreamReader reader = new StreamReader("data1.txt"))
            {
                string? line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (addAsString)
                    {
                        if (hashValues == null)
                        {
                            bloomFilter.Add(line);
                        }
                        else
                        {
                            bloomFilter.Add(line, hashValues);
                        }
                    }
                    else
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(line);

                        if (hashValues == null)
                        {
                            bloomFilter.Add(bytes);
                        }
                        else
                        {
                            bloomFilter.Add(bytes, hashValues);
                        }
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
                        Assert.IsTrue(bloomFilter.Contains(line));
                    }
                    else
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(line);
                        Assert.IsTrue(bloomFilter.Contains(bytes));
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
                        if (bloomFilter.Contains(line))
                        {
                            ++fpCount;
                        }
                    }
                    else
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(line);

                        if (bloomFilter.Contains(bytes))
                        {
                            ++fpCount;
                        }
                    }
                }
            }

            double fpRate = ((double)fpCount / totalCount) * 100;
            Console.WriteLine(bloomFilter.FalsePositiveRate);
            Console.WriteLine(fpRate);
            Assert.IsTrue(fpRate < 2.1);
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