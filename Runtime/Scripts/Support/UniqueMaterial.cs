using UnityEngine;

public class UniqueMaterial : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Creates a unique instance of the material, decoupling it from the other objects.
        // This script is only used for the Slice demo to highlight slices and is not essential
        // for the fracturing/slicing code to work.
        this.GetComponent<MeshRenderer>().material = this.GetComponent<MeshRenderer>().material;   
    }
}
