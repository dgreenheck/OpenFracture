using NUnit.Framework;

public class EdgeConstraintTests
{
    [Test]
    public void IdenticalEdgesAreEqual()
    {
        EdgeConstraint edgeA = new EdgeConstraint(1, 2);
        EdgeConstraint edgeB = new EdgeConstraint(1, 2);

        Assert.True(edgeA == edgeB);
    }

    [Test]
    public void DifferentV1EdgesAreNotEqual()
    {
        EdgeConstraint edgeA = new EdgeConstraint(1, 2);
        EdgeConstraint edgeB = new EdgeConstraint(3, 2);
        Assert.False(edgeA == edgeB);
    }
    
    [Test]
    public void DifferentV2EdgesAreNotEqual()
    {
        EdgeConstraint edgeA = new EdgeConstraint(1, 2);
        EdgeConstraint edgeB = new EdgeConstraint(1, 3);

        Assert.False(edgeA == edgeB);
    }   

    [Test]
    public void EdgesInOppositeDirectionsAreEqual()
    {
        EdgeConstraint edgeA = new EdgeConstraint(1, 2);
        EdgeConstraint edgeB = new EdgeConstraint(2, 1);
        Assert.True(edgeA == edgeB);
    }
    
    [Test]
    public void VerifyHashCodeEqualEdges()
    {
        EdgeConstraint edgeA = new EdgeConstraint(1, 2);
        EdgeConstraint edgeB = new EdgeConstraint(1, 2);
        Assert.True(edgeA.GetHashCode() == edgeB.GetHashCode());
    }

    [Test]
    public void VerifyHashCodeReversedEdges()
    {
        EdgeConstraint edgeA = new EdgeConstraint(1, 2);
        EdgeConstraint edgeB = new EdgeConstraint(2, 1);
        Assert.True(edgeA.GetHashCode() == edgeB.GetHashCode());
    }
    
    [Test]
    public void VerifyHashCodeDifferentEdges()
    {
        EdgeConstraint edgeA = new EdgeConstraint(1, 2);
        EdgeConstraint edgeB = new EdgeConstraint(1, 3);
        Assert.False(edgeA.GetHashCode() == edgeB.GetHashCode());
    }
}