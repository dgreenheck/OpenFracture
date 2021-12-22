using NUnit.Framework;
using UnityEngine;

public class MathUtilsTests
{
    #region IsPointOnRightSideOfLine Tests
    [Test]
    public void IsPointOnRightSideOfLine_PointOnRightSide()
    {
        var a = new Vector2(0, 0);
        var b = new Vector2(1, 1);
        var p = new Vector2(1, 0);
        Assert.True(MathUtils.IsPointOnRightSideOfLine(a, b, p));
    }

    [Test]
    public void IsPointOnRightSideOfLine_PointOnLeftSide()
    {
        var a = new Vector2(0, 0);
        var b = new Vector2(1, 1);
        var p = new Vector2(0, 1);
        Assert.False(MathUtils.IsPointOnRightSideOfLine(a, b, p));
    }

    [Test]
    public void IsPointOnRightSideOfLine_PointFirstEndpoint()
    {
        var a = new Vector2(0, 0);
        var b = new Vector2(1, 1);
        var p = a;
        Assert.True(MathUtils.IsPointOnRightSideOfLine(a, b, p));
    }

    [Test]
    public void IsPointOnRightSideOfLine_PointSecondEndpoint()
    {
        var a = new Vector2(0, 0);
        var b = new Vector2(1, 1);
        var p = b;
        Assert.True(MathUtils.IsPointOnRightSideOfLine(a, b, p));
    }
    
    [Test]
    public void IsPointOnRightSideOfLine_PointLineMidpoint()
    {
        var a = new Vector2(0, 0);
        var b = new Vector2(1, 1);
        var p = (a + b) / 2;
        Assert.True(MathUtils.IsPointOnRightSideOfLine(a, b, p));
    }
    #endregion

    #region LinePlaneIntersectionTests
    [Test]
    public void LinePlaneIntersection_DegenerateLine()
    {
        Vector3 a = Vector3.one;
        Vector3 b = Vector3.one;
        Vector3 n = Vector3.up;
        Vector3 p0 = Vector3.zero;
        Vector3 x;
        float s;
        Assert.False(MathUtils.LinePlaneIntersection(a, b, n, p0, out x, out s));
    }
    
    [Test]
    public void LinePlaneIntersection_ZeroLengthNormal()
    {
        Vector3 a = Vector3.zero;
        Vector3 b = Vector3.one;
        Vector3 n = Vector3.zero;
        Vector3 p0 = Vector3.zero;
        Vector3 x;
        float s;
        Assert.False(MathUtils.LinePlaneIntersection(a, b, n, p0, out x, out s));
    }
        
    [Test]
    public void LinePlaneIntersection_LineAbovePlane()
    {
        Vector3 a = new Vector3(0, 1, 0);
        Vector3 b = new Vector3(0, 2, 0);
        Vector3 n = new Vector3(0, 1, 0);
        Vector3 p0 = Vector3.zero;
        Vector3 x;
        float s;
        Assert.False(MathUtils.LinePlaneIntersection(a, b, n, p0, out x, out s));
    }
            
    [Test]
    public void LinePlaneIntersection_LineBelowPlane()
    {
        Vector3 a = new Vector3(0, -1, 0);
        Vector3 b = new Vector3(0, -2, 0);
        Vector3 n = new Vector3(0, 1, 0);
        Vector3 p0 = Vector3.zero;
        Vector3 x;
        float s;
        Assert.False(MathUtils.LinePlaneIntersection(a, b, n, p0, out x, out s));
    }
            
    [Test]
    public void LinePlaneIntersection_LineCrossingPlane()
    {
        Vector3 a = new Vector3(0, -1, 0);
        Vector3 b = new Vector3(0, 1, 0);
        Vector3 n = new Vector3(0, 1, 0);
        Vector3 p0 = Vector3.zero;
        Vector3 x;
        float s;
        Assert.True(MathUtils.LinePlaneIntersection(a, b, n, p0, out x, out s));

        // Intersection point crosses the mid-point of the line
        Assert.AreEqual(Vector3.zero, x);
        Assert.AreEqual(0.5f, s);
    }

    [Test]
    public void LinePlaneIntersection_StartPointOnPlane()
    {
        Vector3 a = new Vector3(0, 0, 0);
        Vector3 b = new Vector3(0, 1, 0);
        Vector3 n = new Vector3(0, 1, 0);
        Vector3 p0 = Vector3.zero;
        Vector3 x;
        float s;
        Assert.True(MathUtils.LinePlaneIntersection(a, b, n, p0, out x, out s));

        // Intersection point crosses the mid-point of the line
        Assert.AreEqual(Vector3.zero, x);
        Assert.AreEqual(0, s);
    }
    
    [Test]
    public void LinePlaneIntersection_EndPointOnPlane()
    {
        Vector3 a = new Vector3(0, 1, 0);
        Vector3 b = new Vector3(0, 0, 0);
        Vector3 n = new Vector3(0, 1, 0);
        Vector3 p0 = Vector3.zero;
        Vector3 x;
        float s;
        Assert.True(MathUtils.LinePlaneIntersection(a, b, n, p0, out x, out s));

        // Intersection point crosses the mid-point of the line
        Assert.AreEqual(Vector3.zero, x);
        Assert.AreEqual(1, s);
    }
    #endregion

    #region LinesIntersect General Tests
    [Test]
    public void LinesIntersect_True()
    {
        Vector2 a1 = new Vector2(0, 0);
        Vector2 a2 = new Vector2(1, 1);
        Vector2 b1 = new Vector2(0, 1);
        Vector2 b2 = new Vector2(1, 0);
        Assert.True(MathUtils.LinesIntersect(a1, a2, b1, b2));
    }

    [Test]
    public void LinesIntersect_LineAOnLeftSideOfLineB()
    {
        Vector2 a1 = new Vector2(0, 0.5f);
        Vector2 a2 = new Vector2(0.49f, 0.5f);
        Vector2 b1 = new Vector2(0.5f, 0f);
        Vector2 b2 = new Vector2(0.5f, 1);
        Assert.False(MathUtils.LinesIntersect(a1, a2, b1, b2));
    }
        
    [Test]
    public void LinesIntersect_LineAOnRightSideOfLineB()
    {
        Vector2 a1 = new Vector2(0.51f, 0.5f);
        Vector2 a2 = new Vector2(1f, 0.5f);
        Vector2 b1 = new Vector2(0.5f, 0f);
        Vector2 b2 = new Vector2(0.5f, 1);
        Assert.False(MathUtils.LinesIntersect(a1, a2, b1, b2));
    }
    
    [Test]
    public void LinesIntersect_LineBOnTopSideOfLineA()
    {
        Vector2 a1 = new Vector2(0, 0.5f);
        Vector2 a2 = new Vector2(1f, 0.5f);
        Vector2 b1 = new Vector2(0.5f, 0.51f);
        Vector2 b2 = new Vector2(0.5f, 1);
        Assert.False(MathUtils.LinesIntersect(a1, a2, b1, b2));
    }
        
    [Test]
    public void LinesIntersect_LineBOnBottomSideOfLineA()
    {
        Vector2 a1 = new Vector2(0, 0.5f);
        Vector2 a2 = new Vector2(1f, 0.5f);
        Vector2 b1 = new Vector2(0.5f, 0f);
        Vector2 b2 = new Vector2(0.5f, 0.49f);
        Assert.False(MathUtils.LinesIntersect(a1, a2, b1, b2));
    }
    #endregion

    #region LinesIntersect Shared Vertex Tests
    [Test]
    public void LinesIntersect_A1B1Shared()
    {
        Vector2 a1 = new Vector2(0, 0);
        Vector2 a2 = new Vector2(1, 1);
        Vector2 b1 = a1;
        Vector2 b2 = new Vector2(1, 0);
        Assert.False(MathUtils.LinesIntersect(a1, a2, b1, b2));
    }
        
    [Test]
    public void LinesIntersect_A1B2Shared()
    {
        Vector2 a1 = new Vector2(0, 0);
        Vector2 a2 = new Vector2(1, 1);
        Vector2 b1 = new Vector2(0, 1);
        Vector2 b2 = a1;
        Assert.False(MathUtils.LinesIntersect(a1, a2, b1, b2));
    }
            
    [Test]
    public void LinesIntersect_A2B1Shared()
    {
        Vector2 a1 = new Vector2(0, 0);
        Vector2 a2 = new Vector2(1, 1);
        Vector2 b1 = a2;
        Vector2 b2 = new Vector2(1, 0);
        Assert.False(MathUtils.LinesIntersect(a1, a2, b1, b2));
    } 

    [Test]
    public void LinesIntersect_A2B2Shared()
    {
        Vector2 a1 = new Vector2(0, 0);
        Vector2 a2 = new Vector2(1, 1);
        Vector2 b1 = new Vector2(0, 1);
        Vector2 b2 = a2;
        Assert.False(MathUtils.LinesIntersect(a1, a2, b1, b2));
    }
    #endregion

    #region IsQuadConvex Shared Vertex Tests
    [Test]
    public void IsQuadConvex_A1B1Shared()
    {
        Vector2 a1 = new Vector2(0, 0);
        Vector2 a2 = new Vector2(1, 1);
        Vector2 b1 = a1;
        Vector2 b2 = new Vector2(1, 0);
        Assert.True(MathUtils.IsQuadConvex(a1, a2, b1, b2));
    }
        
    [Test]
    public void IsQuadConvex_A1B2Shared()
    {
        Vector2 a1 = new Vector2(0, 0);
        Vector2 a2 = new Vector2(1, 1);
        Vector2 b1 = new Vector2(0, 1);
        Vector2 b2 = a1;
        Assert.True(MathUtils.IsQuadConvex(a1, a2, b1, b2));
    }
            
    [Test]
    public void IsQuadConvex_A2B1Shared()
    {
        Vector2 a1 = new Vector2(0, 0);
        Vector2 a2 = new Vector2(1, 1);
        Vector2 b1 = a2;
        Vector2 b2 = new Vector2(1, 0);
        Assert.True(MathUtils.IsQuadConvex(a1, a2, b1, b2));
    } 

    [Test]
    public void IsQuadConvex_A2B2Shared()
    {
        Vector2 a1 = new Vector2(0, 0);
        Vector2 a2 = new Vector2(1, 1);
        Vector2 b1 = new Vector2(0, 1);
        Vector2 b2 = a2;
        Assert.True(MathUtils.IsQuadConvex(a1, a2, b1, b2));
    }
    #endregion

    #region LinesIntersect Vertex on Line Tests
    [Test]
    public void LinesIntersect_A1OnBLine()
    {
        Vector2 a1 = new Vector2(0.5f, 0.5f);
        Vector2 a2 = new Vector2(1, 0.5f);
        Vector2 b1 = new Vector2(0.5f, 0);
        Vector2 b2 = new Vector2(0.5f, 1);
        Assert.True(MathUtils.LinesIntersect(a1, a2, b1, b2));
    }
    
    [Test]
    public void LinesIntersect_A2OnBLine()
    {
        Vector2 a1 = new Vector2(0, 0.5f);
        Vector2 a2 = new Vector2(0.5f, 0.5f);
        Vector2 b1 = new Vector2(0.5f, 0);
        Vector2 b2 = new Vector2(0.5f, 1);
        Assert.True(MathUtils.LinesIntersect(a1, a2, b1, b2));
    }
        
    [Test]
    public void LinesIntersect_B1OnALine()
    {
        Vector2 a1 = new Vector2(0, 0.5f);
        Vector2 a2 = new Vector2(1, 0.5f);
        Vector2 b1 = new Vector2(0.5f, 0.5f);
        Vector2 b2 = new Vector2(0.5f, 1);
        Assert.True(MathUtils.LinesIntersect(a1, a2, b1, b2));
    }
        
    [Test]
    public void LinesIntersect_B2OnALine()
    {
        Vector2 a1 = new Vector2(0, 0.5f);
        Vector2 a2 = new Vector2(1, 0.5f);
        Vector2 b1 = new Vector2(0.5f, 0);
        Vector2 b2 = new Vector2(0.5f, 0.5f);
        Assert.True(MathUtils.LinesIntersect(a1, a2, b1, b2));
    }
    #endregion
}