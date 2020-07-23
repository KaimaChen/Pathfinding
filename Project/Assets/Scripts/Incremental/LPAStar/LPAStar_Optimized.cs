using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 根据原始论文中优化章节进行的优化
/// </summary>
public class LPAStar_Optimized : LPAStar
{
    private int m_mazeIteration;

    public LPAStar_Optimized(SearchNode start, SearchNode goal, SearchNode[,] nodes, float showTime)
        : base(start, goal, nodes, showTime) { }

    protected override void Initialize()
    {
        m_mazeIteration++;

        m_openQueue.Clear();

        m_mapStart.G = c_large;
        m_mapStart.SetRhs(0, null);
        m_mapStart.LPAKey = CalculateKey(m_mapStart);
        m_mapStart.Iteration = m_mazeIteration;
        AddOrUpdateOpenQueue(m_mapStart);

        m_mapGoal.G = c_large;
        m_mapGoal.SetRhs(c_large, null);
        m_mapGoal.Iteration = m_mazeIteration;

        //优化点：避免遍历所有格子
        //* 实际应用时，因为格子可能很多，导致遍历格子十分耗时，而且其中很多格子根本不会访问到
        //* 这时可以直到访问该格子时才进行初始化（可以每个格子保存一个Iteration来确定格子是否在同一次搜索中）
    }

    private void InitNode(SearchNode node)
    {
        if(node.Iteration != m_mazeIteration)
        {
            node.G = c_large;
            node.SetRhs(c_large, null);
            node.Iteration = m_mazeIteration;
        }
    }

    protected override void UpdateVertex(SearchNode curtNode)
    {
        //注意：这里去掉了UpdateRhs，而把rhs的计算放到了ComputeShortestPath里

        if (Mathf.Approximately(curtNode.G, curtNode.Rhs))
        {
            RemoveFromOpenQueue(curtNode);
        }
        else
        {
            curtNode.LPAKey = CalculateKey(curtNode);
            AddOrUpdateOpenQueue(curtNode);
        }
    }

    protected override void UpdateOverConsistent(SearchNode curtNode)
    {
        curtNode.G = curtNode.Rhs;
        RemoveFromOpenQueue(curtNode);

        List<SearchNode> neighbors = GetNeighbors(curtNode);
        for (int i = 0; i < neighbors.Count; i++)
        {
            InitNode(neighbors[i]);
            float value = curtNode.G + c(curtNode, neighbors[i]);
            //优化点：简化rhs的计算
            //* 因为curtNode的g值变小，因此邻居中rhs值变化也肯定和curtNode有关，而不用遍历自己的邻居
            if (neighbors[i] != m_mapStart && neighbors[i].Rhs > value)
            {
                neighbors[i].SetRhs(value, curtNode);
                UpdateVertex(neighbors[i]);
            }
        }
    }

    protected override void UpdateUnderConsistent(SearchNode curtNode)
    {
        curtNode.G = c_large;
        UpdateVertex(curtNode);

        List<SearchNode> neighbors = GetNeighbors(curtNode);
        for (int i = 0; i < neighbors.Count; i++)
        {
            InitNode(neighbors[i]);
            //优化点：简化rhs的计算
            //* 因为curtNode的g值变大，那么受影响的只有邻居中rhs依赖curtNode旧的g值的节点
            if (neighbors[i] != m_mapStart && neighbors[i].RhsSource == curtNode)
            {
                UpdateRhs(neighbors[i]);
                UpdateVertex(neighbors[i]);
            }
        }
    }

    protected override void ComputeShortestPath()
    {
        //优化点：提早结束
        //* 假设终点位于开放队列顶部，且属于过一致，那么当前要扩展的节点就是终点
        //* 扩展终点会把其g值设置为rhs值，让其变为局部一致，然后整个搜索结束
        //* 但最终生成路径时并不会使用到终点的g值，因此这一步可以省掉
        //* 该优化能避免扩展其它Key值与终点相同的点
        while(m_openQueue.Count > 0 && (TopKey() < CalculateKey(m_mapGoal) || m_mapGoal.Rhs > m_mapGoal.G))
        {
            //优化点：减少开放队列的操作
            //* 这里由Pop改为Top，可以在欠一致的情况下减少一次移除操作的开销
            SearchNode curtNode = Top();

            if (curtNode.G > curtNode.Rhs)
                UpdateOverConsistent(curtNode);
            else
                UpdateUnderConsistent(curtNode);
        }

        GeneratePath();
    }
}