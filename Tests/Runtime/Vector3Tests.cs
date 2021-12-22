using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class Vector3Tests
{
    [Test]
    public void PointAbovePlaneReturnsTrue()
    {
        var planeOrigin = Vector3.zero;
        var planeNormal = Vector3.up;
        var testPoint = Vector3.up;
        Assert.True(testPoint.IsAbovePlane(planeNormal, planeOrigin));
    }

    [Test]
    public void PointBelowPlaneReturnsFalse()
    {
        var planeOrigin = Vector3.zero;
        var planeNormal = Vector3.up;
        var testPoint = -Vector3.up;
        Assert.False(testPoint.IsAbovePlane(planeNormal, planeOrigin));
    }
    
    [Test]
    public void PointOnPlaneReturnsTrue()
    {
        var planeOrigin = Vector3.zero;
        var planeNormal = Vector3.up;
        var testPoint = Vector3.right;
        Assert.True(testPoint.IsAbovePlane(planeNormal, planeOrigin));
    }

    [Test]
    public void PointEqualToPlaneOriginReturnsTrue()
    {
        var planeOrigin = Vector3.one;
        var planeNormal = Vector3.up;
        var testPoint = planeOrigin;
        Assert.True(testPoint.IsAbovePlane(planeNormal, planeOrigin));
    }
}