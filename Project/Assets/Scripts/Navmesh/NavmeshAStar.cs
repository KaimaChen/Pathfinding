using System.Collections.Generic;
using Pathfinding.Poly2Tri;
using UnityEngine;
using Priority_Queue;

public class NavmeshAStar
{
    public static NavmeshNode StartNode;
    public static NavmeshNode EndNode;
    private readonly List<NavmeshNode> m_nodes = new List<NavmeshNode>();
    private readonly SimplePriorityQueue<NavmeshNode> m_open = new SimplePriorityQueue<NavmeshNode>();

    public NavmeshAStar(List<DelaunayTriangle> triangles, DelaunayTriangle start, DelaunayTriangle end)
    {
        var dict = new Dictionary<DelaunayTriangle, NavmeshNode>();
        for(int i = 0; i < triangles.Count; i++)
        {
            var centroid = triangles[i].Centroid();
            var center = new Vector2(centroid.Xf, centroid.Yf);
            var node = new NavmeshNode(center);
            dict[triangles[i]] = node;
            m_nodes.Add(node);

            if (triangles[i] == start)
                StartNode = node;
            else if (triangles[i] == end)
                EndNode = node;
        }

        for(int i = 0; i < triangles.Count; i++)
        {
            var t = triangles[i];
            var node = m_nodes[i];

            if (t.Neighbors._0 != null)
            {
                NavmeshNode n;
                if (dict.TryGetValue(t.Neighbors._0, out n))
                    node.AddNeighbor(n);
            }
            if (t.Neighbors._1 != null)
            {
                NavmeshNode n;
                if (dict.TryGetValue(t.Neighbors._1, out n))
                    node.AddNeighbor(n);
            }
            if (t.Neighbors._2 != null)
            {
                NavmeshNode n;
                if (dict.TryGetValue(t.Neighbors._2, out n))
                    node.AddNeighbor(n);
            }
        }
    }

    public void Process()
    {
        StartNode.G = 0;
        AddToOpen(StartNode);

        while(OpenSize() > 0)
        {
            var node = PopOpen();

            if (node == EndNode)
                break;

            node.Closed = true;
            var neighbors = node.Neighbors;
            for(int i = 0; i < neighbors.Count; i++)
            {
                var n = neighbors[i];
                if (!n.Closed)
                {
                    if (!n.Opened)
                        n.SetParent(null, float.MaxValue);

                    UpdateVertex(node, n);
                }
            }
        }
    }

    private void UpdateVertex(NavmeshNode curtNode, NavmeshNode nextNode)
    {
        float oldG = nextNode.G;
        ComputeCost(curtNode, nextNode);

        if(nextNode.G < oldG)
        {
            if (!nextNode.Opened)
                AddToOpen(nextNode);
        }
    }

    private void ComputeCost(NavmeshNode curtNode, NavmeshNode nextNode)
    {
        float cost = curtNode.G + Vector2.SqrMagnitude(curtNode.Center - nextNode.Center);
        if (cost < nextNode.G)
            nextNode.SetParent(curtNode, cost);
    }

    #region Open
    private void AddToOpen(NavmeshNode node)
    {
        m_open.Enqueue(node, node.F);
        node.Opened = true;
    }

    private NavmeshNode PopOpen()
    {
        return m_open.Dequeue();
    }

    private int OpenSize()
    {
        return m_open.Count;
    }
    #endregion
}