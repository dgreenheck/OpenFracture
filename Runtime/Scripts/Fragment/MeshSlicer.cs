using UnityEngine;

/// <summary>
/// Class which handles slicing a mesh into two pieces given the origin and normal of the slice plane.
/// </summary>
public static class MeshSlicer
{
    /// <summary>
    /// Slices the mesh by the plane specified by `sliceNormal` and `sliceOrigin`
    /// The sliced mesh data is return via out parameters.
    /// </summary>
    /// <param name="meshData"></param>
    /// <param name="sliceNormal">The normal of the slice plane (points towards the top slice)</param>
    /// <param name="sliceOrigin">The origin of the slice plane</param>
    /// <param name="textureScale">Scale factor to apply to UV coordinates</param>
    /// <param name="textureOffset">Offset to apply to UV coordinates</param>
    /// <param name="topSlice">Out parameter returning fragment mesh data for slice above the plane</param>
    /// <param name="bottomSlice">Out parameter returning fragment mesh data for slice below the plane</param>
    public static void Slice(FragmentData meshData,
                             Vector3 sliceNormal,
                             Vector3 sliceOrigin,
                             Vector2 textureScale,
                             Vector2 textureOffset,
                             out FragmentData topSlice,
                             out FragmentData bottomSlice)
    {
        topSlice = new FragmentData(meshData.vertexCount, meshData.triangleCount);
        bottomSlice = new FragmentData(meshData.vertexCount, meshData.triangleCount);

        // Keep track of what side of the cutting plane each vertex is on
        bool[] side = new bool[meshData.vertexCount];

        // Go through and identify which vertices are above/below the split plane
        for (int i = 0; i < meshData.Vertices.Count; i++)
        {
            var vertex = meshData.Vertices[i];
            side[i] = vertex.position.IsAbovePlane(sliceNormal, sliceOrigin);
            var slice = side[i] ? topSlice : bottomSlice;
            slice.AddMappedVertex(vertex, i);
        }

        int offset = meshData.Vertices.Count;
        for (int i = 0; i < meshData.CutVertices.Count; i++)
        {
            var vertex = meshData.CutVertices[i];
            side[i + offset] = vertex.position.IsAbovePlane(sliceNormal, sliceOrigin);
            var slice = side[i + offset] ? topSlice : bottomSlice;
            slice.AddMappedVertex(vertex, i + offset);
        }

        SplitTriangles(meshData, topSlice, bottomSlice, sliceNormal, sliceOrigin, side, SlicedMeshSubmesh.Default);
        SplitTriangles(meshData, topSlice, bottomSlice, sliceNormal, sliceOrigin, side, SlicedMeshSubmesh.CutFace);

        // Fill in the cut plane for each mesh.
        // The slice normal points to the "above" mesh, so the face normal for the cut face
        // on the above mesh is opposite of the slice normal. Conversely, normal for the
        // cut face on the "below" mesh is in the direction of the slice normal
        FillCutFaces(topSlice, bottomSlice, -sliceNormal, textureScale, textureOffset);
    }

    /// <summary>
    /// Fills the cut faces for each sliced mesh. The `sliceNormal` is the normal for the plane and points
    /// in the direction of `topMeshData`
    /// </summary>
    /// <param name="topSlice">Fragment mesh data for slice above the slice plane</param>
    /// <param name="bottomSlice">Fragment mesh data for slice above the slice plane</param>
    /// <param name="sliceNormal">Normal of the slice plane (points towards the top slice)</param>
    /// <param name="textureScale">Scale factor to apply to UV coordinates</param>
    /// <param name="textureOffset">Offset to apply to UV coordinates</param>
    private static void FillCutFaces(FragmentData topSlice,
                                     FragmentData bottomSlice,
                                     Vector3 sliceNormal,
                                     Vector2 textureScale,
                                     Vector2 textureOffset)
    {
        // Since the topSlice and bottomSlice both share the same cut face, we only need to calculate it
        // once. Then the same vertex/triangle data for the face will be used for both slices, except
        // with the normals reversed.

        // First need to weld the coincident vertices for the triangulation to work properly
        topSlice.WeldCutFaceVertices();

        // Need at least 3 vertices to triangulate
        if (topSlice.CutVertices.Count < 3) return;

        // Triangulate the cut face
        var triangulator = new ConstrainedTriangulator(topSlice.CutVertices, topSlice.Constraints, sliceNormal);
        int[] triangles = triangulator.Triangulate();

        // Update normal and UV for the cut face vertices
        for (int i = 0; i < topSlice.CutVertices.Count; i++)
        {
            var vertex = topSlice.CutVertices[i];
            var point = triangulator.points[i];

            // UV coordinates are based off of the 2D coordinates used for triangulation
            // During triangulation, coordinates are normalized to [0,1], so need to multiply
            // by normalization scale factor to get back to the appropritate scale
            Vector2 uv = new Vector2(
                (triangulator.normalizationScaleFactor * point.coords.x) * textureScale.x + textureOffset.x,
                (triangulator.normalizationScaleFactor * point.coords.y) * textureScale.y + textureOffset.y);

            // Update normals and UV coordinates for the cut vertices
            var topVertex = vertex;
            topVertex.normal = sliceNormal;
            topVertex.uv = uv;

            var bottomVertex = vertex;
            bottomVertex.normal = -sliceNormal;
            bottomVertex.uv = uv;

            topSlice.CutVertices[i] = topVertex;
            bottomSlice.CutVertices[i] = bottomVertex;
        }

        // Add the new triangles to the top/bottom slices
        int offsetTop = topSlice.Vertices.Count;
        int offsetBottom = bottomSlice.Vertices.Count;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            topSlice.AddTriangle(
                offsetTop + triangles[i],
                offsetTop + triangles[i + 1],
                offsetTop + triangles[i + 2],
                SlicedMeshSubmesh.CutFace);

            bottomSlice.AddTriangle(
                offsetBottom + triangles[i],
                offsetBottom + triangles[i + 2], // Swap two vertices so triangles are wound CW
                offsetBottom + triangles[i + 1],
                SlicedMeshSubmesh.CutFace);
        }
    }

    /// <summary>
    /// Identifies triangles that are intersected by the slice plane and splits them in two
    /// </summary>
    /// <param name="meshData"></param>
    /// <param name="topSlice">Fragment mesh data for slice above the slice plane</param>
    /// <param name="bottomSlice">Fragment mesh data for slice above the slice plane</param>
    /// <param name="sliceNormal">The normal of the slice plane (points towards the top slice)</param>
    /// <param name="sliceOrigin">The origin of the slice plane</param>
    /// <param name="side">Array mapping each vertex to either the top/bottom slice</param>
    /// <param name="subMesh">Index of the sub mesh</param>
    private static void SplitTriangles(FragmentData meshData,
                                       FragmentData topSlice,
                                       FragmentData bottomSlice,
                                       Vector3 sliceNormal,
                                       Vector3 sliceOrigin,
                                       bool[] side,
                                       SlicedMeshSubmesh subMesh)
    {
        int[] triangles = meshData.GetTriangles((int)subMesh);

        // Keep track of vertices that lie on the intersection plane
        int a, b, c;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            // Get vertex indexes for this triangle
            a = triangles[i];
            b = triangles[i + 1];
            c = triangles[i + 2];

            // Triangle is contained completely within mesh A
            if (side[a] && side[b] && side[c])
            {
                topSlice.AddMappedTriangle(a, b, c, subMesh);
            }
            // Triangle is contained completely within mesh B
            else if (!side[a] && !side[b] && !side[c])
            {
                bottomSlice.AddMappedTriangle(a, b, c, subMesh);
            }
            // Triangle is intersected by the slicing plane. Need to subdivide it
            else
            {
                // In these cases, two vertices of the triangle are above the cut plane and one vertex is below
                if (side[b] && side[c] && !side[a])
                {
                    SplitTriangle(b, c, a, sliceNormal, sliceOrigin, meshData, topSlice, bottomSlice, subMesh, true);
                }
                else if (side[c] && side[a] && !side[b])
                {
                    SplitTriangle(c, a, b, sliceNormal, sliceOrigin, meshData, topSlice, bottomSlice, subMesh, true);
                }
                else if (side[a] && side[b] && !side[c])
                {
                    SplitTriangle(a, b, c, sliceNormal, sliceOrigin, meshData, topSlice, bottomSlice, subMesh, true);
                }
                // In these cases, two vertices of the triangle are below the cut plane and one vertex is above
                else if (!side[b] && !side[c] && side[a])
                {
                    SplitTriangle(b, c, a, sliceNormal, sliceOrigin, meshData, topSlice, bottomSlice, subMesh, false);
                }
                else if (!side[c] && !side[a] && side[b])
                {
                    SplitTriangle(c, a, b, sliceNormal, sliceOrigin, meshData, topSlice, bottomSlice, subMesh, false);
                }
                else if (!side[a] && !side[b] && side[c])
                {
                    SplitTriangle(a, b, c, sliceNormal, sliceOrigin, meshData, topSlice, bottomSlice, subMesh, false);
                }
            }
        }
    }

    /// <summary>
    /// Splits triangle defined by the points (v1,v2,v3)
    /// </summary>
    /// <param name="v1_idx">Index of first vertex in triangle</param>
    /// <param name="v2_idx">Index of second vertex in triangle<</param>
    /// <param name="v3_idx">Index of third vertex in triangle<</param>
    /// <param name="sliceNormal">The normal of the slice plane (points towards the top slice)</param>
    /// <param name="sliceOrigin">The origin of the slice plane</param>
    /// <param name="meshData">Original mesh data</param>
    /// <param name="topSlice">Mesh data for top slice</param>
    /// <param name="bottomSlice">Mesh data for bottom slice</param>
    /// <param name="subMesh">Index of the submesh that the triangle belongs to</param>
    /// <param name="v3BelowCutPlane">Boolean indicating whether v3 is above or below the slice plane.</param>                                             
    private static void SplitTriangle(int v1_idx,
                                      int v2_idx,
                                      int v3_idx,
                                      Vector3 sliceNormal,
                                      Vector3 sliceOrigin,
                                      FragmentData meshData,
                                      FragmentData topSlice,
                                      FragmentData bottomSlice,
                                      SlicedMeshSubmesh subMesh,
                                      bool v3BelowCutPlane)       
    {
        // - `v1`, `v2`, `v3` are the indexes of the triangle relative to the original mesh data
        // - `v1` and `v2` are on the the side of split plane that belongs to meshA
        // - `v3` is on the side of the split plane that belongs to meshB
        // - `vertices`, `normals`, `uv` are the original mesh data used for interpolation  
        //      
        // v3BelowCutPlane = true
        // ======================
        //                                
        //     v1 *_____________* v2   .
        //         \           /      /|\  cutNormal
        //          \         /        |
        //       ----*-------*---------*--
        //        v13 \     /  v23       cutOrigin
        //             \   /
        //              \ /
        //               *  v3         triangle normal out of screen                                                                                  
        //    
        // v3BelowCutPlane = false
        // =======================
        //
        //               *  v3         .                                             
        //              / \           /|\  cutNormal  
        //         v23 /   \ v13       |                    
        //       -----*-----*----------*--
        //           /       \         cut origin                                
        //          /         \                                                                  
        //      v2 *___________* v1    triangle normal out of screen
        //                 
        
        float s13;
        float s23;
        Vector3 v13;
        Vector3 v23;

        MeshVertex v1 = v1_idx < meshData.Vertices.Count ? meshData.Vertices[v1_idx] : meshData.CutVertices[v1_idx - meshData.Vertices.Count];
        MeshVertex v2 = v2_idx < meshData.Vertices.Count ? meshData.Vertices[v2_idx] : meshData.CutVertices[v2_idx - meshData.Vertices.Count];
        MeshVertex v3 = v3_idx < meshData.Vertices.Count ? meshData.Vertices[v3_idx] : meshData.CutVertices[v3_idx - meshData.Vertices.Count];

        if (MathUtils.LinePlaneIntersection(v1.position, v3.position, sliceNormal, sliceOrigin, out v13, out s13) &&
            MathUtils.LinePlaneIntersection(v2.position, v3.position, sliceNormal, sliceOrigin, out v23, out s23))
        {
            // Interpolate normals and UV coordinates
            var norm13 = (v1.normal + s13 * (v3.normal - v1.normal)).normalized;
            var norm23 = (v2.normal + s23 * (v3.normal - v2.normal)).normalized;
            var uv13 = v1.uv + s13 * (v3.uv - v1.uv);
            var uv23 = v2.uv + s23 * (v3.uv - v2.uv);

            // Add vertices/normals/uv for the intersection points to each mesh
            topSlice.AddCutFaceVertex(v13, norm13, uv13);
            topSlice.AddCutFaceVertex(v23, norm23, uv23);
            bottomSlice.AddCutFaceVertex(v13, norm13, uv13);
            bottomSlice.AddCutFaceVertex(v23, norm23, uv23);

            // Indices for the intersection vertices (for the original mesh data)
            int index13_A = topSlice.Vertices.Count - 2;
            int index23_A = topSlice.Vertices.Count - 1;
            int index13_B = bottomSlice.Vertices.Count - 2;
            int index23_B = bottomSlice.Vertices.Count - 1;

            if (v3BelowCutPlane)
            {
                // Triangle slice above the cutting plane is a quad, so divide into two triangles
                topSlice.AddTriangle(index23_A, index13_A, topSlice.IndexMap[v2_idx], subMesh);
                topSlice.AddTriangle(index13_A, topSlice.IndexMap[v1_idx], topSlice.IndexMap[v2_idx], subMesh);

                // One triangle must be added to mesh 2
                bottomSlice.AddTriangle(bottomSlice.IndexMap[v3_idx], index13_B, index23_B, subMesh);

                // When looking at the cut-face, the edges should wind counter-clockwise
                topSlice.Constraints.Add(new EdgeConstraint(topSlice.CutVertices.Count - 2, topSlice.CutVertices.Count - 1));
                bottomSlice.Constraints.Add(new EdgeConstraint(bottomSlice.CutVertices.Count - 1, bottomSlice.CutVertices.Count - 2));
            }
            else
            {
                // Triangle slice above the cutting plane is a simple triangle
                topSlice.AddTriangle(index13_A, index23_A, topSlice.IndexMap[v3_idx], subMesh);

                // Triangle slice below the cutting plane is a quad, so divide into two triangles
                bottomSlice.AddTriangle(bottomSlice.IndexMap[v1_idx], bottomSlice.IndexMap[v2_idx], index13_B, subMesh);
                bottomSlice.AddTriangle(bottomSlice.IndexMap[v2_idx], index23_B, index13_B, subMesh);

                // When looking at the cut-face, the edges should wind counter-clockwise
                topSlice.Constraints.Add(new EdgeConstraint(topSlice.CutVertices.Count - 1, topSlice.CutVertices.Count - 2));
                bottomSlice.Constraints.Add(new EdgeConstraint(bottomSlice.CutVertices.Count - 2, bottomSlice.CutVertices.Count - 1));
            }
        }
    }
}