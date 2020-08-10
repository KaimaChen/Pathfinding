using UnityEngine;
using System.Collections.Generic;

public class AbstractNode : INode
{
    public int Id { get; }
    public int Level { get; set; }

    public int ClusterId { get; }

    public Vector2Int Pos { get; }

    public Dictionary<int, AbstractEdge> Edges { get; } = new Dictionary<int, AbstractEdge>();

    public AbstractNode(int id, int level, int clusterId, Vector2Int concretePos)
    {
        Id = id;
        Level = level;
        ClusterId = clusterId;
        Pos = concretePos;
    }

    public void AddEdge(AbstractEdge edge)
    {
        AbstractEdge originEdge;
        if (!Edges.TryGetValue(edge.TargetNodeId, out originEdge) || originEdge.Level < edge.Level)
            Edges[edge.TargetNodeId] = edge;
    }

    public void RemoveEdge(int targetNodeId)
    {
        Edges.Remove(targetNodeId);
    }

    public void ClearEdges()
    {
        Edges.Clear();
    }
}