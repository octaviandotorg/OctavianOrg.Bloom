

namespace OctavianOrg.Bloom
{
    public interface IBitSet
    {
        /// <summary>
        /// Sets the specified bit and returns whether the bit was changed.
        /// </summary>
        /// <param name="bitNumber">
        /// The 0-based bit number to set.
        /// </param>
        /// <returns>
        /// True if the bit was 0 then set to 1, false otherwise.
        /// </returns>
        bool Set(long bitNumber);

        /// <summary>
        /// Determines whether the specified bit is set.
        /// </summary>
        /// <param name="bitNumber">
        /// The 0-based bit number to check.
        /// </param>
        /// <returns>
        /// True if the bit is set, false otherwise.
        /// </returns>
        bool IsSet(long bitNumber);
    }
}
