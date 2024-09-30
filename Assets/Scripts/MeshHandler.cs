using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshHandler : MonoBehaviour
{
    [Tooltip("Name of the .off file to read in Assets/Models")]
    public string offFileName;

    public Material mat;

    [Tooltip("Use computed face normals or Unity's RecalculateNormals")]
    public bool useFaceNormals;
    [Tooltip("Edit the model and rewrite the edited version")]
    public bool editAndRewrite;
    
    public void HandleOFFFile()
    {
        OFFMesh off = OFFReader.ReadFile(offFileName, useFaceNormals);

        Exception e = new Exception("Error while deleting faces");
        OFFReader.removeTrianglesFromOFF(ref off);
        
        OFFReader.WriteOFF(offFileName, off);
        
        // spawn the thing in the world
        GameObject shape = new GameObject(offFileName);
        shape.transform.position = -off._centroid;
    
        MeshFilter filter = shape.AddComponent<MeshFilter>();
        filter.sharedMesh = off._m;
        var renderer = shape.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = mat;
    }
}
