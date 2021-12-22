using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SliceOptions
{
    [Tooltip("Enables reslicing of fragments.")]
    public bool enableReslicing;

    [Tooltip("Maximum number of times a fragment can be re-sliced.")]
    [Range(1, 100)]
    public int maxResliceCount;

    [Tooltip("Enables detection of \"floating\" fragments when slicing non-convex meshes. This setting has no effect for convex meshes and should be disabled.")]
    public bool detectFloatingFragments;

    [Tooltip("The material to use for the inside faces")]
    public Material insideMaterial;
    
    [Tooltip("Scale factor to apply to texture coordinates")]
    public Vector2 textureScale;

    [Tooltip("Offset to apply to texture coordinates")]
    public Vector2 textureOffset;

    [Tooltip("Enable if re-slicing should also invoke the callback functions.")]
    public bool invokeCallbacks;

    public SliceOptions()
    {
        this.enableReslicing = false;
        this.maxResliceCount = 1;
        this.insideMaterial = null;
        this.textureScale = Vector2.one;
        this.textureOffset = Vector2.zero;
        this.invokeCallbacks = false;
    }
}