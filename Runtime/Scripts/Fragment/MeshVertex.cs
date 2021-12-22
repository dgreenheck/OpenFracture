using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Data structure containing position/normal/UV data for a single vertex
/// </summary>
public struct MeshVertex
{
    public Vector3 position;
    public Vector3 normal;
    public Vector2 uv;

    public MeshVertex(Vector3 position)
    {
        this.position = position;
        this.normal = Vector3.zero;
        this.uv = Vector2.zero;
    }

    public MeshVertex(Vector3 position, Vector3 normal, Vector2 uv)
    {
        this.position = position;
        this.normal = normal;
        this.uv = uv;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is MeshVertex)) return false;
       
        return ((MeshVertex)obj).position.Equals(this.position);
    }

    public static bool operator ==(MeshVertex lhs, MeshVertex rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(MeshVertex lhs, MeshVertex rhs)
    {
        return !lhs.Equals(rhs);
    }

    public override int GetHashCode()
    {
        return this.position.GetHashCode();
    }

    [ExcludeFromCoverage]
    public override string ToString()
    {
        return $"Position = {position}, Normal = {normal}, UV = {uv}";
    }
}