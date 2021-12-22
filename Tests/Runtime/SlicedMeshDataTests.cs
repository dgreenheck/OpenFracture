using NUnit.Framework;
using UnityEngine;

public class FragmentDataTests
{
    [Test]
    public void EmptyInit()
    {
        int vertexCount = 101;
        FragmentData data = new FragmentData(vertexCount, 0);

        // Verify initialization
        Assert.AreEqual(vertexCount, data.Vertices.Capacity);
        Assert.AreEqual(vertexCount, data.IndexMap.Length);
        Assert.Zero(data.Vertices.Count);
        Assert.Zero(data.CutVertices.Count);
        Assert.Zero(data.Constraints.Count);
        Assert.Zero(data.triangleCount);
        Assert.Zero(data.vertexCount);

        // Assume two submeshes for now
        Assert.AreEqual(data.Triangles.Length, 2);
    }
    
    [Test]
    public void MeshInit()
    {
        // Test mesh data
        var vertices = new Vector3[] {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(1, 1, 0)
        };
        var normals = new Vector3[] {
            new Vector3(-0.1f, -0.1f, 1).normalized,
            new Vector3(0.1f, -0.1f, 1).normalized,
            new Vector3(-0.1f, 0.1f, 1).normalized,
            new Vector3(0.1f, 0.1f, 1).normalized
        };
        var uv = new Vector2[] {
            new Vector3(0, 0),
            new Vector3(1, 0),
            new Vector3(0, 1),
            new Vector3(1, 1)
        };
        var triangles = new int[] { 0, 2, 1, 1, 2, 3 };
        
        // Setup the test mesh
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uv;
        
        // Generate the sliced mesh data from the mesh
        FragmentData meshData = new FragmentData(mesh);

        // Verify sliced mesh data was initialized properly

        // Check vertex data
        Assert.AreEqual(mesh.vertexCount, meshData.Vertices.Count);
        for(int i = 0; i < meshData.Vertices.Count; i++)
        {
            var vertex = meshData.Vertices[i];
            Assert.AreEqual(vertices[i], vertex.position);
            Assert.AreEqual(normals[i], vertex.normal);
            Assert.AreEqual(uv[i], vertex.uv);
        }

        // Check triangle data
        Assert.AreEqual(mesh.triangles.Length, meshData.Triangles[0].Count);
        // Assume two submeshes for now
        Assert.AreEqual(2, meshData.Triangles.Length);

        Assert.Zero(meshData.CutVertices.Count);
        Assert.Zero(meshData.Constraints.Count);
        Assert.AreEqual(mesh.vertexCount, meshData.IndexMap.Length);
    }

    [Test]
    public void AddCutFaceVertex()
    {
        int vertexCount = 10;
        var meshData = new FragmentData(vertexCount, 0);

        for (int i = 0; i < vertexCount; i++)
        {
            meshData.AddCutFaceVertex(
                new Vector3(i + 1, i + 2, i + 3), 
                new Vector3(i + 4, i + 5, i + 6),
                new Vector2(i + 7, i + 8)
            );
        }

        // Vertex add to both main vertex data and cut-face vertex data
        Assert.AreEqual(vertexCount, meshData.Vertices.Count);
        Assert.AreEqual(vertexCount, meshData.CutVertices.Count);

                // Index map should be updated as well
        for (int i = 0; i < vertexCount; i++)
        {
            Assert.AreEqual(new Vector3(i + 1, i + 2, i + 3), meshData.Vertices[i].position);
            Assert.AreEqual(new Vector3(i + 4, i + 5, i + 6), meshData.Vertices[i].normal);
            Assert.AreEqual(new Vector2(i + 7, i + 8), meshData.Vertices[i].uv); 
            Assert.AreEqual(new Vector3(i + 1, i + 2, i + 3), meshData.CutVertices[i].position);
            Assert.AreEqual(new Vector3(i + 4, i + 5, i + 6), meshData.CutVertices[i].normal);
            Assert.AreEqual(new Vector2(i + 7, i + 8), meshData.CutVertices[i].uv); 
        }
    }

    [Test]
    public void AddMappedVertex()
    {
        int vertexCount = 10;
        var meshData = new FragmentData(vertexCount, 0);

        for (int i = 0; i < vertexCount; i++)
        {
            MeshVertex vertex = new MeshVertex(
                new Vector3(i + 1, i + 2, i + 3), 
                new Vector3(i + 4, i + 5, i + 6),
                new Vector2(i + 7, i + 8));

            meshData.AddMappedVertex(vertex, i);
        }
            
        // Vertex added only to main vertex data
        Assert.AreEqual(vertexCount, meshData.Vertices.Count);
        Assert.AreEqual(0, meshData.CutVertices.Count);

        // Index map should be updated as well
        for (int i = 0; i < vertexCount; i++)
        {
            Assert.AreEqual(new Vector3(i + 1, i + 2, i + 3), meshData.Vertices[i].position);
            Assert.AreEqual(new Vector3(i + 4, i + 5, i + 6), meshData.Vertices[i].normal);
            Assert.AreEqual(new Vector2(i + 7, i + 8), meshData.Vertices[i].uv); 
            Assert.AreEqual(i, meshData.IndexMap[i]);
        }
    }

    [Test]
    public void AddTriangle()
    {
        FragmentData meshData = new FragmentData(0, 0);

        int v1 = 1;
        int v2 = 2;
        int v3 = 3;

        meshData.AddTriangle(v1, v2, v3, SlicedMeshSubmesh.Default);

        Assert.AreEqual(3, meshData.Triangles[(int)SlicedMeshSubmesh.Default].Count);
        Assert.AreEqual(v1, meshData.Triangles[(int)SlicedMeshSubmesh.Default][0]);
        Assert.AreEqual(v2, meshData.Triangles[(int)SlicedMeshSubmesh.Default][1]);
        Assert.AreEqual(v3, meshData.Triangles[(int)SlicedMeshSubmesh.Default][2]);
    }

    [Test]
    public void AddMappedTriangle()
    {
        int vertexCount = 46;
        FragmentData meshData = new FragmentData(vertexCount, 0);
     
        // Indices of the vertices in the original mesh (arbitary indices)
        int v1 = 11;
        int v2 = 23;
        int v3 = 45;

        // First, add the vertx data. v1, v2, v3 will be mapped to the indices
        // 0, 1, 2 (respectively) since that is the order they are added
        meshData.AddMappedVertex(new MeshVertex(Vector3.zero), v1);
        int v1Mapped = meshData.vertexCount - 1;
        meshData.AddMappedVertex(new MeshVertex(Vector3.zero), v2);
        int v2Mapped = meshData.vertexCount - 1;
        meshData.AddMappedVertex(new MeshVertex(Vector3.zero), v3);
        int v3Mapped = meshData.vertexCount - 1;

        // Add the triangle, but map v1, v2, v3 to the indices for the sliced mesh
        meshData.AddMappedTriangle(v1, v2, v3, SlicedMeshSubmesh.Default);

        Assert.AreEqual(3, meshData.Triangles[(int)SlicedMeshSubmesh.Default].Count);
        Assert.AreEqual(v1Mapped, meshData.Triangles[(int)SlicedMeshSubmesh.Default][0]);
        Assert.AreEqual(v2Mapped, meshData.Triangles[(int)SlicedMeshSubmesh.Default][1]);
        Assert.AreEqual(v3Mapped, meshData.Triangles[(int)SlicedMeshSubmesh.Default][2]);
    }

    [Test]
    public void WeldCutFaceVertices_AllVerticesUnique()
    {
        int vertexCount = 10;
        FragmentData meshData = new FragmentData(vertexCount, 0);

        for (int i = 0; i < vertexCount; i++)
        {
            meshData.AddCutFaceVertex(
                new Vector3(i + 1, i + 2, i + 3), 
                Vector3.zero,
                Vector2.zero
            );
        }

        // Verify vertex count prior to weld
        Assert.AreEqual(vertexCount, meshData.CutVertices.Count);

        meshData.WeldCutFaceVertices();

        // # of cut vertices should remain unchanged after weld
        Assert.AreEqual(vertexCount, meshData.CutVertices.Count);
    }

    [Test]
    public void WeldCutFaceVertices_OneDuplicateVertex()
    {
        int vertexCount = 10;
        FragmentData meshData = new FragmentData(vertexCount, 0);

        for (int i = 0; i < vertexCount; i++)
        {
            meshData.AddCutFaceVertex(
                new Vector3(i + 1, i + 2, i + 3), 
                Vector3.zero,
                Vector2.zero
            );
        }

        // Duplicate one of the vertices
        meshData.CutVertices[vertexCount - 1] = meshData.CutVertices[0];

        // Verify vertex count prior to weld
        Assert.AreEqual(vertexCount, meshData.CutVertices.Count);

        meshData.WeldCutFaceVertices();

        // Expect two vertices welded, so final count is one less vertex
        Assert.AreEqual(vertexCount - 1, meshData.CutVertices.Count);
    }

    [Test]
    public void WeldCutFaceVertices_GreaterThanEpsilon()
    {
        int vertexCount = 10;
        FragmentData meshData = new FragmentData(vertexCount, 0);

        for (int i = 0; i < vertexCount; i++)
        {
            meshData.AddCutFaceVertex(
                new Vector3(i + 1, i + 2, i + 3), 
                Vector3.zero,
                Vector2.zero
            );
        }

        var magnitude = meshData.CutVertices[0].position.magnitude;

        // According to Unity documentation, == comparison between two vectors
        // returns true if their magnitude is less than 1E-5. We make two vertices 
        // differ by slightly larger than this amount so that they are not welded together.
        meshData.CutVertices[vertexCount - 1] = new MeshVertex(
            (magnitude + 1.1E-5f) * meshData.CutVertices[0].position.normalized,
            Vector3.zero,
            Vector2.zero
        );

        // Verify vertex count prior to weld
        Assert.AreEqual(vertexCount, meshData.CutVertices.Count);

        meshData.WeldCutFaceVertices();

        // Expect same vertex count since no vertices welded
        Assert.AreEqual(vertexCount, meshData.CutVertices.Count);
    }
    
    [Test]
    public void WeldCutFaceVertices_LessThanEpsilon()
    {
        int vertexCount = 10;
        FragmentData meshData = new FragmentData(vertexCount, 0);

        for (int i = 0; i < vertexCount; i++)
        {
            meshData.AddCutFaceVertex(
                new Vector3(i + 1, i + 2, i + 3), 
                Vector3.zero,
                Vector2.zero
            );
        }

        var magnitude = meshData.CutVertices[0].position.magnitude;

        // According to Unity documentation, == comparison between two vectors
        // returns true if their magnitude is less than 1E-5. We make two vertices 
        // differ by slightly less than this amount so that they are welded together.
        meshData.CutVertices[vertexCount - 1] = new MeshVertex(
            (magnitude + 0.9E-5f) * meshData.CutVertices[0].position.normalized,
            Vector3.zero,
            Vector2.zero
        );

        // Verify vertex count prior to weld
        Assert.AreEqual(vertexCount, meshData.CutVertices.Count);

        meshData.WeldCutFaceVertices();

        // Expect two vertices welded, so final count is one less vertex
        Assert.AreEqual(vertexCount - 1, meshData.CutVertices.Count);
    }
        
    [Test]
    public void WeldCutFaceVertices_CutEdgeUpdate()
    {
        int vertexCount = 10;
        FragmentData meshData = new FragmentData(vertexCount, 0);

        for (int i = 0; i < vertexCount; i++)
        {
            meshData.AddCutFaceVertex(
                new Vector3(i + 1, i + 2, i + 3), 
                Vector3.zero,
                Vector2.zero
            );
        }

        // Last vertex is equal to the first vertex
        meshData.CutVertices[vertexCount - 1] = meshData.CutVertices[0];

        // Add cut edges that map to the duplicate vertex
        meshData.Constraints.Add(new EdgeConstraint(0, vertexCount - 1));

        // Assert expected edge state prior to weld
        Assert.AreEqual(0, meshData.Constraints[0].v1);
        Assert.AreEqual(vertexCount - 1, meshData.Constraints[0].v2);

        meshData.WeldCutFaceVertices();

        // Assert expected edge state after the weld
        Assert.AreEqual(0, meshData.Constraints[0].v1);
        Assert.AreEqual(0, meshData.Constraints[0].v2);
    }

    [Test]
    public void ToMesh()
    {
        int vertexCount = 100;
        int cutVertexCount = 50;
        int triangleCount1 = 10;
        int triangleCount2 = 15;

        FragmentData meshData = new FragmentData(vertexCount, 0);

        // Add some fake data
        for (int i = 0; i < vertexCount; i++)
        {
            meshData.AddMappedVertex(new MeshVertex(), 0);
        }
        
        for (int i = 0; i < cutVertexCount; i++)
        {
            meshData.AddCutFaceVertex(Vector3.zero, Vector3.zero, Vector2.zero);
        }

        for (int i = 0; i < triangleCount1; i++)
        {
            meshData.AddTriangle(0, 0, 0, SlicedMeshSubmesh.Default);
        }

        for (int i = 0; i < triangleCount2; i++)
        {
            meshData.AddTriangle(0, 0, 0, SlicedMeshSubmesh.CutFace);
        }

        Mesh mesh = meshData.ToMesh();

        // Each time we add a cut face vertex, it adds two vertices, one for the
        // original sub mesh and another for the cut face sub mesh.
        Assert.AreEqual(vertexCount + 2 * cutVertexCount, mesh.vertexCount);
        Assert.AreEqual(2, mesh.subMeshCount);

        // Three indices for each triangle
        Assert.AreEqual(3 * triangleCount1, mesh.GetTriangles(0).Length);
        Assert.AreEqual(3 * triangleCount2, mesh.GetTriangles(1).Length);
    }
}