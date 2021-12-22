using NUnit.Framework;
using UnityEngine;

public class MeshVertexTests
{
    [Test]
    public void EqualPositionsEqual()
    {
        MeshVertex vertexA = new MeshVertex(new Vector3(1, 2, 3), Vector3.up, Vector2.zero);
        MeshVertex vertexB = new MeshVertex(new Vector3(1, 2, 3), Vector3.up, Vector2.zero);
        Assert.True(vertexA == vertexB);
    }
    
    [Test]
    public void DifferentPositionsNotEqual()
    {
        MeshVertex vertexA = new MeshVertex(new Vector3(1, 2, 3), Vector3.up, Vector2.zero);
        MeshVertex vertexB = new MeshVertex(new Vector3(1, 2, 3), Vector3.up, Vector2.zero);
        Assert.True(vertexA == vertexB);
    }
}