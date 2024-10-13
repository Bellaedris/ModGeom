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

            _bounds = new Bounds();
            Bounds leftBounds = left.GetBounds();
            Bounds rightBounds = right.GetBounds();
            _bounds.Encapsulate(leftBounds.min);
            _bounds.Encapsulate(leftBounds.max);
            _bounds.Encapsulate(rightBounds.min);
            _bounds.Encapsulate(rightBounds.max);
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

