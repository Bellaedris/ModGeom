# Project summary
This project is a Unity project developped during my Geometric modelling class for my GAMAGORA master's degree. It includes: 
- generation of simple shapes from code

# Installation
Clone the repository and use Unity Hub to add the folder containing this repo. 

# Usage
## Shapes
To generate shapes, you can use the default scene to interact with the ShapeGenerator object in the editor. If you want to use your own scene, You have to create an ampty object and attach a ShapeGenerator script to it. The script generates shapes within the editor. You can select the shape to generate and various parameters.

## OFF import
Similarly to Shapes, you can attach a MeshHandler to an object to read OFF meshes. You mmust put the files in Assets/Models and enter the name of the model. The models can be just read, read then modified (deletion of half the triangles), and can be exported afterwards (to OFF or OBJ).
