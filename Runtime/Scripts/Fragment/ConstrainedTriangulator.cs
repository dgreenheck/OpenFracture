using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for triangulating a set of 3D points with edge constraints. Supports convex and non-convex polygons
/// as well as polygons with holes.
/// </summary>
public sealed class ConstrainedTriangulator : Triangulator
{
    /// <summary>
    /// Given an edge E12, E23, E31, this returns the first vertex for that edge (V1, V2, V3, respectively)
    /// </summary>
    /// <value></value>
    private static readonly int[] edgeVertex1 = new int[] { 0, 0, 0, V1, V2, V3 };

    /// <summary>
    /// Given an edge E12, E23, E31, this returns the second vertex for that edge (V2, V3, V1, respectively)
    /// </summary>
    /// <value></value>
    private static readonly int[] edgeVertex2 = new int[] { 0, 0, 0, V2, V3, V1 };

    /// <summary>
    /// Given an edge E12, E23, E31, this returns the vertex opposite that edge (V3, V1, V2, respectively)
    /// </summary>
    /// <value></value>
    private static readonly int[] oppositePoint = new int[] { 0, 0, 0, V3, V1, V2 };

    /// <summary>
    /// Given an edge E12, E23, E31, this returns the next clockwise edge (E23, E31, E12, respectively)
    /// </summary>
    /// <value></value>
    private static readonly int[] nextEdge = new int[] { 0, 0, 0, E23, E31, E12 };

    /// <summary>
    /// Given an edge E12, E23, E31, this returns the previous clockwise edge (E31, E12, E23, respectively)
    /// </summary>
    /// <value></value>
    private static readonly int[] previousEdge = new int[] { 0, 0, 0, E31, E12, E23 };

    /// <summary>
    /// List of edge constraints provided during initialization
    /// </summary>
    private List<EdgeConstraint> constraints;

    /// <summary>
    /// This array maps each vertex to a triangle in the triangulation that contains it. This helps
    /// speed up the search when looking for intersecting edge. It isn't necessary to keep track of
    /// every triangle for each vertex.
    /// </summary>
    private int[] vertexTriangles;

    /// <summary>
    /// Flag for each triangle to track whether it has been visited or not when finding the starting edge.
    /// Define at the class level to prevent unnecessary GC when calling FindStartingEdge multiple times.
    /// </summary>
    private bool[] visited;

    /// <summary>
    /// Initializes the triangulator with the vertex data to be triangulated given a set of edge constraints
    /// </summary>
    /// <param name="inputPoints">The of points to triangulate.</param>
    /// <param name="constraints">The list of edge constraints which defines how the vertices in `inputPoints` are connected.</param>
    /// <param name="normal">The normal of the plane in which the `inputPoints` lie.</param>
    /// <returns></returns>
    public ConstrainedTriangulator(List<MeshVertex> inputPoints, List<EdgeConstraint> constraints, Vector3 normal)
        : base(inputPoints, normal)
    {
        this.constraints = constraints;
    }

    /// <summary>
    /// Calculates the triangulation
    /// </summary>
    /// <returns>Returns an array containing the indices of the triangles, mapped to the list of points passed in during initialization.</returns>
    public override int[] Triangulate()
    {
        // Need at least 3 vertices to triangulate
        if (N < 3)
        {
            return new int[] { };
        }

        this.AddSuperTriangle();
        this.NormalizeCoordinates();
        this.ComputeTriangulation();

        if (constraints.Count > 0)
        {
            this.ApplyConstraints();
            this.DiscardTrianglesViolatingConstraints();
        }

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
    /// Applys the edge constraints to the triangulation
    /// </summary>
    internal void ApplyConstraints()
    {
        visited = new bool[triangulation.GetLength(0)];

        // Map each vertex to a triangle that contains it
        vertexTriangles = new int[N + 3];
        for (int i = 0; i < triangulation.GetLength(0); i++)
        {
            vertexTriangles[triangulation[i, V1]] = i;
            vertexTriangles[triangulation[i, V2]] = i;
            vertexTriangles[triangulation[i, V3]] = i;
        }

        // Loop through each edge constraint
        foreach (EdgeConstraint constraint in constraints)
        {
            if (constraint.v1 == constraint.v2) continue;

            // We find the edges of the triangulation that intersect the constraint edge and remove them
            // For each intersecting edge, we identify the triangles that share that edge (which form a quad)
            // The diagonal of this quad is flipped.
            Queue<EdgeConstraint> intersectingEdges = FindIntersectingEdges(constraint, vertexTriangles);
            RemoveIntersectingEdges(constraint, intersectingEdges);
        }
    }

    /// <summary>
    /// Searches through the triangulation to find intersecting edges
    /// </summary>
    /// <param name="intersectingEdges"></param>
    internal Queue<EdgeConstraint> FindIntersectingEdges(EdgeConstraint constraint, int[] vertexTriangles)
    {
        Queue<EdgeConstraint> intersectingEdges = new Queue<EdgeConstraint>();

        // Need to find the first edge that the constraint crosses.
        EdgeConstraint startEdge;
        if (FindStartingEdge(vertexTriangles, constraint, out startEdge))
        {
            intersectingEdges.Enqueue(startEdge);
        }
        else
        {
            return intersectingEdges;
        }

        // Search for all triangles that intersect the constraint. Stop when we find a triangle that contains v_j
        int t = startEdge.t1;
        int edgeIndex = startEdge.t1Edge;
        int lastTriangle = t;
        bool finalTriangleFound = false;
        while (!finalTriangleFound)
        {
            // Cross the last intersecting edge and inspect the next triangle
            lastTriangle = t;
            t = triangulation[t, edgeIndex];

            // Get coordinates of constraint end points and triangle vertices
            Vector2 v_i = points[constraint.v1].coords;
            Vector2 v_j = points[constraint.v2].coords;
            Vector2 v1 = points[triangulation[t, V1]].coords;
            Vector2 v2 = points[triangulation[t, V2]].coords;
            Vector2 v3 = points[triangulation[t, V3]].coords;

            // If triangle contains the endpoint of the constraint, the search is done
            if (TriangleContainsVertex(t, constraint.v2))
            {
                finalTriangleFound = true;
            }
            // Otherwise, the constraint must intersect one edge of this triangle. Ignore the edge that we entered from
            else if ((triangulation[t, E12] != lastTriangle) && MathUtils.LinesIntersect(v_i, v_j, v1, v2))
            {
                edgeIndex = E12;
                var edge = new EdgeConstraint(triangulation[t, V1], triangulation[t, V2], t, triangulation[t, E12], edgeIndex);
                intersectingEdges.Enqueue(edge);
            }
            else if ((triangulation[t, E23] != lastTriangle) && MathUtils.LinesIntersect(v_i, v_j, v2, v3))
            {
                edgeIndex = E23;
                var edge = new EdgeConstraint(triangulation[t, V2], triangulation[t, V3], t, triangulation[t, E23], edgeIndex);
                intersectingEdges.Enqueue(edge);
            }
            else if ((triangulation[t, E31] != lastTriangle) && MathUtils.LinesIntersect(v_i, v_j, v3, v1))
            {
                edgeIndex = E31;
                var edge = new EdgeConstraint(triangulation[t, V3], triangulation[t, V1], t, triangulation[t, E31], edgeIndex);
                intersectingEdges.Enqueue(edge);
            }
            else
            {
                // Shouldn't reach this point
                Debug.LogWarning("Failed to find final triangle, exiting early.");
                break;
            }
        }

        return intersectingEdges;
    }

    /// <summary>
    /// Finds the starting edge for the search to find all edges that intersect the constraint
    /// </summary>
    /// <param name="constraint">The constraint being used to check for intersections</param>
    internal bool FindStartingEdge(int[] vertexTriangles, EdgeConstraint constraint, out EdgeConstraint startingEdge)
    {
        // Initialize out parameter to default value
        startingEdge = new EdgeConstraint(-1, -1);

        // v_i->v_j are the start/end points of the constraint, respectively
        int v_i = constraint.v1;
        int v_j = constraint.v2;

        // Start the search with an initial triangle that contains v_i
        int tSearch = vertexTriangles[v_i];

        // Reset visited states
        for (int i = 0; i < visited.Length; i++)
        {
            visited[i] = false;
        }

        // Circle v_i until we find a triangle that contains an edge which intersects the constraint edge
        // This will be the starting triangle in the search for finding all triangles that intersect the constraint
        bool intersectionFound = false;
        bool noCandidatesFound = false;
        int intersectingEdgeIndex = E12;
        int tE12, tE23, tE31;
        while (!intersectionFound && !noCandidatesFound)
        {
            visited[tSearch] = true;

            // Triangulation already contains the constraint so we ignore the constraint
            if (TriangleContainsConstraint(tSearch, constraint))
            {
                return false;
            }
            // Check if the constraint intersects any edges of this triangle
            else if (EdgeConstraintIntersectsTriangle(tSearch, constraint, out intersectingEdgeIndex))
            {
                intersectionFound = true;
                break;
            }

            tE12 = triangulation[tSearch, E12];
            tE23 = triangulation[tSearch, E23];
            tE31 = triangulation[tSearch, E31];

            // If constraint does not intersect this triangle, check adjacent triangles by crossing edges that have v_i as a vertex
            // Avoid triangles that we have previously visited in the search
            if (tE12 != OUT_OF_BOUNDS && !visited[tE12] && TriangleContainsVertex(tE12, v_i))
            {
                tSearch = tE12;
            }
            else if (tE23 != OUT_OF_BOUNDS && !visited[tE23] && TriangleContainsVertex(tE23, v_i))
            {
                tSearch = tE23;
            }
            else if (tE31 != OUT_OF_BOUNDS && !visited[tE31] && TriangleContainsVertex(tE31, v_i))
            {
                tSearch = tE31;
            }
            else
            {
                noCandidatesFound = true;
                break;
            }
        }
        
        if (intersectionFound)
        {
            int v_k = triangulation[tSearch, edgeVertex1[intersectingEdgeIndex]];
            int v_l = triangulation[tSearch, edgeVertex2[intersectingEdgeIndex]];
            int triangle2 = triangulation[tSearch, intersectingEdgeIndex];
            startingEdge = new EdgeConstraint(v_k, v_l, tSearch, triangle2, intersectingEdgeIndex);

            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Remove the edges from the triangulation that intersect the constraint. Find two triangles that
    /// share the intersecting edge, swap the diagonal and repeat until no edges intersect the constraint.
    /// </summary>
    /// <param name="constraint">The constraint to check against</param>
    /// <param name="intersectingEdges">A queue containing the previously found edges that intersect the constraint</param>
    internal void RemoveIntersectingEdges(EdgeConstraint constraint, Queue<EdgeConstraint> intersectingEdges)
    {
        // Remove intersecting edges. Keep track of the new edges that we create
        List<EdgeConstraint> newEdges = new List<EdgeConstraint>();
        EdgeConstraint edge, newEdge;

        // Mark the number of times we have been through the loop. If no new edges
        // have been added after all edges have been visited, stop the loop. Every 
        // time an edge is added to newEdges, reset the counter.
        int counter = 0;

        // Loop through all intersecting edges until they have been properly resolved
        // or they have all been visited with no diagonal swaps.
        while (intersectingEdges.Count > 0 && counter <= intersectingEdges.Count)
        {
            edge = intersectingEdges.Dequeue();

            Quad quad;
            if (FindQuadFromSharedEdge(edge.t1, edge.t1Edge, out quad))
            {
                // If the quad is convex, we swap the diagonal (a quad is convex if the diagonals intersect)
                // Otherwise push it back into the queue so we can swap the diagonal later on.
                if (MathUtils.LinesIntersect(points[quad.q4].coords,
                        points[quad.q3].coords,
                        points[quad.q1].coords,
                        points[quad.q2].coords))
                {
                    // Swap diagonals of the convex quads whose diagonals intersect the constraint
                    SwapQuadDiagonal(quad, intersectingEdges, newEdges, constraints);

                    // The new diagonal is between Q3 and Q4
                    newEdge = new EdgeConstraint(quad.q3, quad.q4, quad.t1, quad.t2, E31);

                    // If the new diagonal still intersects the constraint edge v_i->v_j,
                    // put back on the list of intersecting eddges
                    if (MathUtils.LinesIntersect(points[constraint.v1].coords,
                            points[constraint.v2].coords,
                            points[quad.q3].coords,
                            points[quad.q4].coords))
                    {
                        intersectingEdges.Enqueue(newEdge);
                    }
                    // Otherwise record in list of new edges
                    else
                    {
                        counter = 0;
                        newEdges.Add(newEdge);
                    }
                }
                else
                {
                    intersectingEdges.Enqueue(edge);
                }
            }

            counter++;
        }

        // If any new edges were formed due to a diagonal being swapped, restore the Delauney condition
        // of the triangulation while respecting the constraints
        if (newEdges.Count > 0)
        {
            RestoreConstrainedDelauneyTriangulation(constraint, newEdges);
        }
    }

    /// <summary>
    /// Restores the Delauney triangulation after the constraint has been inserted
    /// </summary>
    /// <param name="constraint">The constraint that was added to the triangulation</param>
    /// <param name="newEdges">The list of new edges that were added</param>
    internal void RestoreConstrainedDelauneyTriangulation(EdgeConstraint constraint, List<EdgeConstraint> newEdges)
    {
        // Iterate over the list of newly created edges and swap non-constraint diagonals until no more swaps take place
        bool swapOccurred = true;
        int counter = 0;
        while (swapOccurred)
        {
            counter++;
            swapOccurred = false;

            for (int i = 0; i < newEdges.Count; i++)
            {
                EdgeConstraint edge = newEdges[i];

                // If newly added edge is equal to constraint, we don't want to flip this edge so skip it
                if (edge == constraint)
                {
                    continue;
                }

                Quad quad;
                if (FindQuadFromSharedEdge(edge.t1, edge.t1Edge, out quad))
                {
                    if (SwapTest(points[quad.q1].coords, points[quad.q2].coords, points[quad.q3].coords, points[quad.q4].coords))
                    {
                        SwapQuadDiagonal(quad, newEdges, constraints, null);

                        // Enqueue the new diagonal
                        int v_m = quad.q3;
                        int v_n = quad.q4;
                        newEdges[i] = new EdgeConstraint(v_m, v_n, quad.t1, quad.t2, E31);

                        swapOccurred = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Discards triangles that violate the any of the edge constraints
    /// </summary>
    internal void DiscardTrianglesViolatingConstraints()
    {
        // Initialize to all triangles being skipped
        for (int i = 0; i < triangleCount; i++)
        {
            skipTriangle[i] = true;
        }

        // Identify the boundary edges
        HashSet < (int, int) > boundaries = new HashSet < (int, int) > ();
        for (int i = 0; i < this.constraints.Count; i++)
        {
            EdgeConstraint constraint = this.constraints[i];
            boundaries.Add((constraint.v1, constraint.v2));
        }

        // Reset visited states
        for (int i = 0; i < visited.Length; i++)
        {
            visited[i] = false;
        }

        // Search frontier
        Queue<int> frontier = new Queue<int>();

        int v1, v2, v3;
        bool boundaryE12, boundaryE23, boundaryE31;
        for (int i = 0; i < triangleCount; i++)
        {
            // If we've already visited this triangle, skip it
            if (visited[i])
            {
                continue;
            }

            v1 = triangulation[i, V1];
            v2 = triangulation[i, V2];
            v3 = triangulation[i, V3];
            boundaryE12 = boundaries.Contains((v1, v2));
            boundaryE23 = boundaries.Contains((v2, v3));
            boundaryE31 = boundaries.Contains((v3, v1));

            // If this triangle has a boundary edge, start searching for adjacent triangles
            if (boundaryE12 || boundaryE23 || boundaryE31)
            {
                skipTriangle[i] = false;

                // Search along edges that are not boundary edges
                frontier.Clear();
                if (!boundaryE12)
                {
                    frontier.Enqueue(triangulation[i, E12]);
                }
                if (!boundaryE23)
                {
                    frontier.Enqueue(triangulation[i, E23]);
                }
                if (!boundaryE31)
                {
                    frontier.Enqueue(triangulation[i, E31]);
                }

                // Recursively search along all non-boundary edges, marking the
                // adjacent triangles as "keep"
                while (frontier.Count > 0)
                {
                    int k = frontier.Dequeue();

                    if (k == OUT_OF_BOUNDS || visited[k])
                    {
                        continue;
                    }

                    skipTriangle[k] = false;
                    visited[k] = true;

                    v1 = triangulation[k, V1];
                    v2 = triangulation[k, V2];
                    v3 = triangulation[k, V3];

                    // Continue searching along non-boundary edges
                    if (!boundaries.Contains((v1, v2)))
                    {
                        frontier.Enqueue(triangulation[k, E12]);
                    }
                    if (!boundaries.Contains((v2, v3)))
                    {
                        frontier.Enqueue(triangulation[k, E23]);
                    }
                    if (!boundaries.Contains((v3, v1)))
                    {
                        frontier.Enqueue(triangulation[k, E31]);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Determines if the triangle contains the edge constraint
    /// </summary>
    /// <param name="t">The triangle to test</param>
    /// <param name="constraint">The edge constraint</param>
    /// <returns>True if the triangle contains one or both of the endpoints of the constraint</returns>
    internal bool TriangleContainsConstraint(int t, EdgeConstraint constraint)
    {
        return (triangulation[t, V1] == constraint.v1 || triangulation[t, V2] == constraint.v1 || triangulation[t, V3] == constraint.v1) &&
               (triangulation[t, V1] == constraint.v2 || triangulation[t, V2] == constraint.v2 || triangulation[t, V3] == constraint.v2);
    }

    /// <summary>
    /// Returns true if the edge constraint intersects an edge of triangle `t`
    /// </summary>
    /// <param name="t">The triangle to test</param>
    /// <param name="constraint">The edge constraint</param>
    /// <param name="intersectingEdgeIndex">The index of the intersecting edge (E12, E23, E31)</param>
    /// <returns>Returns true if an intersection is found, otherwise false.</returns>
    internal bool EdgeConstraintIntersectsTriangle(int t, EdgeConstraint constraint, out int intersectingEdgeIndex)
    {
        Vector2 v_i = points[constraint.v1].coords;
        Vector2 v_j = points[constraint.v2].coords;
        Vector2 v1 = points[triangulation[t, V1]].coords;
        Vector2 v2 = points[triangulation[t, V2]].coords;
        Vector2 v3 = points[triangulation[t, V3]].coords;

        if (MathUtils.LinesIntersect(v_i, v_j, v1, v2))
        {
            intersectingEdgeIndex = E12;
            return true;
        }
        else if (MathUtils.LinesIntersect(v_i, v_j, v2, v3))
        {
            intersectingEdgeIndex = E23;
            return true;
        }
        else if (MathUtils.LinesIntersect(v_i, v_j, v3, v1))
        {
            intersectingEdgeIndex = E31;
            return true;
        }
        else
        {
            intersectingEdgeIndex = -1;
            return false;
        }
    }

    /// <summary>
    /// Returns the quad formed by triangle `t1` and the other triangle that shares the intersecting edge
    /// </summary>
    /// <param name="t1">Base triangle</param>
    /// <param name="intersectingEdge">Edge index that is being intersected</param>
    internal bool FindQuadFromSharedEdge(int t1, int t1SharedEdge, out Quad quad)
    {
        //               q3        
        //      *---------*---------*
        //       \       / \       /
        //        \ t2L /   \ t2R /
        //         \   /     \   /
        //          \ /   t2  \ /
        //        q1 *---------* q2 
        //          / \   t1  / \    
        //         /   \     /   \     
        //        / t1L \   / t1R \   
        //       /       \ /       \  
        //      *---------*---------*
        //               q4             

        int q1, q2, q3, q4;
        int t1L, t1R, t2L, t2R;

        // t2 is adjacent to t1 along t1Edge
        int t2 = triangulation[t1, t1SharedEdge];
        int t2SharedEdge;
        if (FindSharedEdge(t2, t1, out t2SharedEdge))
        {
            // Get the top 3 vertices of the quad from t2
            if (t2SharedEdge == E12)
            {
                q2 = triangulation[t2, V1];
                q1 = triangulation[t2, V2];
                q3 = triangulation[t2, V3];
            }
            else if (t2SharedEdge == E23)
            {
                q2 = triangulation[t2, V2];
                q1 = triangulation[t2, V3];
                q3 = triangulation[t2, V1];
            }
            else // (t2SharedEdge == E31)
            {
                q2 = triangulation[t2, V3];
                q1 = triangulation[t2, V1];
                q3 = triangulation[t2, V2];
            }

            // q4 is the point in t1 opposite of the shared edge
            q4 = triangulation[t1, oppositePoint[t1SharedEdge]];

            // Get the adjacent triangles to make updating adjacency easier
            t1L = triangulation[t1, previousEdge[t1SharedEdge]];
            t1R = triangulation[t1, nextEdge[t1SharedEdge]];
            t2L = triangulation[t2, nextEdge[t2SharedEdge]];
            t2R = triangulation[t2, previousEdge[t2SharedEdge]];

            quad = new Quad(q1, q2, q3, q4, t1, t2, t1L, t1R, t2L, t2R);

            return true;
        }

        quad = new Quad();

        return false;
    }

    /// <summary>
    /// Swaps the diagonal of the quadrilateral q0->q1->q2->q3 formed by t1 and t2
    /// </summary>
    /// <param name="">The quad that will have its diagonal swapped</param>
    internal void SwapQuadDiagonal(Quad quad, IEnumerable<EdgeConstraint> edges1, IEnumerable<EdgeConstraint> edges2, IEnumerable<EdgeConstraint> edges3)
    {
        // BEFORE
        //               q3        
        //      *---------*---------*
        //       \       / \       /
        //        \ t2L /   \ t2R /
        //         \   /     \   /
        //          \ /   t2  \ /
        //        q1 *---------* q2 
        //          / \   t1  / \    
        //         /   \     /   \     
        //        / t1L \   / t1R \   
        //       /       \ /       \  
        //      *---------*---------*
        //               q4           

        // AFTER
        //               q3        
        //      *---------*---------*
        //       \       /|\       /
        //        \ t2L / | \ t2R /
        //         \   /  |  \   /
        //          \ /   |   \ /
        //        q1 * t1 | t2 * q2 
        //          / \   |   / \    
        //         /   \  |  /   \     
        //        / t1L \ | / t1R \   
        //       /       \|/       \  
        //      *---------*---------*
        //               q4      

        int t1 = quad.t1;
        int t2 = quad.t2;
        int t1R = quad.t1R;
        int t1L = quad.t1L;
        int t2R = quad.t2R;
        int t2L = quad.t2L;

        // Perform the swap. As always, put the new vertex as the first vertex of the triangle
        triangulation[t1, V1] = quad.q4;
        triangulation[t1, V2] = quad.q1;
        triangulation[t1, V3] = quad.q3;

        triangulation[t2, V1] = quad.q4;
        triangulation[t2, V2] = quad.q3;
        triangulation[t2, V3] = quad.q2;

        triangulation[t1, E12] = t1L;
        triangulation[t1, E23] = t2L;
        triangulation[t1, E31] = t2;

        triangulation[t2, E12] = t1;
        triangulation[t2, E23] = t2R;
        triangulation[t2, E31] = t1R;

        // Update adjacency for the adjacent triangles
        UpdateAdjacency(t2L, t2, t1);
        UpdateAdjacency(t1R, t1, t2);

        // Now that triangles have moved, need to update edges as well
        UpdateEdgesAfterSwap(edges1, t1, t2, t1L, t1R, t2L, t2R);
        UpdateEdgesAfterSwap(edges2, t1, t2, t1L, t1R, t2L, t2R);
        UpdateEdgesAfterSwap(edges3, t1, t2, t1L, t1R, t2L, t2R);

        // Also need to update the vertexTriangles array since the vertices q1 and q2
        // may have been referencing t2/t1 respectively and they are no longer.
        vertexTriangles[quad.q1] = t1;
        vertexTriangles[quad.q2] = t2;
    }

    /// <summary>
    /// Update the Edges
    /// </summary>
    /// <param name="edges"></param>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <param name="t1L"></param>
    /// <param name="t1R"></param>
    /// <param name="t2L"></param>
    /// <param name="t2R"></param>
    internal void UpdateEdgesAfterSwap(IEnumerable<EdgeConstraint> edges, int t1, int t2, int t1L, int t1R, int t2L, int t2R)
    {
        if (edges == null)
        {
            return;
        }

        // Update edges to reflect changes in triangles
        foreach (EdgeConstraint edge in edges)
        {
            if (edge.t1 == t1 && edge.t2 == t1R)
            {
                edge.t1 = t2;
                edge.t2 = t1R;
                edge.t1Edge = E31;
            }
            else if (edge.t1 == t1 && edge.t2 == t1L)
            {
                // Triangles stay the same
                edge.t1Edge = E12;
            }
            else if (edge.t1 == t1R && edge.t2 == t1)
            {
                edge.t2 = t2;
            }
            else if (edge.t1 == t1L && edge.t2 == t1)
            {
                // Unchanged
            }
            else if (edge.t1 == t2 && edge.t2 == t2R)
            {
                // Triangles stay the same
                edge.t1Edge = E23;
            }
            else if (edge.t1 == t2 && edge.t2 == t2L)
            {
                edge.t1 = t1;
                edge.t2 = t2L;
                edge.t1Edge = E23;
            }
            else if (edge.t1 == t2R && edge.t2 == t2)
            {
                // Unchanged
            }
            else if (edge.t1 == t2L && edge.t2 == t2)
            {
                edge.t2 = t1;
            }
        }
    }
}