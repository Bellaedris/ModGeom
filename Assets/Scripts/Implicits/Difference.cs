using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using vxl;

namespace vxl
{
    public class Difference : INode
    {
        private INode _left;
        private INode _right;
        private Bounds _bounds;

        public Difference(INode left, INode right)
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

            float maxBound = Mathf.Max(_bounds.extents.x, _bounds.extents.y, _bounds.extents.z);
            _bounds.extents = new Vector3(maxBound, maxBound, maxBound);
        }
        
        public float Distance(Vector3 point)
        {
            return Mathf.Max(_left.Distance(point), -_right.Distance(point));
        }

        public Bounds GetBounds()
        {
            return _bounds;
        }

        public Color GetColor(Vector3 point)
        {
            return Color.white;
        }
    }
}