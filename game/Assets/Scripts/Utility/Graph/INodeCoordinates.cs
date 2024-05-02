using UnityEngine;

namespace Utility.Graph
{
    public interface INodeCoordinates
    {
        public Vector3 ToPhysicalSpace { get; }

        public INodeCoordinates Add(INodeCoordinates other);
    }
}