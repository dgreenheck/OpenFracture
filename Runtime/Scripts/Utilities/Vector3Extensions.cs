using System;
using UnityEngine;

public static class Vector3Extensions
{
    // 
    // that the normal is pointing to
    //   - p: The point being checked
    //   - n: The normal of the plane
    //   - o: The origin of the plane
    /// <summary>
    /// Returns true if the point is either on or above the plane. "Above" is the side of the place in the direction of the normal.
    /// </summary>
    /// <param name="p">The test point</param>
    /// <param name="n">The plane normal</param>
    /// <param name="o">The plane origin</param>
    /// <returns></returns>
    public static bool IsAbovePlane(this Vector3 p, Vector3 n, Vector3 o)
    {
        return (n.x * (p.x - o.x) + n.y * (p.y - o.y) + n.z * (p.z - o.z)) >= 0;
    }
}