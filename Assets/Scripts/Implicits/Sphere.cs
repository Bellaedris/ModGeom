using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vxl
{
    public class Sphere : INode
    {
        [SerializeField]
        private Vector3 _center;
        [SerializeField]
        private float _radius;
        private Bounds _bounds;

        public Sphere(Vector3 center, float radius)
        {
            _radius = radius;
            _center = center;
            
            float doubleRadius = _radius * 2f;
            _bounds = new Bounds(_center, new Vector3(doubleRadius, doubleRadius, doubleRadius));
        }
        
        public float Distance(Vector3 point)
        {
            return Vector3.Distance(point, _center) - _radius;
        }
        
        public Bounds GetBounds()
        {
            return _bounds;
        }
    }
}
