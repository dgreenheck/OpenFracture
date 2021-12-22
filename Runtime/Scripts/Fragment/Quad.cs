using UnityEngine.TestTools;

/// <summary>
/// Data structure that holds triangulation adjacency data for a quad
/// </summary>
public struct Quad
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

    /// <summary>
    /// The indices of the quad vertices
    /// </summary>
    public int q1, q2, q3, q4;

    /// <summary>
    /// The triangles that make up the quad
    /// </summary>
    public int t1, t2;

    /// <summary>
    /// Triangle adjacency data
    /// </summary>
    public int t1L, t1R, t2L, t2R;

    public Quad(int q1, int q2, int q3, int q4, int t1, int t2, int t1L, int t1R, int t2L, int t2R)
    {
        this.q1 = q1;
        this.q2 = q2;
        this.q3 = q3;
        this.q4 = q4;
        this.t1 = t1;
        this.t2 = t2;
        this.t1L = t1L;
        this.t1R = t1R;
        this.t2L = t2L;
        this.t2R = t2R;
    }

    [ExcludeFromCoverage]
    public override string ToString()
    {
        return $"T{t1}/T{t2} (V{q1},V{q2},V{q3},V{q4})";
    }
}