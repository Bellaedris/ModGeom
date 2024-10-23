using System.Collections.Generic;
using Unity.Jobs;
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
    Octahedron,
    Terrain
}

public enum PrimitiveOperator
{
    Union,
    Intersection,
    Difference
}

[System.Serializable]
public struct BiomeParams
{
    public float height;
    public Material color;
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
    [Header("Terrain params")] 
    public int terrainSeed;
    public float terrainScale;
    public int terrainOctaves;
    public float terrainGain;
    public float terrainLacunarity;
    public float terrainMaxHeight;
    public BiomeParams[] biomes;
    public float chunkSize = 1f;
    
    private INode _scene;
    private Dictionary<Vector2, INode> _chunks;
    private PlayerController _player;
    private Vector2 _currentChunk;
    
    private Vector2[] _neighbors =
    {
        new Vector2(0, 0),
        new Vector2(1, 0),
        new Vector2(-1, 0),
        new Vector2(0, 1),
        new Vector2(0, -1),
    };

    private void Start()
    {
        _player = FindObjectOfType<PlayerController>();
        _chunks = new Dictionary<Vector2, INode>();
        
        // generate the chunk the player spawns on at start, and its neighbors
        Vector2 playerChunk = new Vector2(Mathf.Round(_player.transform.position.x / chunkSize), Mathf.Round(_player.transform.position.z / chunkSize));
        _currentChunk = playerChunk;
        GenerateChunk(playerChunk);
        GenerateNeighboringChunks(playerChunk);
    }

    private void Update()
    {
        // if the player is visiting a new chunk, generate its neighbors with decreasing LoD
        Vector2 playerChunk = new Vector2(Mathf.Round(_player.transform.position.x / chunkSize), Mathf.Round(_player.transform.position.z / chunkSize));
        if (!_currentChunk.Equals(playerChunk))
            GenerateNeighboringChunks(playerChunk);
    }
    
    public void GenerateChunk(Vector2 chunk)
    {
        if (!_chunks.ContainsKey(chunk))
        {
            _chunks[chunk] = new Noise(planeNormal,
                new Vector3(chunk.x, 0, chunk.y),
                planeDistance,
                terrainSeed,
                terrainOctaves,
                terrainScale,
                terrainGain,
                terrainLacunarity,
                terrainMaxHeight);
            VoxelizeNode(_chunks[chunk]);
        }
    }

    public void GenerateNeighboringChunks(Vector2 chunk)
    {
        foreach (Vector2 neighbor in _neighbors)
        {
            Vector2 current = chunk + neighbor;
            if (!_chunks.ContainsKey(current))
            {
                _chunks[current] = new Noise(planeNormal,
                    new Vector3(current.x, 0, current.y),
                    planeDistance,
                    terrainSeed,
                    terrainOctaves,
                    terrainScale,
                    terrainGain,
                    terrainLacunarity,
                    terrainMaxHeight);
                VoxelizeNode(_chunks[current]);
            }
        }
    }

    public void Voxelize()
    {
        float begin = Time.realtimeSinceStartup;
        switch (primitive)
        {
            case PrimitiveTypes.Terrain:
                _scene = new Noise(planeNormal,
                        Vector3.zero,
                    planeDistance,
                    terrainSeed,
                    terrainOctaves,
                    terrainScale,
                    terrainGain,
                    terrainLacunarity,
                    terrainMaxHeight);
                break;
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
        node.Voxelize(_scene, 0, voxelizationLevel, b.size.x,  ref parent, biomes, ref voxelPrefab);

        Debug.Log("Voxelized " + parent.transform.childCount + " voxels in " + (Time.realtimeSinceStartup - begin) + "s");
    }

    void VoxelizeNode(INode scene)
    {
        float begin = Time.realtimeSinceStartup;
        Bounds b = scene.GetBounds();
        OctreeNode node = new OctreeNode(b, 0, voxelizationLevel);
        node.Voxelize(scene, 0, voxelizationLevel, b.size.x,  ref parent, biomes, ref voxelPrefab);

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