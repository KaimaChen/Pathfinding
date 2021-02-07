using UnityEngine;

namespace Pathfinding.VisibilityGraph
{
    public class CircleGraphAStar : GraphAStar
    {
        public CircleGraphAStar(CircleGraphNode startNode, CircleGraphNode endNode) : base(startNode, endNode) { }

        protected override void ComputeCost(GraphNode curtNode, GraphNode nextNode)
        {
            float cost = curtNode.G + EdgeCost(curtNode, nextNode);
            if (cost < nextNode.G)
                nextNode.SetParent(curtNode, cost);
        }

        private float EdgeCost(GraphNode n1, GraphNode n2)
        {
            var a = n1 as CircleGraphNode;
            var b = n2 as CircleGraphNode;

            if(a.BelongCircle != null && a.BelongCircle == b.BelongCircle)
            {
                var circle = a.BelongCircle;
                float aAngle = Utils.Facing(circle.center, a.Center);
                float bAngle = Utils.Facing(circle.center, b.Center);
                float deltaAngle = Utils.AngleDifference(aAngle, bAngle);
                return deltaAngle * circle.radius;
            }
            else
            {
                return Vector2.Distance(a.Center, b.Center);
            }
        }
    }
}
