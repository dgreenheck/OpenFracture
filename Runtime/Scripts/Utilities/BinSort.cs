/// <summary>
/// Defines an interface for an object that is sorted by bin number
/// </summary>
public interface IBinSortable
{
    int bin { get; set; }
}

/// <summary>
/// Methods for sorting objects on an ordered grid by bin number.
/// 
/// The grid ordering is shown by example below. Even rows (row 0 = bottom row) are ordered
/// right-to-left while odd rows are ordered left-to-right.
///  _____ _____ _____
/// |     |     |     |
/// |  6  |  7  |  8  |
/// |_____|_____|_____|
/// |     |     |     |
/// |  5  |  4  |  3  |
/// |_____|_____|_____|
/// |     |     |     |
/// |  0  |  1  |  2  |
/// |_____|_____|_____|
/// 
/// </summary>
public class BinSort
{
    /// <summary>
    /// Computes the bin number for the set of grid coordinates
    /// </summary>
    /// <param name="i">Grid row</param>
    /// <param name="j">Grid column</param>
    /// <param name="n">Grid size</param>
    /// <returns></returns>
    internal static int GetBinNumber(int i, int j, int n)
    {
        return (i % 2 == 0) ? (i * n) + j : (i + 1) * n - j - 1;
    }

    /// <summary>
    /// Performs a counting sort of the input points based on their bin number. Only
    /// sorts the elements in the index range [0, count]. If binCount is <= 1, no sorting
    /// is performed. If lastIndex > input.Length, the entire input array is sorted.
    /// </summary>
    /// <param name="input">The input array to sort</param>
    /// <param name="lastIndex">The index of the last element in `input` to sort. Only the
    /// elements [0, lastIndex) are sorted.</param>
    /// <param name="binCount">Number of bins</param>
    internal static T[] Sort<T>(T[] input, int lastIndex, int binCount) where T: IBinSortable
    {
        int[] count = new int[binCount];
        T[] output = new T[input.Length];

        #region Validation
        // Need at least two bins to sort
        if (binCount <= 1)
        {
            return input;
        }

        // If lastIndex is out of range, default to sorting the entire input array
        if (lastIndex > input.Length)
        {
            lastIndex = input.Length;
        }
        #endregion

        // Only sort the first [0, count] points, don't want to sort super-triangle vertices
        for (int i = 0; i < lastIndex; i++)
        {
            int j = input[i].bin;
            count[j] += 1;
        }

        for (int i = 1; i < binCount; i++)
        {
            count[i] += count[i - 1];
        }

        for (int i = lastIndex - 1; i >= 0; i--)
        {
            int j = input[i].bin;
            count[j] -= 1;
            output[count[j]] = input[i];
        }

        // Copy over the rest of the un-sorted points
        for (int i = lastIndex; i < output.Length; i++)
        {
            output[i] = input[i];
        }

        return output;
    }

}