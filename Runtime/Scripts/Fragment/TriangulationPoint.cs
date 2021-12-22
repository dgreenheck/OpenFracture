using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// This data structure is used to represent a point during triangulation.
/// </summary>
public class TriangulationPoint: IBinSortable
{
    /// <summary>
    /// 2D coordinates of the point on the triangulation plane
    /// </summary>
    public Vector2 coords;

    /// <summary>
    /// Bin used for sorting points in grid
    /// </summary>
    public int bin { get; set; }

    /// <summary>
    /// Original index prior to sorting
    /// </summary>
    public int index = 0;

    /// <summary>
    /// Instantiates a new triangulation point
    /// </summary>
    /// <param name="index">The index of the point in the original point list</param>
    /// <param name="coords">The 2D coordinates of the point in the triangulation plane</param>
    public TriangulationPoint(int index, Vector2 coords)
    {
        this.index = index;
        this.coords = coords;
    }

    [ExcludeFromCoverage]
    public override string ToString()
    {
        return $"{coords} -> {bin}";
    }
}