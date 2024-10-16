using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vxl
{
    public class Octahedron : INode
    {
        private Vector3 _center;
        private float _sideLength;
        private Bounds _bounds;

        public Octahedron(Vector3 center, float sideLength)
        {
            _center = center;
            _sideLength = sideLength;
            
            _bounds = new Bounds(_center, new Vector3(_sideLength * 2f, _sideLength * 2f, _sideLength * 2f));
        }
        
        /// <summary>
        /// Thanks Inigo Quilez https://iquilezles.org/articles/distfunctions/
        /// </summary>
        /// <param name="point">A point in space</param>
        /// <returns>Distance from point to an octahedron</returns>
        public float Distance(Vector3 point)
        {
            Vector3 worldPoint = _center - point;
            
            worldPoint = new Vector3(Mathf.Abs(worldPoint.x), Mathf.Abs(worldPoint.y), Mathf.Abs(worldPoint.z));
            Vector3 worldPointYZX = new Vector3(worldPoint.y, worldPoint.z, worldPoint.x);
            Vector3 worldPointZXY = new Vector3(worldPoint.z, worldPoint.x, worldPoint.y);
            
            float m = worldPoint.x+worldPoint.y+worldPoint.z-_sideLength;
            Vector3 q;
            if( 3.0f * worldPoint.x < m ) q = worldPoint;
            else if( 3.0f * worldPoint.y < m ) q = worldPointYZX;
            else if( 3.0f * worldPoint.z < m ) q = worldPointZXY;
            else return m * 0.57735027f;
    
            float k = Mathf.Clamp(0.5f* (q.z - q.y+ _sideLength),0.0f,_sideLength); 
            return new Vector3(q.x,q.y-_sideLength+k,q.z-k).magnitude;
        }

        public Bounds GetBounds()
        {
            return _bounds;
        }
    }
}