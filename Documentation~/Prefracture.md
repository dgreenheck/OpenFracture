# Prefracture

## Overview

The `Prefracture` script allows meshes to be fractured in the editor. When a mesh is prefractured, the fragments are added to the scene and the original object is set to an inactive state. Each fragment has a RigidBody attached to it which is frozen. Fragments can be unfrozen based on several trigger conditions (listed below). When the trigger condition is satisfied, the fragment is unfrozen. Optionally, all fragments can be unfrozen if a single fragment's trigger conditions are satisfied. This allows prefractured meshes to be broken apart one piece at a time or shatter all at once.

## Prerequisites

An object should have the following components added to it. The first three are required components for the script (i.e. they are added automatically) while the Collider is not a required component since you are allowed to use any type of collider.

* `MeshFilter`
* `MeshRenderer`
* `RigidBody`
* `Collider` (any type)

## Properties

![image](https://user-images.githubusercontent.com/3814912/148163874-281eeb3a-3916-4f94-bcc3-bb8f32708c6c.png)

### Trigger Options

- **Trigger Type**: The method that triggers the fragments to "wake up" and become active physics objects.
  - **Collision**: Physics-based colliders
  - **Trigger**: Trigger-based colliders
  - **Keyboard**: User presses a key 
- **Minimum Collision Force**: The minimum collision force required to trigger the fracture. If the collision force is equal to or greater than this value, the fracture will be triggered. To ignore this setting, set the minimum force to 0. This option is available for the **Collision** trigger type only.
- **Limit collisions to selected tags?**: By enabling this option, you can limit which object tags will trigger the collision. When a collision is detected, the colliding object's tag will be compared against the list of **Included Tags**. If it is contained within that list (and the other collision criteria are met), the fracture will be triggered. This option is available for the **Collision** and **Trigger** trigger types only.
- **Included Tags**: The set of GameObject tags that can trigger a fracture. This option is available for the **Collision** and **Trigger** trigger types only.
- **Trigger Key**: The key that will trigger the fracture when press. This option is available for the **Keyboard** trigger type only.

### Fracture Options
- **Fragment Count**: The number of fragments to break the object into. *Note:* If **Detect Floating Fragments** is set to true, the final number of fragments may be higher than **Fragment Count**. This is because floating fragment detection is performed after the fracturing stage is complete.
- **Asynchronous**: Has no effect for prefracturing 
- **Detect Floating Fragments**: If enable, a pass will be made on the resulting fragments after the fracture algorithm has executed to determine if any of the fragments contain unconnected geometry. This can occur when fracturing non-convex meshes. Since the geometry of each fragment must be searched to identify these disconnected sets of vertices/faces, this option will significantly reduce the performance of the fracturing. Once again, if performance is an issue, it is recommended that you use the `Prefracture` script.
- **Fracture Along X/Y/Z Plane**: Each fracture line can be specified by a vector. For some objects, it is desirable to keep this vector locked to specific planes. For example, assume you have a model of a pane of glass with the width along the X-axis, the height along the Y-axis and the thickness along the Z-axis. The fracture lines should be constrained to the face of the glass (X-Y plane) and not split the glass along its thickness. In this case you would set **Fracture Along X Plane** and **Fracture Along Y Plane** to true and **Fracture Along Z Plane** to false.
- **Inside Material**: The material to use for the newly fractured faces.
- **Texture Scale**: Scale factor applied to the UV coordinates for the fractured faces.
- **Texture Offset**: Constant offset applied to the UV coordinates for the fractured faces.

### Callback Options
- **OnCompleted()**: This callback is triggered when the fracturing has been completed. Use this to play a sound, turn on a light or any other in-game logic you require.

### Prefracture Options
- **Unfreeze All**: If one fragment is triggered, unfreeze all fragments.
- **Save Fragments to Disk**: Saves the fragment meshes to disk. Required if the prefractured mesh will be used in a prefab. Optional if the prefractured mesh will not be used as a prefab and will be embedded in the scene.
- **Save Location**: Location to save the fragments to relative to the project root directory.
- **Prefracture**: This button will fracture the mesh and generate the fragments. After the fragments have been generated, the object containing the base mesh is set to inactive but remains in the scene.
