using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ConstrainedTriangulatorTests
{
    [Test]
    public void TestNullInputPoints()
    {
        var triangulator = new ConstrainedTriangulator(null, new List<EdgeConstraint>(), Vector3.forward);
        int[] triangles = triangulator.Triangulate();
        Assert.Zero(triangles.Length);
    }

    [Test]
    public void TestNullConstraints()
    {
        var triangulator = new ConstrainedTriangulator(new List<MeshVertex>(), null, Vector3.forward);
        int[] triangles = triangulator.Triangulate();
        Assert.Zero(triangles.Length);
    }

    [Test]
    public void TestEmptyInputPoints()
    {
        var triangulator = new ConstrainedTriangulator(new List<MeshVertex>(), new List<EdgeConstraint>(), Vector3.forward);
        int[] triangles = triangulator.Triangulate();
        Assert.Zero(triangles.Length);
    }

    [Test]
    public void TestLessThanThreeInputPoints()
    {
        List<MeshVertex> points = new List<MeshVertex>();
        points.Add(new MeshVertex(Vector3.zero));
        points.Add(new MeshVertex(Vector3.one));

        var triangulator = new ConstrainedTriangulator(points, new List<EdgeConstraint>(), Vector3.forward);
        int[] triangles = triangulator.Triangulate();
        Assert.Zero(triangles.Length);
    }

    [Test]
    public void TestUnconstrainedConvexPolygons()
    {
        // This test generates points for regular convex polygons of n = 3 to n = 20
        // and verifies the triangulation is correct. Each polygon has a vertex in its
        // center as well to ensure the triangulation is identical between runs.
        for (int n = 3; n <= 20; n++)
        {
            // Create the points of the polygon
            List<MeshVertex> points = new List<MeshVertex>();
            
            // Add an additional center point
            points.Add(new MeshVertex(Vector3.zero));

            for (int i = 0; i < n; i++)
            {   
                float angle = ((float)i / (float)n) * 2f * Mathf.PI;
                points.Add(new MeshVertex(new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f)));
            }

            var triangulator = new ConstrainedTriangulator(points, new List<EdgeConstraint>(), Vector3.forward);
            int[] triangles = triangulator.Triangulate();

            // Verify the triangulation has the correct number of triangles
            Assert.AreEqual(3 * n, triangles.Length);

            for (int i = 0; i < triangles.Length; i += 3)
            {
                // Verify each contains the origin point
                Assert.True(triangles[i] == 0 || triangles[i + 1] == 0 || triangles[i + 2] == 0);

                // Verify the other two vertices are adjacent and wound clockwise
                if (triangles[i] == 0)
                {
                    Assert.AreEqual(triangles[i + 2], GetAdjacentVertex(triangles[i + 1], points.Count));
                }
                else if (triangles[i + 1] == 0)
                {
                    Assert.AreEqual(triangles[i], GetAdjacentVertex(triangles[i + 2], points.Count));
                }
                else if (triangles[i + 2] == 0)
                {
                    Assert.AreEqual(triangles[i + 1], GetAdjacentVertex(triangles[i], points.Count));
                }
            }
        }
    }

    private int GetAdjacentVertex(int i, int n)
    {
        if ((i + 1) < n)
        {
            return i + 1;
        }
        else
        {
            // If i == n, adjacent vertex is i == 1
            return ((i + 1) % n) + 1;
        }
    }
}