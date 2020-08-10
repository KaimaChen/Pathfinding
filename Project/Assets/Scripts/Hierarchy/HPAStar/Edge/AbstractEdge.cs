using System.Collections.Generic;

public class AbstractEdge : IEdge
{
    public int TargetNodeId { get; }
    public float Cost { get; }
    public int Level { get; }
    public bool IsInterEdge { get; }
    public List<INode> InnerLowerLevelPath { get; private set; }

    public AbstractEdge(int targetNodeId, float cost, int level, bool isInterEdge)
    {
        TargetNodeId = targetNodeId;
        Cost = cost;
        Level = level;
        IsInterEdge = isInterEdge;
    }

    public void SetInnerLowerLevelPath(List<INode> path)
    {
        if (path != null)
            InnerLowerLevelPath = new List<INode>(path);
        else
            InnerLowerLevelPath = null;
    }
}