using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using vxl;

[System.Serializable]
public struct SphereParams
{
    public Vector3 center;
    public float radius;
}

public enum PrimitiveTypes
{
    Sphere,
    Taurus,
    Plane,
    Octahedron
}

public enum PrimitiveOperator
{
    Union,
    Intersection,
    Difference
}

public class Voxelizer : MonoBehaviour
{
    public GameObject voxelPrefab;
    [Tooltip("Gameobject that will wrap our voxels (just for convenience)")]
    public GameObject parent;
    [Tooltip("Material to avoid a bunch of pink voxels for the URP people out there")]
    public Material mat;
    [Tooltip("Maximal depth of our subdivision. A max depth of 5 will give you up to 8^5, be careful not to input huge numbers.")]
    public int voxelizationLevel = 1;
    public PrimitiveTypes primitive = PrimitiveTypes.Sphere;
    [Tooltip("Operator used to combine your spheres")]
    public PrimitiveOperator primitiveOperator = PrimitiveOperator.Union;
    [Header("spheres params")]
    [SerializeField]
    public SphereParams[] spheres;
    [Header("Taurus params")]
    public Vector3 taurusCenter;
    public float taurusOuterRadius;
    public float taurusInnerRadius;
    [Header("Plane params")] 
    [Tooltip("Normal direction of the plane. Doesn't need to be normalized")]
    public Vector3 planeNormal;
    public float planeDistance;
    [Header("Octahedron params")]
    public Vector3 octahedronCenter;
    public float octahedronLength;

    private INode _scene;

    public void Voxelize()
    {
        float begin = Time.realtimeSinceStartup;
        switch (primitive)
        {
            case PrimitiveTypes.Octahedron:
                _scene = new Octahedron(octahedronCenter, octahedronLength);
                break;
            case PrimitiveTypes.Plane:
                _scene = new vxl.Plane(planeNormal, planeDistance);
                break;
            case PrimitiveTypes.Taurus:
                _scene = new Taurus(taurusCenter, taurusInnerRadius, taurusOuterRadius);
                break;
            case PrimitiveTypes.Sphere:
                _scene = new Sphere(spheres[0].center, spheres[0].radius);
                for (int i = 1; i < spheres.Length; i++)
                {
                    _scene = primitiveOperator switch
                    {
                        PrimitiveOperator.Union => new Union(new Sphere(spheres[i].center, spheres[i].radius),
                            new Sphere(spheres[i - 1].center, spheres[i - 1].radius)),
                        PrimitiveOperator.Intersection => new Intersection(
                            new Sphere(spheres[i].center, spheres[i].radius),
                            new Sphere(spheres[i - 1].center, spheres[i - 1].radius)),
                        PrimitiveOperator.Difference => new Difference(new Sphere(spheres[i].center, spheres[i].radius),
                            new Sphere(spheres[i - 1].center, spheres[i - 1].radius)),
                        _ => _scene
                    };
                }
                break;
            default:
                _scene = new Sphere(new Vector3(0, 0, 0), .5f);
                break;
        }

        Bounds b = _scene.GetBounds();
        OctreeNode node = new OctreeNode(b, 0, voxelizationLevel);
        node.Voxelize(_scene, 0, voxelizationLevel, b.size.x,  ref parent, ref mat, ref voxelPrefab);

        Debug.Log("Voxelized " + parent.transform.childCount + " voxels in " + (Time.realtimeSinceStartup - begin) + "s");
    }

    public void Clear()
    {
        // this is ugly but deleting all voxels objects is cumbersome
        while (parent.transform.childCount > 0)
            GameObject.DestroyImmediate(parent.transform.GetChild(0).gameObject);
    }
}

// Benchmark:
// 5 lvl, Sphere:
// 17256 in 0.5957
//
// 6 lvl, Sphere:
// 137376 in 5.17
//
// 5 lvl, Sphere with adaptative:
// 4264 in 0.1638
//
// 6 lvl, Sphere with adaptative:
// 16384 in 0.79
//
// 7 lvl, Sphere with adaptative:
// 64464 in 4.01
// Imprecisions begin to appear… We should improve our quantification of how much volume is Inside.