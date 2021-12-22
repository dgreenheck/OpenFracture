using System;
using UnityEngine;

public class HighlightSlice : MonoBehaviour
{
    public Color baseColor;
    public Color sliceColor;
    public float lineThickness;

    public void Start()
    {
        var material = this.GetComponent<MeshRenderer>().sharedMaterial;
        material.SetVector("CutPlaneNormal", Vector3.zero);
        material.SetVector("CutPlaneOrigin", Vector3.positiveInfinity);
        material.SetColor("BaseColor", baseColor);
        material.SetColor("SliceColor", sliceColor);
        material.SetFloat("LineThickness", lineThickness);
    }

    public void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject.tag == "Slicer")
        {
            var material = this.GetComponent<MeshRenderer>().sharedMaterial;
            material.SetVector("CutPlaneNormal", collider.gameObject.transform.up);
            material.SetVector("CutPlaneOrigin", collider.gameObject.transform.position);
        }
    }

    public void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.tag == "Slicer")
        {
            HideSliceLine();
        }
    }

    public void HideSliceLine()
    {
        var material = this.GetComponent<MeshRenderer>().sharedMaterial;
        material.SetVector("CutPlaneNormal", Vector3.zero);
        material.SetVector("CutPlaneOrigin", Vector3.positiveInfinity);
    }
}
