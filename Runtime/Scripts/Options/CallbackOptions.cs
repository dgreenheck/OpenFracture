using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class CallbackOptions
{
    [Tooltip("This callback is invoked when a fracture has been triggered. Not called for slicing and prefracturing.")]
    public UnityEvent<Collider, GameObject, Vector3> onFracture;

    [Tooltip("This callback is invoked when the fracturing/slicing process has been completed.")]
    public UnityEvent onCompleted;


    public CallbackOptions()
    {
        this.onCompleted = null;
    }

    public void CallOnFracture(Collider instigator, GameObject fracturedObject, Vector3 point)
    {
        onFracture?.Invoke(instigator, fracturedObject, point);
    }
}