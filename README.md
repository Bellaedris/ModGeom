# Project summary
This project is a Unity project developped during my Geometric modelling class for my GAMAGORA master's degree. It includes: 
- generation of simple shapes from code
- import and export of OFF files
- Voxelisation of geometric primitives

# Installation
Clone the repository and use Unity Hub to add the folder containing this repo. 

# Usage
## Shapes
To generate shapes, you can use the default scene to interact with the ShapeGenerator object in the editor. If you want to use your own scene, You have to create an ampty object and attach a ShapeGenerator script to it. The script generates shapes within the editor. You can select the shape to generate and various parameters.

## OFF import
Similarly to Shapes, you can attach a MeshHandler to an object to read OFF meshes. You mmust put the files in Assets/Models and enter the name of the model. The models can be just read, read then modified (deletion of half the triangles), and can be exported afterwards (to OFF or OBJ).

## Voxels
Attach a Voxelizer to an empty gameobject, and play with the settings in the editor. Careful with the voxelization depth, the time it takes is exponential. I recommand not going above 7 to keep smooth generation times.
On the default scene, clicking play allows you to manage a ball that removes potential to the voxels. All voxels receive a fixed potential, proportional to their size. If a voxel has a potential < 0, it is not displayed anymore. Moving the ball out of the voxel will restore its initial potential. 
Use the R and F keys to increase the size of the ball. You can edit how much potential the ball removes in the editor, by clicking the ball. 
it is possible to generate procedural terrains, even tho the way i handled this is kind of ugly. You can and should feel anxious when looking at the voxel color part of the code.