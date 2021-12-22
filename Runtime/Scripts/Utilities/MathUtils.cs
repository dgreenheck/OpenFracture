using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtils
{
    /// <summary>
    /// Returns true if the quad specified by the two diagonals a1->a2 and b1->b2 is convex
    /// Quad is convex if a1->a2 and b1->b2 intersect each other
    /// </summary>
    /// <param name="a1">Start point of diagonal A</param>
    /// <param name="a2">End point of diagonal A</param>
    /// <param name="b1">Start point of diagonal B</param>
    /// <param name="b2">End point of diagonal B</param>
    /// <returns></returns>
    public static bool IsQuadConvex(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
    {
        return LinesIntersectInternal(a1, a2, b1, b2, true);
    }

    /// <summary>
    /// Returns true lines a1->a2 and b1->b2 is intersect
    /// </summary>
    /// <param name="a1">Start point of line A</param>
    /// <param name="a2">End point of line A</param>
    /// <param name="b1">Start point of line B</param>
    /// <param name="b2">End point of line B</param>
    /// <returns></returns>
    public static bool LinesIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
    {
        return LinesIntersectInternal(a1, a2, b1, b2, false);
    }

    /// <summary>
    /// Returns true lines a1->a2 and b1->b2 is intersect
    /// </summary>
    /// <param name="a1">Start point of line A</param>
    /// <param name="a2">End point of line A</param>
    /// <param name="b1">Start point of line B</param>
    /// <param name="b2">End point of line B</param>
    /// <returns></returns>
    private static bool LinesIntersectInternal(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, bool includeSharedEndpoints)
    {
        Vector2 a12 = new Vector2(a2.x - a1.x, a2.y - a1.y);
        Vector2 b12 = new Vector2(b2.x - b1.x, b2.y - b1.y);
        
        // If any of the vertices are shared between the two diagonals,
        // the quad collapses into a triangle and is convex by default.
        if (a1 == b1 || a1 == b2 || a2 == b1 || a2 == b2)
        {
            return includeSharedEndpoints;
        }
        else
        {
            // Compute cross product between each point and the opposite diagonal
            // Look at sign of the Z component to see which side of line point is on
            float a1xb = (a1.x - b1.x) * b12.y - (a1.y - b1.y) * b12.x;
            float a2xb = (a2.x - b1.x) * b12.y - (a2.y - b1.y) * b12.x;
            float b1xa = (b1.x - a1.x) * a12.y - (b1.y - a1.y) * a12.x;
            float b2xa = (b2.x - a1.x) * a12.y - (b2.y - a1.y) * a12.x;

            // Check that the points for each diagonal lie on opposite sides of the other
            // diagonal. Quad is also convex if a1/a2 lie on b1->b2 (and vice versa) since
            // the shape collapses into a triangle (hence >= instead of >)
            return ((a1xb >= 0 && a2xb <= 0) || (a1xb <= 0 && a2xb >= 0)) &&
                   ((b1xa >= 0 && b2xa <= 0) || (b1xa <= 0 && b2xa >= 0));
        }
    }

    /// <summary>
    /// Determines the intersection between the line segment a->b and the plane defined by the specified normal and origin point. If an intersection point exists, it is returned via the out parameter `intersection`. The parameter `s` is defined below and is used to properly interpolate normals/uvs for intersection vertices.
    /// </summary>
    /// <param name="a">Start point of line</param>
    /// <param name="b">End point of line</param>
    /// <param name="n">Plane normal</param>
    /// <param name="p0">Plane origin</param>
    /// <param name="x">If intersection exists, intersection point return as out parameter.</param>
    /// <param name="s">Returns the parameterization of the intersection where x = a + (b - a) * s</param>
    /// <returns></returns>
    public static bool LinePlaneIntersection(Vector3 a,
                                             Vector3 b,
                                             Vector3 n,
                                             Vector3 p0,
                                             out Vector3 x,
                                             out float s)
    {
        // Initialize out params
        s = 0;
        x = Vector3.zero;

        // Handle degenerate cases
        if (a == b)
        {
            return false;
        }
        else if (n == Vector3.zero)
        {
            return false;
        }

        // `s` is the parameter for the line segment a -> b where 0.0 <= s <= 1.0
        s = Vector3.Dot(p0 - a, n) / Vector3.Dot(b - a, n);

        if (s >= 0 && s <= 1)
        {
            x = a + (b - a) * s;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns true of the point `p` is on the left side of the directed line segment `i` -> `j`
    /// Use for checking if a point is inside of a triangle. Since triangle vertices oriented
    /// CCW, a point on the left side of a triangle edge is "inside" that edge of the triangle.
    /// </summary>
    /// <param name="p">Index of test point in `points` array</param>
    /// <param name="i">Index of first vertex of the edge in the `points` array</param>
    /// /// <param name="j">Index of second vertex of the edge in the `points` array</param>
    /// <returns>True if the point `p` is on the left side of the line `i`->`j`</returns>
    public static bool IsPointOnRightSideOfLine(Vector2 a, Vector2 b, Vector2 c)
    {
        // The <= is essential; if it is <, the whole thing falls apart
        return ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x)) <= 0;
    }

}
