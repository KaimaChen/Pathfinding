using System.Collections.Generic;
using Priority_Queue;
using UnityEngine;

public class PathPlanner
{
    private IMap m_map;
    private PlannerNode[] m_nodes;
    private INode m_start;
    private INode m_goal;
    private RectInt? m_limitRect; //限制在哪个区域内寻路
    private readonly SimplePriorityQueue<PlannerNode> m_open = new SimplePriorityQueue<PlannerNode>();

    public PathPlanner(IMap map, RectInt? limitRect)
    {
        m_map = map;
        m_limitRect = limitRect;
        m_nodes = new PlannerNode[map.NodeCount()];
    }

    public Path Search(INode start, INode goal)
    {
        m_start = start;
        m_goal = goal;

        PlannerNode startNode = new PlannerNode(start, 0, m_map.CalcHeuristic(start, goal), NodeStatus.Open);
        m_nodes[start.Id] = startNode;
        m_open.Enqueue(startNode, startNode.F);

        while(m_open.Count > 0)
        {
            PlannerNode curtNode = m_open.Dequeue();
            if (curtNode.OriginNode.Id == m_goal.Id)
                break;

            curtNode.Status = NodeStatus.Close;

            List<INode> successors = m_map.GetSuccessors(curtNode.OriginNode);
            for(int i = 0; i < successors.Count; i++)
            {
                INode successor = successors[i];
                if (m_limitRect != null && !m_limitRect.Value.Contains(successor.Pos))
                    continue;

                PlannerNode node = m_nodes[successor.Id];
                if(node != null)
                {
                    if(node.Status != NodeStatus.Close)
                    {
                        float oldG = node.G;
                        float newG = curtNode.G + m_map.CalcHeuristic(curtNode.OriginNode, successor);
                        if(newG < node.G)
                        {
                            node.G = newG;
                            node.Parent = curtNode;
                        }
                    }
                }
                else
                {
                    float g = curtNode.G + m_map.CalcHeuristic(curtNode.OriginNode, successor);
                    float h = m_map.CalcHeuristic(successor, m_goal);
                    node = new PlannerNode(successor, g, h, NodeStatus.Open);
                    m_open.Enqueue(node, node.F);
                }
            }
        }

        return GeneratePath();
    }

    private Path GeneratePath()
    {
        PlannerNode node = m_nodes[m_goal.Id];
        if (node == null)
            return null;

        Path path = new Path(node.G);
        List<INode> list = new List<INode>() { node.OriginNode };
        while(node.Parent != null)
        {
            node = node.Parent;
            list.Add(node.OriginNode);

            if (node.OriginNode.Id == m_start.Id)
                break;
        }

        list.Reverse();

        path.Nodes = list;
        return path;
    }

    #region Node
    enum NodeStatus
    {
        Open,
        Close,
    }

    class PlannerNode
    {
        public INode OriginNode { get; }
        public float G { get; set; }
        public float H { get; set; }
        public float F { get { return G + H; } }
        public PlannerNode Parent { get; set; }
        public NodeStatus Status { get; set; }

        public PlannerNode(INode originNode, float g, float h, NodeStatus status)
        {
            OriginNode = originNode;
            G = g;
            H = h;
            Status = status;
        }
    }
    #endregion
}