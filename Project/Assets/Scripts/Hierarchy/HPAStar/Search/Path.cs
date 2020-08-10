using System.Collections.Generic;
using UnityEngine;

public class Path
{
    public List<INode> Nodes { get; set; }
    public float Cost { get; }

    public Path(float cost)
    {
        Cost = cost;
    }

    public Path(List<INode> nodes, float cost)
    {
        Nodes = nodes;
        Cost = cost;
    }
}