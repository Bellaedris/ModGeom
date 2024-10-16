using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vxl
{
    /*
     * A Taurus is defined by a center, an inner radius (radius inside) and an outer radius (offsetted by the inner radius).
     * If you want a Taurus with a total radius of 1, you need inner radius + outer radius = 1.
     */
    public class Taurus : INode
    {
        private Vector3 _center;
        private float _innerRadius;
        private float _outerRadius;

        private Bounds _aabb;

        public Taurus(Vector3 center, float innerRadius, float outerRadius)
        {
            _center = center;
            _innerRadius = innerRadius;
            _outerRadius = outerRadius;
            
            Vector3 extents = new Vector3(outerRadius + innerRadius, outerRadius + innerRadius, outerRadius + innerRadius);
    
            // Create the bounding box (center and extents)
            _aabb = new Bounds(_center, extents * 2);
        }
        
        /*
         * Thanks Inigo Quilez https://iquilezles.org/articles/distfunctions/ 
         */
        public float Distance(Vector3 point)
        {
            Vector3 localPoint = point - _center;
            
            float lengthXZ = new Vector2(localPoint.x, localPoint.z).magnitude;
            Vector2 q = new Vector2(lengthXZ - _outerRadius, localPoint.y);
            
            return q.magnitude - _innerRadius;
        }

        public Bounds GetBounds()
        {
            return _aabb;
        }
    }
}