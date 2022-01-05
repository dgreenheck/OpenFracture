# Getting Started

There are three separate components included in this package. Click on the links below to see documentation for that specific script.

- [Fracture](/Documentation~/Fracture.md) - Break meshes into pieces during runtime.
- [Prefracture](/Documentation~/Prefracture.md) - Used for pre-fracturing meshes in the editor. The generated fragments can either be saved directly in the scene or saved to disk if you would like to create a prefab.
- [Slice](/Documentation~/Slice.md) - Runtime slicing of meshes

## Tips
Before using the scripts, there are a few limitations and considerations to be aware of.

### 1. If you are importing a custom mesh, you must set "Read/Write Enabled" to true in the Import settings. Otherwise you will get an error.

![Screenshot 2022-01-03 203011](https://user-images.githubusercontent.com/3814912/148001795-2fad5714-b927-43b8-9e2a-9a9ad711f726.png)

### 2. Meshes must be be non-intersecting and closed. If not, the re-triangulation will fail.

![Screenshot 2022-01-03 203659](https://user-images.githubusercontent.com/3814912/148002307-a2297807-5dfa-4758-ae9a-2eb25b45f039.png)

Depicted above is the wireframe model of a stool. Notice how the crossbars intersect the legs of the stool. Intersecting geometry will cause the triangulation algorithm (which fills in the newly cut faces) to fail and will result in artifacts like you see below. The triangulation algorithm I implemented does not handle self-intersecting polygons. Detecting self-intersecting geometry is a non-trivial problem and is not something I plan on adding.

![Screenshot 2022-01-03 204203](https://user-images.githubusercontent.com/3814912/148002632-904d4d53-691c-4099-98c5-7e2c8240360a.png)

### 3. Optimizing Performance

The fracturing process is a computationally intensive process. Simple models (several hundred vertices) can be fractured into dozens of pieces without much of a hitch. The performance of fracturing complex models consisting of several thousands vertices during runtime will likely be quite slow on older, less capable machines.

Here are some tips for optimizing performance
1. Reduce number of fragments
2. Disable or reduce the number of refracturing interations
3. Prefracture models in the editor rather than fracturing during runtime
4. Bring your model into a 3D modeling program like Blender and use the Decimation tool to simplify the mesh geometry
