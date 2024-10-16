using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vxl
{
    /// <summary>
    /// A plane defined by a normal and a distance along  this normal
    /// </summary>
    public class Plane : INode
    {
        private Vector3 _normal;
        private float _height;
        private Bounds _bounds;

        public Plane(Vector3 normal, float height)
        {
            _normal = normal.normalized;
            _height = height;
            _bounds = new Bounds(Vector3.zero + normal * height, Vector3.one);
        }
        
        public float Distance(Vector3 point)
        {
            return Vector3.Dot(point, _normal) + _height;
        }

        public Bounds GetBounds()
        {
            // an infinite plane doesn't really has bounds....
            // we return a unit square instead
            return _bounds;
        }

        public Color GetColor(Vector3 point)
        {
            return Color.white;
        }
    }

}
