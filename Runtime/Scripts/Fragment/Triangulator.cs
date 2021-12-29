using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Logic for triangulating a set of 3D points. Only supports convex polygons.
/// </summary>
public class Triangulator
{
    // Constants for triangulation array indices
    protected const int V1 = 0; // Vertex 1
    protected const int V2 = 1; // Vertex 2
    protected const int V3 = 2; // Vertex 3
    protected const int E12 = 3; // Adjacency data for edge (V1 -> V2)
    protected const int E23 = 4; // Adjacency data for edge (V2 -> V3)
    protected const int E31 = 5; // Adjacency data for edge (V3 -> V1)

    // Index for super triangle
    protected const int SUPERTRIANGLE = 0;

    // Index for out of bounds triangle (boundary edge)
    protected const int OUT_OF_BOUNDS = -1;

    // Number of points to be triangulated (excluding super triangle vertices)
    protected int N;

    // Total number of triangles generated during triangulation
    protected int triangleCount;

    // Triangle vertex and adjacency data
    // Index 0 = Triangle index
    // Index 1 = [V1, V2, V3, E12, E23, E32]
    protected int[, ] triangulation;

    // Points on the plane to triangulate
    public TriangulationPoint[] points;

    // Array which tracks which triangles should be ignored in the final triangulation
    protected bool[] skipTriangle;

    // Normal of the plane on which the points lie
    protected Vector3 normal;

    // Normalization scale factor
    public float normalizationScaleFactor = 1f;

    /// <summary>
    /// Initializes the triangulator with the vertex data to be triangulated
    /// </summary>
    /// <param name="inputPoints">The points to triangulate</param>
    /// <param name="normal">The normal of the triangulation plane</param>
    public Triangulator(List<MeshVertex> inputPoints, Vector3 normal)
    {
        // Need at least three input vertices to triangulate
        if (inputPoints == null || inputPoints.Count < 3)
        {
            return;
        }

        this.N = inputPoints.Count;
        this.triangleCount = 2 * N + 1;
        this.triangulation = new int[triangleCount, 6];
        this.skipTriangle = new bool[triangleCount];
        this.points = new TriangulationPoint[N + 3]; // Extra 3 points used to store super triangle
        this.normal = normal;
        
        // Choose two points in the plane as one basis vector
        Vector3 e1 = (inputPoints[0].position - inputPoints[1].position).normalized;
        Vector3 e2 = normal.normalized;
        Vector3 e3 = Vector3.Cross(e1, e2).normalized;

        // To find the 2nd basis vector, find the largest component and swap with the smallest, negating the largest
        
        // Project 3D vertex onto the 2D plane
        for (int i = 0; i < N; i++)
        {
            var position = inputPoints[i].position;
            var coords = new Vector2(Vector3.Dot(position, e1), Vector3.Dot(position, e3));
            this.points[i] = new TriangulationPoint(i, coords);
        }
    }

    /// <summary>
    /// Performs the triangulation
    /// </summary>
    /// <returns>Returns an array containing the indices of the triangles, mapped to the list of points passed in during initialization</returns>
    public virtual int[] Triangulate()
    {
        // Need at least 3 vertices to triangulate
        if (N < 3) 
        {
            return new int[] { };
        }

        this.AddSuperTriangle();
        this.NormalizeCoordinates();
        this.ComputeTriangulation();
        this.DiscardTrianglesWithSuperTriangleVertices();

        List<int> triangles = new List<int>(3 * triangleCount);
        for (int i = 0; i < triangleCount; i++)
        {
            // Add all triangles that don't contain a super-triangle vertex
            if (!skipTriangle[i])
            {
                triangles.Add(triangulation[i, V1]);
                triangles.Add(triangulation[i, V2]);
                triangles.Add(triangulation[i, V3]);
            }
        }

        return triangles.ToArray();
    }

    /// <summary>
    /// Uniformly scales the 2D coordinates of all the points between [0, 1]
    /// </summary>
    protected void NormalizeCoordinates()
    {
        // 1) Normalize coordinates. Coordinates are scaled so they lie between 0 and 1
        // The scaling should be uniform so relative positions of points are unchanged

        float xMin = float.MaxValue;
        float xMax = float.MinValue;
        float yMin = float.MaxValue;
        float yMax = float.MinValue;

        // Find min/max points in the set
        for (int i = 0; i < N; i++)
        {
            var point = points[i];
            if (point.coords.x < xMin) xMin = point.coords.x;
            if (point.coords.y < yMin) yMin = point.coords.y;
            if (point.coords.x > xMax) xMax = point.coords.x;
            if (point.coords.y > yMax) yMax = point.coords.y;
        }

        // Normalization coefficient. Using same coefficient for both x & y
        // ensures uniform scaling
        normalizationScaleFactor = Mathf.Max(xMax - xMin, yMax - yMin);

        // Normalize each point
        for (int i = 0; i < N; i++)
        {
            var point = points[i];
            var normalizedPos = new Vector2(
                (point.coords.x - xMin) / normalizationScaleFactor,
                (point.coords.y - yMin) / normalizationScaleFactor);

            points[i].coords = normalizedPos;            
        }
    }

    /// <summary>
    /// Sorts the points into bins using an ordered grid
    /// </summary>
    /// <returns>Returns the array of sorted points</returns>
    protected TriangulationPoint[] SortPointsIntoBins()
    {
        // Compute the number of bins along each axis
        int n = Mathf.RoundToInt(Mathf.Pow((float) N, 0.25f));
        
        // Total bin count
        int binCount = n * n;

        // Assign bin numbers to each point by taking the normalized coordinates
        // and dividing them into a n x n grid.
        for (int k = 0; k < N; k++)
        {
            var point = this.points[k];
            int i = (int) (0.99f * n * point.coords.y);
            int j = (int) (0.99f * n * point.coords.x);
            point.bin = BinSort.GetBinNumber(i, j, n);
        }

        return BinSort.Sort<TriangulationPoint>(this.points, N, binCount);
    }

    /// <summary>
    /// Computes the triangulation of the point set.
    /// </summary>
    /// <returns>Returns true if the triangulation was successful</returns>
    protected bool ComputeTriangulation()
    {
        // Index of the current triangle being searched
        int tSearch = 0;
        // Index of the last triangle formed
        int tLast = 0;

        var sortedPoints = SortPointsIntoBins();

        // Loop through each point and insert it into the triangulation
        for (int i = 0; i < N; i++)
        {
            TriangulationPoint point = sortedPoints[i];

            // Insert new point into the triangulation. Start by finding the triangle that contains the point `p`
            // Keep track of how many triangles we visited in case search fails and we get stuck in a loop
            int counter = 0;
            bool pointInserted = false;
            while (!pointInserted)
            {
                if (counter++ > tLast || tSearch == OUT_OF_BOUNDS)
                {
                    break;
                }

                // Get coordinates of triangle vertices
                var v1 = this.points[triangulation[tSearch, V1]].coords;
                var v2 = this.points[triangulation[tSearch, V2]].coords;
                var v3 = this.points[triangulation[tSearch, V3]].coords;
                
                // Verify that point is on the correct side of each edge of the triangle.
                // If a point is on the left side of an edge, move to the adjacent triangle and check again. The search
                // continues until a containing triangle is found or the point is outside of all triangles
                if (!MathUtils.IsPointOnRightSideOfLine(v1, v2, point.coords))
                {
                    tSearch = triangulation[tSearch, E12];
                }
                else if (!MathUtils.IsPointOnRightSideOfLine(v2, v3, point.coords))
                {
                    tSearch = triangulation[tSearch, E23];
                }
                else if (!MathUtils.IsPointOnRightSideOfLine(v3, v1, point.coords))
                {
                    tSearch = triangulation[tSearch, E31];
                }
                // If it is on the right  side of all three edges, it is contained within the triangle (Unity uses CW winding). 
                else
                {
                    InsertPointIntoTriangle(point, tSearch, tLast);
                    tLast += 2;
                    tSearch = tLast;
                    pointInserted = true;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Initializes the triangulation by inserting the super triangle
    /// </summary>
    protected void AddSuperTriangle()
    {
        // Add new points to the end of the points array
        this.points[N] = new TriangulationPoint(N, new Vector2(-100f, -100f));
        this.points[N + 1] = new TriangulationPoint(N + 1, new Vector2(0f, 100f));
        this.points[N + 2] = new TriangulationPoint(N + 2, new Vector2(100f, -100f));

        // Store supertriangle in the first column of the vertex and adjacency data
        triangulation[SUPERTRIANGLE, V1] = N;
        triangulation[SUPERTRIANGLE, V2] = N + 1;
        triangulation[SUPERTRIANGLE, V3] = N + 2;

        // Zeros signify boundary edges
        triangulation[SUPERTRIANGLE, E12] = OUT_OF_BOUNDS;
        triangulation[SUPERTRIANGLE, E23] = OUT_OF_BOUNDS;
        triangulation[SUPERTRIANGLE, E31] = OUT_OF_BOUNDS;
    }

    /// <summary>
    /// Inserts the point `p` into triangle `t`, replacing it with three new triangles
    /// </summary>
    /// <param name="p">The index of the point to insert</param>
    /// <param name="t">The index of the triangle</param>
    /// <param name="triangleCount">Total number of triangles created so far</param>
    protected void InsertPointIntoTriangle(TriangulationPoint p, int t, int triangleCount)
    {
        //                         V1
        //                         *
        //                        /|\
        //                       /3|2\
        //                      /  |  \
        //                     /   |   \
        //                    /    |    \
        //                   /     |     \
        //                  /  t1  |  t3  \
        //                 /       |       \
        //                /      1 * 1      \
        //               /      __/1\__      \
        //              /    __/       \__    \
        //             / 2__/     t2      \__3 \
        //            / _/3                 2\_ \
        //           *---------------------------*
        //         V3                             V2

        int t1 = t;
        int t2 = triangleCount + 1;
        int t3 = triangleCount + 2;

        // Add the vertex & adjacency information for the two new triangles
        // New vertex is set to first vertex of each triangle to help with
        // restoring the triangulation later on
        triangulation[t2, V1] = p.index;
        triangulation[t2, V2] = triangulation[t, V2];
        triangulation[t2, V3] = triangulation[t, V3];

        triangulation[t2, E12] = t3;
        triangulation[t2, E23] = triangulation[t, E23];
        triangulation[t2, E31] = t1;

        triangulation[t3, V1] = p.index;
        triangulation[t3, V2] = triangulation[t, V1];
        triangulation[t3, V3] = triangulation[t, V2];

        triangulation[t3, E12] = t1;
        triangulation[t3, E23] = triangulation[t, E12];
        triangulation[t3, E31] = t2;

        // Triangle index remains the same for E12, no need to update adjacency
        UpdateAdjacency(triangulation[t, E12], t, t3);
        UpdateAdjacency(triangulation[t, E23], t, t2);

        // Replace existing triangle `t` with `t1`
        triangulation[t1, V2] = triangulation[t, V3];
        triangulation[t1, V3] = triangulation[t, V1];
        triangulation[t1, V1] = p.index;

        triangulation[t1, E23] = triangulation[t, E31];
        triangulation[t1, E12] = t2;
        triangulation[t1, E31] = t3;

        // After the triangles have been inserted, restore the Delauney triangulation
        RestoreDelauneyTriangulation(p, t1, t2, t3);
    }

    /// <summary>
    /// Restores the triangulation to a Delauney triangulation after new triangles have been added.
    /// </summary>
    /// <param name="p">Index of the inserted point</param>
    /// <param name="t1">Index of first triangle to check</param>
    /// <param name="t2">Index of second triangle to check</param>
    /// <param name="t3">Index of third triangle to check</param>
    protected void RestoreDelauneyTriangulation(TriangulationPoint p, int t1, int t2, int t3)
    {
        int t4;
        Stack < (int, int) > s = new Stack < (int, int) > ();

        s.Push((t1, triangulation[t1, E23]));
        s.Push((t2, triangulation[t2, E23]));
        s.Push((t3, triangulation[t3, E23]));
        
        while (s.Count > 0)
        {
            // Pop next triangle and its adjacent triangle off the stack
            // t1 contains the newly added vertex at V1
            // t2 is adjacent to t1 along the opposite edge of V1
            (t1, t2) = s.Pop();

            if (t2 == OUT_OF_BOUNDS)
            {
                continue;
            }
            // If t2 circumscribes p, the quadrilateral formed by t1+t2 has the
            // diagonal drawn in the wrong direction and needs to be swapped
            else if (SwapQuadDiagonalIfNeeded(p.index, t1, t2, out t3, out t4))
            {
                // Push newly formed triangles onto the stack to see if their diagonals
                // need to be swapped
                s.Push((t1, t3));
                s.Push((t2, t4));
            }
        }
    }

    /// <summary>
    /// Swaps the diagonal of the quadrilateral formed by triangle `t` and the
    /// triangle adjacent to the edge that is opposite of the newly added point
    /// </summary>
    /// <param name="p">The index of the inserted point</param>
    /// <param name="t1">Index of the triangle containing p</param>
    /// <param name="t2">Index of the triangle opposite t1 that shares edge E23 with t1</param>
    /// <param name="t3">Index of triangle adjacent to t1 after swap</param>
    /// <param name="t4">Index of triangle adjacent to t2 after swap</param>
    /// <returns>Returns true if the swap was performed. If the swap was not
    /// performed (e.g. returns false), t3 and t4 are unused.
    /// </returns>
    protected bool SwapQuadDiagonalIfNeeded(int p, int t1, int t2, out int t3, out int t4)
    {
        // 1) Form quadrilateral from t1 + t2 (q0->q1->q2->q3)
        // 2) Swap diagonal between q1->q3 to q0->q2
        //
        //               BEFORE                            AFTER
        //  
        //                 q3                                q3
        //    *-------------*-------------*    *-------------*-------------*
        //     \           / \           /      \           /|\           / 
        //      \   t3    /   \   t4    /        \   t3    /3|2\   t4    /  
        //       \       /     \       /          \       /  |  \       /   
        //        \     /       \     /            \     /   |   \     /    
        //         \   /   t2    \   /              \   /    |    \   /     
        //          \ /           \ /                \ /     |     \ /     
        //        q1 *-------------*  q2           q1 * 2 t1 | t2 3 * q2
        //            \2         3/                    \     |     /        
        //             \         /                      \    |    /         
        //              \  t1   /                        \   |   /          
        //               \     /                          \  |  /          
        //                \   /                            \1|1/            
        //                 \1/                              \|/             
        //                  *  q4 == p                       *  q4 == p   
        //

        // Get the vertices of the quad. The new vertex is always located at V1 of the triangle
        int q4 = p;
        int q1, q2, q3;

        // Since t2 might be oriented in any direction, find which edge is adjacent to `t`
        // The 4th vertex of the quad will be opposite this edge. We also need the two triangles
        // t3 and t3 that are adjacent to t2 along the other edges since the adjacency information
        // needs to be updated for those triangles.
        if (triangulation[t2, E12] == t1)
        {
            q1 = triangulation[t2, V2];
            q2 = triangulation[t2, V1];
            q3 = triangulation[t2, V3];

            t3 = triangulation[t2, E23];
            t4 = triangulation[t2, E31];
        }
        else if (triangulation[t2, E23] == t1)
        {
            q1 = triangulation[t2, V3];
            q2 = triangulation[t2, V2];
            q3 = triangulation[t2, V1];

            t3 = triangulation[t2, E31];
            t4 = triangulation[t2, E12];
        }
        else // (triangulation[t2, E31] == t1)
        {
            q1 = triangulation[t2, V1];
            q2 = triangulation[t2, V3];
            q3 = triangulation[t2, V2];

            t3 = triangulation[t2, E12];
            t4 = triangulation[t2, E23];
        }

        // Perform test to see if p lies in the circumcircle of t2
        if (SwapTest(points[q1].coords, points[q2].coords, points[q3].coords, points[q4].coords))
        {
            // Update adjacency for triangles adjacent to t1 and t2
            UpdateAdjacency(t3, t2, t1);
            UpdateAdjacency(triangulation[t1, E31], t1, t2);

            // Perform the swap. As always, put the new vertex as the first vertex of the triangle
            triangulation[t1, V1] = q4;
            triangulation[t1, V2] = q1;
            triangulation[t1, V3] = q3;

            triangulation[t2, V1] = q4;
            triangulation[t2, V2] = q3;
            triangulation[t2, V3] = q2;

            // Update adjacency information (order of operations is important here since we
            // are overwriting data).
            triangulation[t2, E12] = t1;
            triangulation[t2, E23] = t4;
            triangulation[t2, E31] = triangulation[t1, E31];

            // triangulation[t1, E12] = t2;
            triangulation[t1, E23] = t3;
            triangulation[t1, E31] = t2;

            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Marks any triangles that contain super-triangle vertices as discarded
    /// </summary>
    protected void DiscardTrianglesWithSuperTriangleVertices()
    {
        for (int i = 0; i < triangleCount; i++)
        {
            // Add all triangles that don't contain a super-triangle vertex
            if (TriangleContainsVertex(i, N) || 
                TriangleContainsVertex(i, N + 1) || 
                TriangleContainsVertex(i, N + 2))
            {
                skipTriangle[i] = true;
            }
        }
    }

    /// <summary>
    /// Checks to see if the triangle formed by points v1->v2->v3 circumscribes point vP
    /// </summary>
    /// <param name="v1">Coordinates of 1st vertex of triangle</param>
    /// <param name="v2">Coordinates of 2nd vertex of triangle</param>
    /// <param name="v3">Coordinates of 3rd vertex of triangle</param>
    /// <param name="v4">Coordinates of test point</param>
    /// <returns> Returns true if the triangle `t` circumscribes the point `p`</returns>
    protected bool SwapTest(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 v4)
    {
        float x13 = v1.x - v3.x;
        float x23 = v2.x - v3.x;
        float y13 = v1.y - v3.y;
        float y23 = v2.y - v3.y;
        float x14 = v1.x - v4.x;
        float x24 = v2.x - v4.x;
        float y14 = v1.y - v4.y;
        float y24 = v2.y - v4.y;

        float cosA = x13 * x23 + y13 * y23;
        float cosB = x24 * x14 + y24 * y14;

        if (cosA >= 0 && cosB >= 0)
        {
            return false;
        }
        else if (cosA < 0 && cosB < 0)
        {
            return true;
        }
        else
        {
            float sinA = (x13 * y23 - x23 * y13);
            float sinB = (x24 * y14 - x14 * y24);
            float sinAB = sinA * cosB + sinB * cosA;
            return sinAB < 0;
        }
    }

    /// <summary>
    /// Checks if the triangle `t` contains the specified vertex
    /// </summary>
    /// <param name="t">The index of the triangle</param>
    /// <param name="v">The index of the vertex</param>
    /// <returns>Returns true if the triangle `t` contains the vertex `v`</returns>
    protected bool TriangleContainsVertex(int t, int v)
    {
        return triangulation[t, V1] == v || triangulation[t, V2] == v || triangulation[t, V3] == v;
    }

    /// <summary>
    /// Updates the adjacency information in triangle `t`. Any references to `tOld are
    /// replaced with `tNew`
    /// </summary>
    /// <param name="t">The index of the triangle to update</param>
    /// <param name="tOld">The index to be replaced</param>
    /// <param name="tNew">The new index to replace with</param>
    protected void UpdateAdjacency(int t, int tOld, int tNew)
    {
        // Boundary edge, no triangle exists
        int sharedEdge;
        if (t == OUT_OF_BOUNDS)
        {
            return;
        }
        else if (FindSharedEdge(t, tOld, out sharedEdge))
        {
            triangulation[t, sharedEdge] = tNew;
        }
    }

    /// <summary>
    /// Finds the edge index for triangle `tOrigin` that is adjacent to triangle `tAdjacent`
    /// </summary>
    /// <param name="tOrigin">The origin triangle to search</param>
    /// <param name="tAdjacent">The triangle index to search for</param>
    /// <param name="edgeIndex">Edge index returned as an out parameter</param>
    /// <returns>True if `tOrigin` is adjacent to `tAdjacent` and supplies the
    /// shared edge index via the out parameter. If `tOrigin` is an invalid index or
    /// `tAdjacent` is not adjacent to `tOrigin`, returns false.</returns>
    protected bool FindSharedEdge(int tOrigin, int tAdjacent, out int edgeIndex)
    {
        edgeIndex = 0;

        if (tOrigin == OUT_OF_BOUNDS)
        {
            return false;
        }
        else if (triangulation[tOrigin, E12] == tAdjacent)
        {
            edgeIndex = E12;
            return true;
        }
        else if (triangulation[tOrigin, E23] == tAdjacent)
        {
            edgeIndex = E23;
            return true;
        }
        else if (triangulation[tOrigin, E31] == tAdjacent)
        {
            edgeIndex = E31;
            return true;
        }
        else
        {
            return false;
        }
    }
}