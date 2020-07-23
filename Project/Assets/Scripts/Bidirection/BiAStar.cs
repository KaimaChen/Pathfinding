using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

public class BiAStar : AStar
{
    private readonly SimplePriorityQueue<Vector2Int> m_startOpen = new SimplePriorityQueue<Vector2Int>();
    private readonly SimplePriorityQueue<Vector2Int> m_endOpen = new SimplePriorityQueue<Vector2Int>();

    public BiAStar(SearchNode start, SearchNode goal, SearchNode[,] nodes, float weight, float showTime)
        : base(start, goal, nodes, weight, showTime)
    {

    }

    public override IEnumerator Process()
    {
        m_mapStart.G = 0;
        AddToOpenList(m_mapStart, true);

        m_mapGoal.G = 0;
        AddToOpenList(m_mapGoal, false);

        SearchNode startStopNode = null, endStopNode = null;

        while(m_startOpen.Count > 0 && m_endOpen.Count > 0)
        {
            //处理start方向
            ProcessStart(ref startStopNode, ref endStopNode);
            yield return new WaitForSeconds(m_showTime); //等待一点时间，以便观察
            if (startStopNode != null && endStopNode != null)
                break;

            //处理end方向
            ProcessEnd(ref startStopNode, ref endStopNode);
            yield return new WaitForSeconds(m_showTime); //等待一点时间，以便观察
            if (startStopNode != null && endStopNode != null)
                break;
        }

        GeneratePath(startStopNode, endStopNode);

        yield break;
    }

    private void ProcessStart(ref SearchNode startStopNode, ref SearchNode endStopNode)
    {
        Vector2Int curtPos = PopOpenList(true);
        SearchNode curtNode = GetNode(curtPos);

        #region show
        curtNode.SetSearchType(SearchType.Expanded, true);
        #endregion

        curtNode.Closed = true;

        List<SearchNode> neighbors = GetNeighbors(curtNode);
        for (int i = 0; i < neighbors.Count; i++)
        {
            SearchNode neighbor = neighbors[i];
            if(neighbor.IsEndOpen())
            {
                startStopNode = curtNode;
                endStopNode = neighbor;
                return;
            }

            if (neighbor.Closed == false)
            {
                if (neighbor.IsStartOpen() == false)
                    neighbor.SetParent(null, float.MaxValue);

                UpdateVertex(curtNode, neighbor, true);
            }
        }
    }

    private void ProcessEnd(ref SearchNode startStopNode, ref SearchNode endStopNode)
    {
        Vector2Int curtPos = PopOpenList(false);
        SearchNode curtNode = GetNode(curtPos);

        #region show
        curtNode.SetSearchType(SearchType.Expanded, true);
        #endregion

        curtNode.Closed = true;

        List<SearchNode> neighbors = GetNeighbors(curtNode);
        for (int i = 0; i < neighbors.Count; i++)
        {
            SearchNode neighbor = neighbors[i];
            if (neighbor.IsStartOpen())
            {
                startStopNode = curtNode;
                endStopNode = neighbor;
                return;
            }

            if (neighbor.Closed == false)
            {
                if (neighbor.IsEndOpen() == false)
                    neighbor.SetParent(null, float.MaxValue);

                UpdateVertex(curtNode, neighbor, false);
            }
        }
    }

    private void AddToOpenList(SearchNode node, bool isStart)
    {
        if(isStart)
        {
            m_startOpen.Enqueue(node.Pos, node.F(m_weight));
            node.SetStartOpen();
        }
        else
        {
            m_endOpen.Enqueue(node.Pos, node.F(m_weight));
            node.SetEndOpen();
        }

        node.SetSearchType(SearchType.Open, true);
    }

    private Vector2Int PopOpenList(bool isStart)
    {
        if (isStart)
            return m_startOpen.Dequeue();
        else
            return m_endOpen.Dequeue();
    }

    private void UpdateVertex(SearchNode curtNode, SearchNode nextNode, bool isStart)
    {
        float oldG = nextNode.G;
        ComputeCost(curtNode, nextNode);

        if (nextNode.G < oldG)
        {
            if (nextNode.Opened == false)
                AddToOpenList(nextNode, isStart);
        }
    }

    private void GeneratePath(SearchNode startStopNode, SearchNode endStopNode)
    {
        if (startStopNode == null || endStopNode == null)
            return;

        SearchNode lastNode = startStopNode;
        while(lastNode != null)
        {
            lastNode.SetSearchType(SearchType.Path, true);
            lastNode = lastNode.Parent;
        }

        lastNode = endStopNode;
        while (lastNode != null)
        {
            lastNode.SetSearchType(SearchType.Path, true);
            lastNode = lastNode.Parent;
        }
    }
}
