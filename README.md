# OpenFracture
OpenFracture is an open source Unity package for fracturing & slicing meshes.

## Installing

### Package Manager

OpenFracture can be imported using Unity’s built-in Package Manager.

## Features
- **Runtime and Editor Support** - Fracture meshes either at run-time or pre-fracture in the editor for optimal performance.
- **Arbitrary Meshes** - Can fracture or slices any closed, non-self-intersecting mesh. Support for convex and non-convex meshes as well as meshes with multiple holes.
- **UV Remapping** - Texture coordinates are preserved along edges where mesh is fractured/sliced.
- **Custom Material** - Use custom material for inside faces. Supports textures with options for UV scaling & offset.
- **Recursive Fracturing** - Fragments can be broken down into smaller fragments.
- **Multiple Trigger Types** - Trigger fractures using triggers, collisions or pressing a key. Additonal trigger types can be added easily.
- **Tunable Fragment Count** - Directly specify the number of fragments to easily tune performance for different platforms.
- **Asynchronous** - Support for asynchronous fracturing (single-threaded)
- **2D/3D Fracturing** - Ability to specify which planes the mesh will be fractured in. This is useful when a mesh is effectively 2D (e.g. glass) and only needs to be fractured on two planes.

##Limitations
- The algorithm which re-triangulates the cut faces requires a closed polygon that does not self-intersect. This means meshes that aren’t completely closed or have self-intersecting geometry will not fracture properly. To fracture meshes that consist of multiple sets of geometry which intersect each other, you can either A) split geometry into separate meshes or B) remove the self-intersecting geometry in the mesh.