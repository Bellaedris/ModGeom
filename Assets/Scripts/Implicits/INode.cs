using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vxl
{
    public interface INode
    {
        public float Distance(Vector3 point);
        public Bounds GetBounds();
    }
}