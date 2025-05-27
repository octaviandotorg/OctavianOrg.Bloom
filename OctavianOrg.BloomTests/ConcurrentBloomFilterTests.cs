using OctavianOrg.Bloom;
using System.Collections.Concurrent;

namespace OctavianOrg.BloomTests
{
    [TestClass]
    public class ConcurrentBloomFilterTests
    {
        private const double FALSE_POSITIVE_RATE = 0.02; // 2%
        private const int ELEMENT_COUNT = 320344;

        [TestMethod]
        [DeploymentItem("data1.txt")]
        [DeploymentItem("data2.txt")]
        public void TestConcurrentAddAndContains()
        {
            // Arrange
            BloomFilterParameters parameters = new BloomFilterParameters(ELEMENT_COUNT, FALSE_POSITIVE_RATE);
            ConcurrentBloomFilter filter = new ConcurrentBloomFilter(parameters, new DoubleHashProvider(parameters), -1);

            // Load all lines from data1.txt
            List<string> lines = new List<string>(ELEMENT_COUNT);
            List<string> chunk1 = new List<string>(106781);
            List<string> chunk2 = new List<string>(106781);
            List<string> chunk3 = new List<string>(106781);
            List<string> chunk4 = new List<string>(160172);

            using (StreamReader reader = new StreamReader("data1.txt"))
            {
                string? line;
                int count = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);

                    if (count < 106781)
                    {
                        chunk1.Add(line);
                    }

                    if (count >= 106781 && count < 213562)
                    {
                        chunk2.Add(line);
                    }

                    if (count >= 213562)
                    {
                        chunk3.Add(line);
                    }

                    if (count >= 0 && count < 160172)
                    {
                        chunk4.Add(line);
                    }
                }
            }

            // Act - Run concurrent operations
            Parallel.Invoke(
                () => AddItems(filter, chunk1.ToArray()),
                () => AddItems(filter, chunk2.ToArray()),
                () => AddItems(filter, chunk3.ToArray()),
                () => CheckItems(filter, chunk4.ToArray())
            );

            // Assert - Verify all items were added correctly
            long[] hashValues = new long[parameters.HashFunctionCount];
            foreach (string line in lines)
            {
                Assert.IsTrue(filter.Contains(line, hashValues), $"Filter should contain '{line}'");
            }
            
            // Check false positive rate
            string[] nonExistingItems = File.ReadAllLines("data2.txt");
            int falsePositives = 0;
            
            foreach (string item in nonExistingItems)
            {
                if (filter.Contains(item, hashValues))
                {
                    falsePositives++;
                }
            }
            
            double actualFalsePositiveRate = (double)falsePositives / nonExistingItems.Length;
            Console.WriteLine($"Expected FP rate: {FALSE_POSITIVE_RATE}, Actual FP rate: {actualFalsePositiveRate}");
            Assert.IsTrue(actualFalsePositiveRate <= FALSE_POSITIVE_RATE * 1.1, 
                $"False positive rate ({actualFalsePositiveRate}) exceeds expected rate ({FALSE_POSITIVE_RATE})");
        }
        
        [TestMethod]
        public void TestConcurrencyLevelDefault()
        {
            // Arrange
            BloomFilterParameters parameters = new BloomFilterParameters(1000, 0.01);
            
            // Act
            ConcurrentBloomFilter filter = new ConcurrentBloomFilter(parameters, new BasicHashProvider(parameters), -1);
            
            // Assert
            Assert.AreEqual(Environment.ProcessorCount, filter.ConcurrencyLevel);
        }
        
        [TestMethod]
        public void TestConcurrencyLevelCustom()
        {
            // Arrange
            BloomFilterParameters parameters = new BloomFilterParameters(1000, 0.01);
            int customLevel = 16;
            
            // Act
            ConcurrentBloomFilter filter = new ConcurrentBloomFilter(parameters, new BasicHashProvider(parameters), customLevel);
            
            // Assert
            Assert.AreEqual(customLevel, filter.ConcurrencyLevel);
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestInvalidConcurrencyLevel()
        {
            // Arrange
            BloomFilterParameters parameters = new BloomFilterParameters(1000, 0.01);
            
            // Act - Should throw ArgumentOutOfRangeException
            ConcurrentBloomFilter filter = new ConcurrentBloomFilter(parameters, new BasicHashProvider(parameters), 0);
        }
        
        [TestMethod]
        [DeploymentItem("data1.txt")]
        public void TestHighConcurrencyStressTest()
        {
            // Arrange
            BloomFilterParameters parameters = new BloomFilterParameters(ELEMENT_COUNT, FALSE_POSITIVE_RATE);
            ConcurrentBloomFilter filter = new ConcurrentBloomFilter(parameters, new DoubleHashProvider(parameters), 32);
            string[] lines = File.ReadAllLines("data1.txt");
            
            // Create a set of random items to add and check
            Random random = new Random(42);
            List<string> itemsToAdd = new List<string>();
            for (int i = 0; i < 10000; i++)
            {
                itemsToAdd.Add(lines[random.Next(lines.Length)]);
            }
            
            // Act - Run many concurrent tasks
            ConcurrentBag<bool> results = new ConcurrentBag<bool>();
            int taskCount = 20;
            Task[] tasks = new Task[taskCount];
            
            for (int i = 0; i < taskCount; i++)
            {
                int taskId = i;
                tasks[i] = Task.Run(() => {
                    long[] hashValues = new long[parameters.HashFunctionCount];
                    bool allSucceeded = true;
                    
                    // Each task adds and checks items
                    for (int j = 0; j < 500; j++)
                    {
                        string item = itemsToAdd[(taskId * 500 + j) % itemsToAdd.Count];
                        filter.Add(item, hashValues);
                        
                        // Immediately check if the item was added
                        if (!filter.Contains(item, hashValues))
                        {
                            allSucceeded = false;
                            break;
                        }
                        
                        // Small delay to increase chance of thread interleaving
                        if (j % 50 == 0) Thread.Sleep(1);
                    }
                    
                    results.Add(allSucceeded);
                });
            }
            
            Task.WaitAll(tasks);
            
            // Assert
            Assert.IsTrue(results.All(r => r), "All concurrent operations should succeed");
            Assert.IsTrue(filter.Count > 0, "Filter should have items added");
        }
        
        [TestMethod]
        public void TestCountAccuracy()
        {
            // Arrange
            BloomFilterParameters parameters = new BloomFilterParameters(1000, 0.01);
            ConcurrentBloomFilter filter = new ConcurrentBloomFilter(parameters, new BasicHashProvider(parameters), -1);
            int itemCount = 500;
            string[] uniqueItems = Enumerable.Range(0, itemCount).Select(i => $"Item_{i}").ToArray();
            
            // Act - Add items sequentially first to establish baseline
            long[] hashValues = new long[parameters.HashFunctionCount];
            foreach (string item in uniqueItems)
            {
                filter.Add(item, hashValues);
            }
            
            long initialCount = filter.Count;

            // Now add the same items concurrently
            Parallel.ForEach(uniqueItems, item => {
                long[] threadHashValues = new long[parameters.HashFunctionCount];
                filter.Add(item, threadHashValues);
            });
            
            // Assert
            Assert.AreEqual(initialCount, filter.Count, 
                "Count should not change when adding existing items");
        }
        
        private void AddItems(BloomFilter filter, string[] items)
        {
            long[] hashValues = new long[filter.Parameters.HashFunctionCount];
            foreach (string item in items)
            {
                filter.Add(item, hashValues);
            }
        }
        
        private void CheckItems(BloomFilter filter, string[] items)
        {
            long[] hashValues = new long[filter.Parameters.HashFunctionCount];
            foreach (string item in items)
            {
                // Just check items - don't assert here as some items might not be added yet
                bool contains = filter.Contains(item, hashValues);
            }
        }
    }
}
