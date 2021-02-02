using Priority_Queue;

public class GraphAStar
{
    private readonly GraphNode m_startNode;
    private readonly GraphNode m_endNode;

    private readonly SimplePriorityQueue<GraphNode> m_open = new SimplePriorityQueue<GraphNode>();

    public GraphAStar(GraphNode startNode, GraphNode endNode)
    {
        m_startNode = startNode;
        m_endNode = endNode;
    }

    public void Process()
    {
        m_startNode.G = 0;
        AddToOpen(m_startNode);

        while (OpenSize() > 0)
        {
            var node = PopOpen();

            if (node == m_endNode)
                break;

            node.Closed = true;
            var neighbors = node.Neighbors;
            for (int i = 0; i < neighbors.Count; i++)
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

    private void UpdateVertex(GraphNode curtNode, GraphNode nextNode)
    {
        float oldG = nextNode.G;
        ComputeCost(curtNode, nextNode);

        if (nextNode.G < oldG)
        {
            if (!nextNode.Opened)
                AddToOpen(nextNode);
            else
                m_open.UpdatePriority(nextNode, nextNode.F(m_endNode));
        }
    }

    private void ComputeCost(GraphNode curtNode, GraphNode nextNode)
    {
        float cost = curtNode.G + (curtNode.Center - nextNode.Center).magnitude;
        if (cost < nextNode.G)
            nextNode.SetParent(curtNode, cost);
    }

    #region Open
    private void AddToOpen(GraphNode node)
    {
        m_open.Enqueue(node, node.F(m_endNode));
        node.Opened = true;
    }

    private GraphNode PopOpen()
    {
        return m_open.Dequeue();
    }

    private int OpenSize()
    {
        return m_open.Count;
    }
    #endregion
}