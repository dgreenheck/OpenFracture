using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
/// <summary>
/// Options for refracturing
/// </summary>
public class RefractureOptions
{
    [Tooltip("Enables refracturing of fragments. WARNING: This setting can result in a significant amount of generated fragments. It is recommended to keep FragmentCount low if this is enabled.")]
    public bool enableRefracturing;

    [Tooltip("Maximum number of times a fragment can be re-fractured.")]
    [Range(1, 3)]
    public int maxRefractureCount;

    [Tooltip("Enable if refracturing should also invoke the callback functions.")]
    public bool invokeCallbacks;

    public RefractureOptions()
    {
        this.enableRefracturing = false;
        this.maxRefractureCount = 1;
        this.invokeCallbacks = false;
    }
}