# OpenFracture

![OpenFracture GitHub Header](https://user-images.githubusercontent.com/3814912/148176407-a0c49ba0-c704-4b60-89a3-cea47175b6c2.gif)

OpenFracture is an open source Unity package for fracturing & slicing meshes. This package supports both convex and non-convex meshes as well as meshes with holes. This means any arbitrary geometry can be fractured/sliced (as long as the geometry is closed and non-intersecting).

## Installing

### Unity Package Manager

OpenFracture can be imported using Unity’s built-in Package Manager. Follow the instructions [here](https://docs.unity3d.com/Manual/upm-ui-giturl.html)

### Import into Unity Project

Alternatively, you may download the code directly and add it to your Unity project.

## Documentation

Here is a [link](/Documentation~/GettingStarted.md) to the documentation.

## Features

### Fracturing
- **Runtime and Editor Support** - Fracture meshes either run-time or pre-fracture in the editor for optimal performance.
- **Arbitary Mesh Geometry** - Support for convex and non-convex meshes as well as meshes with multiple holes. **Note: Meshes must be closed and cannot have self-intersecting geometry**
- **2D/3D Fracturing** - Ability to specify which planes the mesh will be fractured in. This is useful when a mesh is effectively 2D (e.g. glass) and only needs to be fractured on two planes.
- **UV Remapping** - Texture coordinates are preserved along edges where mesh is fractured/sliced.
- **Custom Inside Material** - Use custom material for inside faces. Supports textures with options for UV scaling & offset.
- **Recursive Fracturing** - Fragments can be broken down into smaller fragments.
- **Multiple Trigger Types** - Trigger fractures using triggers, collisions or pressing a key. Additonal trigger types can be added easily.
- **Tunable Fragment Count** - Directly specify the number of fragments to easily tune performance for different platforms.
- **Asynchronous** - Support for asynchronous runtime fracturing (single-threaded)
- **Detect Floating Fragments** - Detects if multiple, isolated fragments are created when fracturing non-convex meshes and treats each fragment as a separate mesh.
- **OnCompletion Callback** - Trigger any behavior after the fracturing is complete, such as playing an AudioSource or executing other in-game logic.

### Slicing
- **Runtime Slicing** - Slice objects at runtime
- **Recursive Slicing** - Slice objects multiple times and slice the fragments into smaller pieces
- **Detect Floating Fragments** - Detects if multiple, isolated fragments are created when slicing non-convex meshes and treats each fragment as a separate mesh.
- **UV Remapping** - Texture coordinates are preserved along edges where mesh is fractured/sliced.
- **Custom Inside Material** - Use custom material for inside faces. Supports textures with options for UV scaling & offset.
- **OnCompletion Callback** - Trigger any behavior after the slicing is complete, such as playing an AudioSource or executing other in-game logic.

## Unsupported Features / Limitations

These features are currently **not supported**. They may be added in a future release.

- **Open and/or Self-Intersecting Geometry** - The algorithm which re-triangulates the cut faces requires a closed polygon that does not self-intersect. This means meshes that aren’t completely closed or have self-intersecting geometry will not fracture properly. To fracture meshes that consist of multiple sets of geometry which intersect each other, you can either A) split the geometry into separate meshes or B) remove the self-intersecting geometry in the mesh. This is a hard limitation due to the triangulation algorithm used.
- **Multiple Submeshes** - The new faces generated when slicing/fracturing a mesh use a custom material and must be placed on a separate submesh. Currently, only meshes with one submesh are supported since the new geometry is stored in `submesh[1]`.
- **Skinned Meshes**
- **Physics Joint Transferring** - Any Physics joints are destroyed upon fracturing/slicing and are not transferred to the fragments.
- **Custom Prefabs for Fragments** - Fragments are instantiated from a default template. If you want to have custom components on the fragments generated from the fracturing/slicing, this is currently not supported.
- **Localized Fracturing** - Fracturing currently is performed uniformly without taking into consideration the point of impact.
- **Slicing in Editor** - Currently slicing is only supported during runtime. The main reason for this is that slicing is extremely fast so this feature was not a top priority.

---

# Appendices

## Appendix A: Algorithm Overview

### Slicing

#### Spliting Mesh In Two

The first step in slicing is splitting the existing mesh data into two separate halves, one above the slice plane and the other below. This data will later be used to create two new meshes for each half.

1. Define the normal and origin of the slice plane
2. Instantiate two data structures, one to store mesh data above the slice plane and another to store mesh data below the slice plane.
3. For each vertex in the mesh, determine if it lies above or below the slice plane
4. For each triangle in the mesh, determine if all vertices of the triangle lie above or below the plane. If true, store in the appropriate data structure. Else, mark triangles that are intersected by the slice plane for later processing.
5. For each triangle intersected by the slice plane, slice the triangle in two and store the new vertex/triangle data in the appropriate data structure. The intersection of a triangle and the slice plane is a line. These lines are stored in a separate data structure for use in the next phase of filling the new face.

#### Filling in the Cut Face

Now that the original mesh data has been divided in two and the geometry intersected by the slice plane has been properly handled, the polygon produced by the intersection of the cut plane and the mesh must be filled in. The total set of lines produced by Step 5 of the previous phase define the border constraints for the new face. These constraints and the vertices of the polygon are fed into a constrained Delauney triangulation algorithm (see Reference [1] for more details). 

### Fracturing

Fracturing is performed by recursively slicing a mesh until the target fragment count has been reached. For each slice, the origin of the geometry is calculated and used as the origin for the slice plane. The normal of the slice plane is randomly chosen.

## Appendix B: Running the Unit Tests

Many of the core functions have unit tests written. These can be helpful if you ever decide to start manipulating some of the core fracturing/slicing code. While the testing coverage isn't comprehensive, some tests are better than none!

1. In the menu bar, go to _Window -> General -> Test Runner_
2. On the _Test Runner_ window, click _Run All_ to run the suite of unit tests.
3. Verify all tests execute successfully

# References

[1] Sloan, Scott W. "A fast algorithm for generating constrained Delaunay triangulations." Computers & Structures 47.3 (1993): 441-450.

[2] Lawson, Charles L. "Software for C1 surface interpolation." Mathematical software. Academic Press, 1977. 161-194.

[3] Cline, A. K., and R. L. Renka. "A storage-efficient method for construction of a Thiessen triangulation." The Rocky Mountain Journal of Mathematics (1984): 119-139.

