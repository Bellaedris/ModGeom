using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using vxl;

public class Voxelizer : MonoBehaviour
{
    public GameObject parent;
    public Material mat;
    public int voxelizationLevel = 1;
    [SerializeField]
    public Sphere[] spheres;

    public void Voxelize()
    {
        INode scene = new Sphere(Vector3.zero, .5f);
        for (int i = 0; i < spheres.Length - 1; i++)
        {
            scene = new Union(spheres[i], spheres[i + 1]);
        }

        OctreeNode node = new OctreeNode(scene.GetBounds(), 0, voxelizationLevel);
        node.Voxelize(scene, 0, voxelizationLevel, ref parent, ref mat);
    }

    public void Clear()
    {
        // this is ugly but deleting all voxels objects is cumbersome
        while (parent.transform.childCount > 0)
            GameObject.DestroyImmediate(parent.transform.GetChild(0).gameObject);
    }
}
