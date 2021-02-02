using UnityEngine;
using System.Collections.Generic;

public class GraphNode
{
    private GraphNode m_parent;
    private float m_g = float.MaxValue;
    private float m_h = -1;
    private bool m_opened;
    private bool m_closed;
    private readonly List<GraphNode> m_neighbors = new List<GraphNode>();

    public Vector2 Center { get; set; }

    public GraphNode Parent
    {
        get { return m_parent; }
        set { m_parent = value; }
    }

    public List<GraphNode> Neighbors { get { return m_neighbors; } }

    public bool Opened
    {
        get { return m_opened; }
        set
        {
            m_opened = value;
            if (m_opened)
                m_closed = false;
        }
    }

    public bool Closed
    {
        get { return m_closed; }
        set
        {
            m_closed = value;
            if (m_closed)
                m_opened = false;
        }
    }

    public float G
    {
        get { return m_g; }
        set { m_g = value; }
    }

    public GraphNode(Vector2 center)
    {
        Center = center;
    }

    public void AddNeighbor(GraphNode n)
    {
        m_neighbors.Add(n);
    }

    public void SetParent(GraphNode parent, float g)
    {
        m_parent = parent;
        m_g = g;
    }

    public float F(GraphNode endNode)
    {
        if (m_h < 0)
            m_h = (Center - endNode.Center).magnitude;

        return m_g + m_h;
    }
}