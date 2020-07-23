using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

/// <summary>
/// A*寻路
/// </summary>
public class AStar : BaseSearchAlgo
{
    protected readonly float m_weight = 1;
    private readonly SimplePriorityQueue<Vector2Int> m_open = new SimplePriorityQueue<Vector2Int>();

    public AStar(SearchNode start, SearchNode goal, SearchNode[,] nodes, float weight, float showTime)
        : base(start, goal, nodes, showTime)
    {
        m_weight = weight;
    }

    public override IEnumerator Process()
    {
        //离线预处理，这里为了展示方便所以放到Process()里
        Preprocess();

        m_mapStart.G = 0;

        AddToOpenList(m_mapStart);
        while (OpenListSize() > 0)
        {
            Vector2Int curtPos = PopOpenList();
            SearchNode curtNode = GetNode(curtPos);

            SetVertex(curtNode);

            if (curtPos == m_mapGoal.Pos) //找到终点
            {
                break;
            }
            else
            {
                #region show
                yield return new WaitForSeconds(m_showTime); //等待一点时间，以便观察
                curtNode.SetSearchType(SearchType.Expanded, true);
                #endregion

                curtNode.Closed = true;

                List<SearchNode> neighbors = GetNeighbors(curtNode);
                for (int i = 0; i < neighbors.Count; i++)
                {
                    SearchNode neighbor = neighbors[i];
                    if (IsNeighborValid(neighbor))
                    {
                        if (neighbor.Opened == false)
                            neighbor.SetParent(null, float.MaxValue);

                        UpdateVertex(curtNode, neighbor);
                    }
                }
            }
        }

        //绘制出最终的路径
        GeneratePath();

        yield break;
    }

    protected virtual void Preprocess()
    {

    }

    protected virtual bool IsNeighborValid(SearchNode neighbor)
    {
        return neighbor.Closed == false;
    }

    protected void GeneratePath()
    {
        SearchNode lastNode = GetNode(m_mapGoal.Pos);
        while (lastNode != null)
        {
            lastNode.SetSearchType(SearchType.Path, true);
            lastNode = lastNode.Parent;
        }
    }

    protected virtual void UpdateVertex(SearchNode curtNode, SearchNode nextNode)
    {
        float oldG = nextNode.G;
        ComputeCost(curtNode, nextNode);

        if(nextNode.G < oldG)
        {
            if (nextNode.Opened == false)
                AddToOpenList(nextNode);
        }
    }

    protected virtual void ComputeCost(SearchNode curtNode, SearchNode nextNode)
    {
        //Path 1
        float cost = curtNode.G + c(curtNode, nextNode);
        if (cost < nextNode.G)
            nextNode.SetParent(curtNode, cost);
    }

    protected virtual void SetVertex(SearchNode node)
    {

    }

    protected virtual void AddToOpenList(SearchNode node)
    {
        m_open.Enqueue(node.Pos, node.F(m_weight));
        node.Opened = true;
        node.SetSearchType(SearchType.Open, true);
    }

    /// <summary>
    /// 在open list中找成本最低的节点并去掉
    /// </summary>
    protected virtual Vector2Int PopOpenList()
    {
        return m_open.Dequeue();
    }

    protected virtual int OpenListSize()
    {
        return m_open.Count;
    }
}