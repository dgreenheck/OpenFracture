using UnityEngine;
using UnityEngine.TestTools;

[ExcludeFromCoverage]
public class PlaneSlicer : MonoBehaviour
{
    public float RotationSensitivity = 0.1f;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            this.transform.Rotate(Vector3.forward, RotationSensitivity, Space.Self);
        }
        if (Input.GetKey(KeyCode.E))
        {
            this.transform.Rotate(Vector3.forward, -RotationSensitivity, Space.Self);
        }
        
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            var mesh = this.GetComponent<MeshFilter>().sharedMesh;
            var center = mesh.bounds.center;
            var extents = mesh.bounds.extents;

            extents = new Vector3(extents.x * this.transform.localScale.x,
                                  extents.y * this.transform.localScale.y,
                                  extents.z * this.transform.localScale.z);
                                  
            // Cast a ray and find the nearest object
            RaycastHit[] hits = Physics.BoxCastAll(this.transform.position, extents, this.transform.forward, this.transform.rotation, extents.z);
            
            foreach(RaycastHit hit in hits)
            {
                var obj = hit.collider.gameObject;
                var sliceObj = obj.GetComponent<Slice>();

                if (sliceObj != null)
                {
                    obj.GetComponent<HighlightSlice>()?.HideSliceLine();
                    sliceObj.ComputeSlice(this.transform.up, this.transform.position);
                }
            }
        }
    }
}
