using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Rigidbody))]
public class Prefracture : MonoBehaviour
{
    public TriggerOptions triggerOptions;
    public FractureOptions fractureOptions;
    public CallbackOptions callbackOptions;
    public PrefractureOptions prefractureOptions;

    /// <summary>
    /// Collector object that stores the produced fragments
    /// </summary>
    private GameObject fragmentRoot;

    void OnValidate()
    {
        if (this.transform.parent != null)
        {
            // When an object is fractured, the fragments are created as children of that object's parent.
            // Because of this, they inherit the parent transform. If the parent transform is not scaled
            // the same in all axes, the fragments will not be rendered correctly.
            var scale = this.transform.parent.localScale;
            if ((scale.x != scale.y) || (scale.x != scale.z) || (scale.y != scale.z))
            {
                Debug.LogWarning($"Warning: Parent transform of fractured object must be uniformly scaled in all axes or fragments will not render correctly.", this.transform);
            }
        }
    }

    /// <summary>
    /// Compute the fracture and create the fragments
    /// </summary>
    /// <returns></returns>
    [ExecuteInEditMode] 
    [ContextMenu("Prefracture")]
    public void ComputeFracture()
    {
        // This method should only be called from the editor during design time
        if (!Application.isEditor || Application.isPlaying) return;

        var mesh = this.GetComponent<MeshFilter>().sharedMesh;

        if (mesh != null)
        {
            // If the fragment root object has not yet been created, create it now
            if (this.fragmentRoot == null)
            {
                // Create a game object to contain the fragments
                this.fragmentRoot = new GameObject($"{this.name}Fragments");
                this.fragmentRoot.transform.SetParent(this.transform.parent);

                // Each fragment will handle its own scale
                this.fragmentRoot.transform.position = this.transform.position;
                this.fragmentRoot.transform.rotation = this.transform.rotation;
                this.fragmentRoot.transform.localScale = Vector3.one;
            }            

            var fragmentTemplate = CreateFragmentTemplate();
            
            Fragmenter.Fracture(this.gameObject,
                                this.fractureOptions,
                                fragmentTemplate,
                                this.fragmentRoot.transform,
                                prefractureOptions.saveFragmentsToDisk,
                                prefractureOptions.saveLocation);
                                        
            // Done with template, destroy it. Since we're in editor, use DestroyImmediate
            GameObject.DestroyImmediate(fragmentTemplate);
                
            // Deactivate the original object
            this.gameObject.SetActive(false);

            // Fire the completion callback
            if (callbackOptions.onCompleted != null)
            {
                callbackOptions.onCompleted.Invoke();
            }
        }
    }

    /// <summary>
    /// Creates a template object which each fragment will derive from
    /// </summary>
    /// <returns></returns>
    private GameObject CreateFragmentTemplate()
    {
        // If pre-fracturing, make the fragments children of this object so they can easily be unfrozen later.
        // Otherwise, parent to this object's parent
        GameObject obj = new GameObject();
        obj.name = "Fragment";
        obj.tag = this.tag;

        // Update mesh to the new sliced mesh
        obj.AddComponent<MeshFilter>();

        // Add renderer. Default material goes in slot 1, cut material in slot 2
        var meshRenderer = obj.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterials = new Material[2] {
            this.GetComponent<MeshRenderer>().sharedMaterial,
            this.fractureOptions.insideMaterial
        };

        // Copy collider properties to fragment
        var thisCollider = this.GetComponent<Collider>();
        var fragmentCollider = obj.AddComponent<MeshCollider>();
        fragmentCollider.convex = true;
        fragmentCollider.sharedMaterial = thisCollider.sharedMaterial;
        fragmentCollider.isTrigger = thisCollider.isTrigger;

        // Copy rigid body properties to fragment
        var rigidBody = obj.AddComponent<Rigidbody>();
        // When pre-fracturing, freeze the rigid body so the fragments don't all crash to the ground when the scene starts.
        rigidBody.constraints = RigidbodyConstraints.FreezeAll;
        rigidBody.drag = this.GetComponent<Rigidbody>().drag;
        rigidBody.angularDrag = this.GetComponent<Rigidbody>().angularDrag;
        rigidBody.useGravity = this.GetComponent<Rigidbody>().useGravity;

        var unfreeze = obj.AddComponent<UnfreezeFragment>();
        unfreeze.unfreezeAll = prefractureOptions.unfreezeAll;
        unfreeze.triggerOptions = this.triggerOptions;
        unfreeze.onFractureCompleted = callbackOptions.onCompleted;
        
        return obj;
    }
}