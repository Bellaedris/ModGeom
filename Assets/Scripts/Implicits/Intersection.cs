using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vxl
{

    public class Intersection : INode
    {
        private INode _left;
        private INode _right;
        private Bounds _bounds;

        public Intersection(INode left, INode right)
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
            return Mathf.Max(_left.Distance(point), _right.Distance(point));
        }

        public Bounds GetBounds()
        {
            return _bounds;
        }
    }
}
