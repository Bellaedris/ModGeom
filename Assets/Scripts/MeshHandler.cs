using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshHandler : MonoBehaviour
{
    public enum MeshFormat
    {
        OFF,
        OBJ
    }
    
    [Tooltip("Name of the .off file to read in Assets/Models")]
    public string offFileName;

    public Material mat;

    [Tooltip("Use computed face normals or Unity's RecalculateNormals")]
    public bool useFaceNormals;
    [Tooltip("Edit the model by reomving half the triangles")]
    public bool edit;
    [Tooltip("Rewrite the model after reading and eventually editing")]
    public bool rewrite;
    public MeshFormat writingFormat;
    
    public void HandleOFFFile()
    {
        OFFMesh off = OFFReader.ReadFile(offFileName, useFaceNormals);

        if (edit)
            OFFReader.removeTrianglesFromOFF(ref off);
        if (rewrite)
            switch (writingFormat)
            {
                case MeshFormat.OFF:
                    OFFReader.WriteOFF(offFileName, ref off);
                    break;
                case MeshFormat.OBJ:
                    OFFReader.WriteOBJ(offFileName, ref off);
                    break;
            }
        
        // spawn the thing in the world
        GameObject shape = new GameObject(offFileName);
        shape.transform.position = -off._centroid;
    
        MeshFilter filter = shape.AddComponent<MeshFilter>();
        filter.sharedMesh = off._m;
        var renderer = shape.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = mat;
    }
}
