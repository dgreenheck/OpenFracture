# Slice

## Overview

The `Slice` script allows meshes to be sliced into two pieces during runtime. Fragments can resliced into smaller fragments if desired.

## Prerequisites

An object should have the following components added to it. The first three are required components for the script (i.e. they are added automatically) while the Collider is not a required component since you are allowed to use any type of collider.

* `MeshFilter`
* `MeshRenderer`
* `RigidBody`
* `Collider` (any type)

## Properties

![image](https://user-images.githubusercontent.com/3814912/148101876-404179ad-5cc3-4427-943a-3b75f8d5d259.png)

### Slice Options

- **Enable Reslicing**: Set to true to enable reslicing of fragments.
- **Max Reslice Count**: The maximum number of times a fragment can be resliced.
- **Detect Floating Fragments**: If enable, a pass will be made on the resulting fragments after the slicing algorithm has executed to determine if any of the fragments contain unconnected geometry. This can occur when slicing non-convex meshes. Since the geometry of each fragment must be searched to identify these disconnected sets of vertices/faces, this option will significantly reduce the performance of the slicing.
- **Inside Material**: The material to use for the newly sliced faces.
- **Texture Scale**: Scale factor applied to the UV coordinates for the sliced faces.
- **Texture Offset**: Constant offset applied to the UV coordinates for the sliced faces.
- **Invoke Callbacks**: If enabled, slicing fragments will also trigger the callback functions. This option can be useful if you only want to trigger an action the first time an object is sliced but not when the fragments are resliced (in this case, you would set this option to false).

### Callback Options
- **OnCompleted()**: This callback is triggered when the slicing has been completed. Use this to play a sound, turn on a light or any other in-game logic you require.

## How to Use

### Call from Script
Unlike `Fracture` and `Prefracture`, `Slice` requires scripting since you must pass in the normal and origin of the slice plane during runtime.

If `obj` is the object to be sliced, execute the following code to slice `obj` into two fragments. `sliceNormal` and `sliceOrigin` are the normal and origin of the slice plane in world coordinates, respectively.

```csharp
var slicer = obj.GetComponent<Slice>();
var sliceNormal = Vector3.up;
var sliceOrigin = obj.transform.position; 

slicer.ComputeSlice(sliceNormal, sliceOrigin);
```
