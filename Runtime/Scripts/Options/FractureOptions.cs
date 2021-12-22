using System;
using UnityEngine;

[Serializable]
/// <summary>
/// Options for fracturing a mesh
/// </summary>
public class FractureOptions
{
    [Range(1, 1024)]
    [Tooltip("Maximum number of times an object and its children are recursively fractured. Larger fragment counts will result in longer computation times.")]
    public int fragmentCount;

    [Tooltip("Enables fracturing in the local X plane")]
    public bool xAxis;

    [Tooltip("Enables fracturing in the local Y plane")]
    public bool yAxis;

    [Tooltip("Enables fracturing in the local  Z plane")]
    public bool zAxis;

    [Tooltip("Enables detection of \"floating\" fragments when fracturing non-convex meshes. This setting has no effect for convex meshes and should be disabled.")]
    public bool detectFloatingFragments;

    [Tooltip("Fracturing is performed asynchronously on the main thread.")]
    public bool asynchronous;

    [Tooltip("The material to use for the inside faces")]
    public Material insideMaterial;
    
    [Tooltip("Scale factor to apply to texture coordinates")]
    public Vector2 textureScale;

    [Tooltip("Offset to apply to texture coordinates")]
    public Vector2 textureOffset;

    public FractureOptions()
    {
        this.fragmentCount = 10;
        this.xAxis = true;
        this.yAxis = true;
        this.zAxis = true;
        this.detectFloatingFragments = false;
        this.asynchronous = false;
        this.insideMaterial = null;
        this.textureScale = Vector2.one;
        this.textureOffset = Vector2.zero;
    }
}