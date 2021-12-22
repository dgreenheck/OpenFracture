using System;
using UnityEngine;

[Serializable]
/// <summary>
/// Options for prefracturing a mesh
/// </summary>
public class PrefractureOptions
{
    [Tooltip("For prefractured objects, if this property is enabled, the all fragments will unfreeze if a single fragment is interacted with.")]
    public bool unfreezeAll;

    [Tooltip("Saves the fragment meshes to disk. Required if the fragments will be used in a prefab.")]
    public bool saveFragmentsToDisk;
    
    [Tooltip("Path to save the fragments to if saveToDisk is enabled. Relative to the project directory.")]
    public string saveLocation;

    public PrefractureOptions()
    {
        this.unfreezeAll = true;
        this.saveFragmentsToDisk = false;
        this.saveLocation = "";
    }
}