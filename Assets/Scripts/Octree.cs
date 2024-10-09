using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using vxl;

// public interface IOctreeNode
// {
//     public bool Contains(INode target);
// }
//
// public class OctreeLeaf : IOctreeNode
// {
//     private Bounds _aabb;
//
//     OctreeLeaf(Bounds aabb)
//     {
//         _aabb = aabb;
//     }
//     
//     public bool Contains(INode target)
//     {
//         return target.Distance(_aabb.center) <= 0;
//     }
// }

public class OctreeNode
{
    private List<OctreeNode> _children = new List<OctreeNode>(8);
    private Bounds _aabb;
    private bool isLeaf;

    public OctreeNode(Bounds bounds, int currentDepth, int maxDepth)
    {
        _aabb = bounds;
        isLeaf = currentDepth == maxDepth;

        // build childs
        if (currentDepth < maxDepth)
        {
            int index = 0;
            for (int x = -1; x <= 1; x += 2)
            {
                float newX = _aabb.center.x + _aabb.extents.x / 2f * x;
                for (int y = -1; y <= 1; y += 2)
                {
                    float newY = _aabb.center.y + _aabb.extents.y / 2f * y;
                    for (int z = -1; z <= 1; z += 2)
                    {
                        Vector3 center = new Vector3(newX, newY, _aabb.center.z + _aabb.extents.z / 2f * z);
                        Bounds newAABB = new Bounds(center, _aabb.extents);
                        
                        _children.Add(new OctreeNode(newAABB, currentDepth + 1, maxDepth));
                    }
                }
            }
        }
    }

    public bool Contains(INode target)
    {
        if (isLeaf)
        {
            return target.Distance(_aabb.center) <= 0;
        }
        else
        {
            bool contains = false;
            for (int i = 0; i < 8; i++)
            {
                contains |= _children[i].Contains(target);
            }

            return contains;
        }
    }

    public void Voxelize(INode target, int currentDepth, int maxDepth, ref GameObject parent, ref Material mat)
    {
        if (!Contains(target))
            return;
        
        if (isLeaf || currentDepth == maxDepth)
        {
            // create the gameobject
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = _aabb.center;
            float scale = Mathf.Pow(2f, -currentDepth);
            cube.transform.localScale = new Vector3(scale, scale, scale);
    
            // MeshFilter filter = cube.AddComponent<MeshFilter>();
            // filter.sharedMesh = Utils.CreateMeshFromBounds(_aabb);
            var renderer = cube.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = mat;
            cube.transform.SetParent(parent.transform);
        }
        else
        {
            foreach (var child in _children)
            {
                child.Voxelize(target, currentDepth + 1, maxDepth, ref parent, ref mat);
            }
        }
    }
}
