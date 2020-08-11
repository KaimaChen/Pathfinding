using System.Collections.Generic;
using UnityEngine;

public class AbstractEdge : MonoBehaviour, IEdge
{
    [SerializeField] private int m_targetNodeId;
    [SerializeField] private float m_cost;
    [SerializeField] private int m_level;
    [SerializeField] private bool m_isInterEdge;

    public int TargetNodeId { get { return m_targetNodeId; } }
    public float Cost { get { return m_cost; } }
    public int Level { get { return m_level; } }
    public bool IsInterEdge { get { return m_isInterEdge; } }
    public List<INode> InnerLowerLevelPath { get; private set; }

    public void Init(int targetNodeId, float cost, int level, bool isInterEdge)
    {
        m_targetNodeId = targetNodeId;
        m_cost = cost;
        m_level = level;
        m_isInterEdge = isInterEdge;
    }

    public void Release()
    {
        Destroy(gameObject);
    }

    public void SetInnerLowerLevelPath(List<INode> path)
    {
        if (path != null)
            InnerLowerLevelPath = new List<INode>(path);
        else
            InnerLowerLevelPath = null;
    }
}