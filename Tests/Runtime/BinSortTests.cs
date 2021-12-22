using System.Collections.Generic;
using NUnit.Framework;

public class BinSortTests
{
    [Test]
    public void BinNumber_SingleBin()
    {
        int n = 1;
        Assert.AreEqual(0, BinSort.GetBinNumber(0, 0, n));
    }

    [Test]
    public void BinNumber_EvenGrid()
    {
        int n = 2;
        Assert.AreEqual(0, BinSort.GetBinNumber(0, 0, n)); // Lower-left
        Assert.AreEqual(1, BinSort.GetBinNumber(0, 1, n)); // Lower-right
        Assert.AreEqual(2, BinSort.GetBinNumber(1, 1, n)); // Upper-right
        Assert.AreEqual(3, BinSort.GetBinNumber(1, 0, n)); // Upper-left
    }
    
    [Test]
    public void BinNumber_OddGrid()
    {
        int n = 3;
        Assert.AreEqual(0, BinSort.GetBinNumber(0, 0, n));
        Assert.AreEqual(1, BinSort.GetBinNumber(0, 1, n));
        Assert.AreEqual(2, BinSort.GetBinNumber(0, 2, n));
        Assert.AreEqual(3, BinSort.GetBinNumber(1, 2, n));
        Assert.AreEqual(4, BinSort.GetBinNumber(1, 1, n));
        Assert.AreEqual(5, BinSort.GetBinNumber(1, 0, n));
        Assert.AreEqual(6, BinSort.GetBinNumber(2, 0, n));
        Assert.AreEqual(7, BinSort.GetBinNumber(2, 1, n));
        Assert.AreEqual(8, BinSort.GetBinNumber(2, 2, n));
    }

    [Test]
    public void Sort_EmptyPointsList()
    {
        int binCount = 1;
        var input = new BinnedObjectMock[0];
        var output = BinSort.Sort<BinnedObjectMock>(input, input.Length, binCount);

        // Expect to get back reference to the input array
        Assert.AreEqual(input, output);
    }

    [Test]
    public void Sort_ZeroBinCount()
    {
        int binCount = 0;
        var input = new BinnedObjectMock[0];
        var output = BinSort.Sort<BinnedObjectMock>(input, input.Length, binCount);

        // Expect to get back reference to the input array
        Assert.AreEqual(input, output);
    }

    [Test]
    public void Sort_SingleBin()
    {
        int binCount = 1;
        var input = new BinnedObjectMock[] {
            new BinnedObjectMock(0)
        };

        var output = BinSort.Sort<BinnedObjectMock>(input, input.Length, binCount);

        // Expect to get back reference to the input array
        Assert.AreEqual(input, output);
    }
    
    [Test]
    public void Sort_MultipleBinsFullSort()
    {
        var binCount = 10;
        var lastIndex = binCount;
        var input = new List<BinnedObjectMock>(binCount);
        
        // Give each object a separate bin number.
        // Input array is in reverse order
        for (int i = 0; i < binCount; i++) {
            input.Insert(0, new BinnedObjectMock(i));
        };
        
        var output = BinSort.Sort<BinnedObjectMock>(input.ToArray(), input.Count, binCount);

        // Input and output are different but have same # of elements
        Assert.AreNotEqual(input, output);
        Assert.AreEqual(input.Count, output.Length);

        // Verify sort
        for (int i = 0; i < output.Length; i++)
        {
            Assert.AreEqual(i, output[i].bin);
        }
    }
        
    [Test]
    public void Sort_MultipleBinsPartialSort()
    {
        var binCount = 10;
        var lastIndex = 5;
        var input = new List<BinnedObjectMock>(binCount);
        
        // Give each object a separate bin number.
        // Input array is in reverse order
        for (int i = 0; i < binCount; i++) {
            input.Insert(0, new BinnedObjectMock(i));
        };
        
        var output = BinSort.Sort<BinnedObjectMock>(input.ToArray(), lastIndex, binCount);

        // Input and output are different but have same # of elements
        Assert.AreNotEqual(input, output);
        Assert.AreEqual(input.Count, output.Length);

        // Verify sort
        for (int i = 0; i < lastIndex; i++)
        {
            Assert.AreEqual(i + lastIndex, output[i].bin);
        }
        // Last elements should not be sorted since we only sorted up to lastIndex
        for (int i = lastIndex; i < output.Length; i++)
        {
            Assert.AreEqual(output.Length - i - 1, output[i].bin);
        }
    }
          
    [Test]
    public void Sort_LastIndexOutOfRange()
    {
        var binCount = 10;
        var lastIndex = binCount + 1;
        var input = new List<BinnedObjectMock>(binCount);
        
        // Give each object a separate bin number.
        // Input array is in reverse order
        for (int i = 0; i < binCount; i++) {
            input.Insert(0, new BinnedObjectMock(i));
        };
        
        var output = BinSort.Sort<BinnedObjectMock>(input.ToArray(), lastIndex, binCount);

        // Input and output are different but have same # of elements
        Assert.AreNotEqual(input, output);
        Assert.AreEqual(input.Count, output.Length);

        // Expect to get back reference to the input array
        for (int i = 0; i < output.Length; i++)
        {
            Assert.AreEqual(i, output[i].bin);
        }
    }
}

public class BinnedObjectMock: IBinSortable
{
    public int bin { get; set; }

    internal BinnedObjectMock(int bin)
    {
        this.bin = bin;
    }

    public override string ToString()
    {
        return $"Bin = {bin}";
    }
}