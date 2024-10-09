using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using vxl;

namespace vxl
{
    public class Union : INode
    {
        private INode _left;
        private INode _right;
        private Bounds _bounds;

        public Union(INode left, INode right)
        {
            _left = left;
            _right = right;

            Bounds leftBounds = left.GetBounds();
            Bounds rightBounds = right.GetBounds();
            leftBounds.Expand(rightBounds.min);
            leftBounds.Expand(rightBounds.max);
            
            _bounds = leftBounds;
        }
        
        public float Distance(Vector3 point)
        {
            return Mathf.Min(_left.Distance(point), _right.Distance(point));
        }

        public Bounds GetBounds()
        {
            return _bounds;
        }
    }
}

