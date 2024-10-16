using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vxl
{
    public abstract class Node : INode
    {
        protected Bounds _bounds;
        
        public float Distance(Vector3 point)
        {
            return Mathf.Infinity;
        }

        public Bounds GetBounds()
        {
            return _bounds;
        }

        public Color GetColor()
        {
            return Color.white;
        }
    }
}