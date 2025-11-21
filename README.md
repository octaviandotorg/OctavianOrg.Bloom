This is a C# library for investigating different bloom filter variants. A bloom filter is a space-efficient probabilistic data structure that is used to test whether an element is a member of a set.
False positives are possible, but false negatives are not.

In this implementation I am focusing on a variant called a partitioned bloom filter, which splits the bit array into multiple partitions, each corresponding to a different hash function.

The supplied hash function is probably a bit slow for production work, but is simple and easy to understand.
I have tried to follow guidance from the paper "Less Hashing, Same Performance: Building a Better Bloom Filter" by Adam Kirsch and Michael Mitzenmacher and "A Case for Partitioned Bloom Filters" by Paulo Sérgio Almeida.
