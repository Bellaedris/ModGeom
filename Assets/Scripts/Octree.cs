using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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

namespace vxl
{

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

        /// <summary>
        /// Build a dictionnary with, for each voxel, all the vertices contained inside this voxel.
        /// </summary>
        /// <param name="vertices">The vertices of a mesh</param>
        /// <returns>A map of voxels and list of indices</returns>
        public Dictionary<Vector3, List<int>> FindVoxelForEachVertex(Vector3[] vertices)
        {
            var result = new Dictionary<Vector3, List<int>>();

            for (int i = 0; i < vertices.Length; i++)
            {
                // find the voxel our vertex is in
                Vector3 voxel = FindNodeOfVertex(vertices[i]);
                if (!result.ContainsKey(voxel))
                {
                    result.Add(voxel, new List<int>());
                }

                result[voxel].Add(i);
            }

            return result;
        }

        private Vector3 FindNodeOfVertex(Vector3 vertex)
        {
            if(isLeaf)
                return _aabb.Contains(vertex) ? _aabb.center : Vector3.zero;
            
            var position = Vector3.zero;
            foreach (var child in _children)
            {
                position += child.FindNodeOfVertex(vertex);
            }

            return position;
        }

        public bool Contains(INode target)
        {
            if (isLeaf)
                return target.Distance(_aabb.center) <= 0;
            
            bool contains = false;
            for (int i = 0; i < 8; i++)
            {
                contains |= _children[i].Contains(target);
            }

            return contains;
        }

        /*
         * Computes the amount of edges of the voxels that lie within the volume
         */
        public int EdgesInVolume(INode target)
        {
            Vector3[] corners = new Vector3[8];
            Vector3 min = _aabb.min;
            Vector3 max = _aabb.max;

            corners[0] = new Vector3(min.x, min.y, min.z); // Bottom left near
            corners[1] = new Vector3(max.x, min.y, min.z); // Bottom right near
            corners[2] = new Vector3(max.x, max.y, min.z); // Top right near
            corners[3] = new Vector3(min.x, max.y, min.z); // Top left near
            corners[4] = new Vector3(min.x, min.y, max.z); // Bottom left far
            corners[5] = new Vector3(max.x, min.y, max.z); // Bottom right far
            corners[6] = new Vector3(max.x, max.y, max.z); // Top right far
            corners[7] = new Vector3(min.x, max.y, max.z); // Top left far

            int edgesInside = 0;
            foreach (var corner in corners)
                if (target.Distance(corner) <= 0)
                    edgesInside++;

            return edgesInside;
        }

        public void Voxelize(INode target, int currentDepth, int maxDepth, float scale, ref GameObject parent, BiomeParams[] mat, ref GameObject prefab)
        {
            // if the voxel has more than half of its edge in the primitive, we can place a voxel. 
            // otherwise, keep the subdivision going
            int edgesInside = EdgesInVolume(target);

            // if the voxel has no volume inside, return
            // highly inefficient... I'm recursing through all the voxels.
            // there's a lot of time to gain here but i'm not sure how yet.
            // TODO handle this horrible bottleneck
            if (!Contains(target))
                return;

            if (edgesInside > 7 || isLeaf || currentDepth == maxDepth)
            {
                // create the gameobject
                GameObject cube = GameObject.Instantiate(prefab);
                cube.transform.position = _aabb.center;
                float levelScale = Mathf.Pow(2f, -currentDepth);
                cube.transform.localScale = new Vector3(scale * levelScale, scale * levelScale, scale * levelScale);

                // MeshFilter filter = cube.AddComponent<MeshFilter>();
                // filter.sharedMesh = Utils.CreateMeshFromBounds(_aabb);
                var renderer = cube.GetComponent<MeshRenderer>();
                // renderer.sharedMaterial = mat;
                // renderer.sharedMaterial.color = target.GetColor(_aabb.center);
                
                //handle mesh color. A bit ugly but since most voxels don't have a real meaning to a color....
                Color biomeColor = target.GetColor(_aabb.center);
                for (int i = 0; i < mat.Length; i++)
                {
                    if (mat[i].color.color == biomeColor)
                        renderer.sharedMaterial = mat[i].color;
                }
                
                cube.transform.SetParent(parent.transform);
                var voxel = cube.GetComponent<Voxel>();
                // bigger voxels have bigger potential
                voxel.AddPotential(100f * levelScale);
            }
            else
            {
                foreach (var child in _children)
                {
                    child.Voxelize(target, currentDepth + 1, maxDepth, scale, ref parent, mat, ref prefab);
                }
            }
        }
    }
}