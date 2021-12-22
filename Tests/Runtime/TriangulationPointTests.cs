using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TriangulationPointTests
{
    [Test]
    public void TestInit()
    {
        int index = 1;
        Vector2 coords = new Vector2(0.5f, 0.75f);

        var point = new TriangulationPoint(index, coords);

        Assert.AreEqual(index, point.index);
        Assert.AreEqual(coords, point.coords);
    }
}