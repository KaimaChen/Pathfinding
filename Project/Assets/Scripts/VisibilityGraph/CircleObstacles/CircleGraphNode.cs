using UnityEngine;

namespace Pathfinding.VisibilityGraph
{
    public class CircleGraphNode : GraphNode
    {
        public Circle BelongCircle { get; private set; }

        public CircleGraphNode(Vector2 center, Circle c) : base(center)
        {
            BelongCircle = c;
        }
    }
}
