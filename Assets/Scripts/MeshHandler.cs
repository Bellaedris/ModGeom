using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using vxl;

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
    
    [Header("Mesh simplification")]
    public MeshFilter meshToSimplify;
    public int octreeDepth = 3;
    
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

    public void Simplify()
    {
        var vertices = meshToSimplify.sharedMesh.vertices;
        var indices = meshToSimplify.sharedMesh.triangles;
        OctreeNode oc = new OctreeNode(meshToSimplify.sharedMesh.bounds, 0, octreeDepth);
        
        var voxelsToVertices = oc.FindVoxelForEachVertex(meshToSimplify.sharedMesh.vertices);
        var replaceIndexWith = new Dictionary<int, int>();
        
        var newVertices = new List<Vector3>();
        var newIndices = new List<int>();

        foreach (var voxelContent in voxelsToVertices)
        {
            // find the barycenter of the voxels
            var barycenterIndex = voxelContent.Value[0];
            Vector3 barycenter = vertices[voxelContent.Value[0]];
            for(var i = 1; i < voxelContent.Value.Count; i++)
            {
                barycenter += vertices[voxelContent.Value[i]];
            }
            
            barycenter /= voxelContent.Value.Count;
            int newBarycenterIndex = newVertices.Count;
            newVertices.Add(barycenter);
            
            foreach (var index in voxelContent.Value)
            {
                // store all the vertices that should be deleted (map with the one to replace with)
                replaceIndexWith.Add(index, newBarycenterIndex);
            }
            
            // move the first voxel to the barycenter
            vertices[voxelContent.Value[0]] = barycenter;
        }
        
        // replace all the indices to replace with the new barycenter index
        for (int i = 0; i < indices.Length; i += 3)
        {
            int a = replaceIndexWith[indices[i]];
            int b = replaceIndexWith[indices[i + 1]];
            int c = replaceIndexWith[indices[i + 2]];

            // only insert unique triangles
            if (a != b && a != c && b != c)
            {
                newIndices.Add(a);
                newIndices.Add(b);
                newIndices.Add(c);
            }
        }
        
        meshToSimplify.sharedMesh.Clear();
        meshToSimplify.sharedMesh.SetVertices(newVertices);
        meshToSimplify.sharedMesh.SetTriangles(newIndices, 0);
        meshToSimplify.sharedMesh.RecalculateNormals();
        
        
        
        if (rewrite)
        {
            OFFMesh off;
            off._m = meshToSimplify.sharedMesh;
            off._centroid = Vector3.zero;
            switch (writingFormat)
            {
                case MeshFormat.OFF:
                    OFFReader.WriteOFF(offFileName, ref off);
                    break;
                case MeshFormat.OBJ:
                    OFFReader.WriteOBJ(offFileName, ref off);
                    break;
            }
        }
    }
}
