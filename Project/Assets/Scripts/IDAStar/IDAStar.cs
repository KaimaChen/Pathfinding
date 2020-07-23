using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO 可视化搜索过程
public class IDAStar : BaseSearchAlgo
{
    private const int c_found = -1;
    private const int c_maxMilliSeconds = 2000; //最多运行多少毫秒
    private readonly float m_weight = 1;
    
    public IDAStar(SearchNode start, SearchNode end, SearchNode[,] nodes, float weight, float showTime)
        : base(start, end, nodes, showTime)
    {
        m_weight = weight;
    }

    public override IEnumerator Process()
    {
        DateTime startTime = DateTime.Now;

        float threshold = Heuristic(m_mapStart);
        Stack<SearchNode> path = new Stack<SearchNode>();
        path.Push(m_mapStart);

        float result;
        while(true)
        {
            result = Search(path, 0, threshold);

            if (result == c_found)
                break;

            if (result == float.MaxValue)
                break;

            double runTime = DateTime.Now.Subtract(startTime).TotalMilliseconds;
            if (runTime > c_maxMilliSeconds)
            {
                Debug.LogError("Time Exceeded");
                break;
            }

            threshold = result;
        }

        #region show
        if(result == c_found)
        {
            while(path.Count > 0)
            {
                SearchNode node = path.Pop();
                node.SetSearchType(SearchType.Path, true);
            }
        }
        #endregion

        yield break;
    }

    private float Search(Stack<SearchNode> path, float g, float threshold)
    {
        SearchNode node = path.Peek();
        float f = g + Heuristic(node);
        if (f > threshold)
            return f;

        if (node == m_mapGoal)
            return c_found;

        float min = float.MaxValue;
        List<SearchNode> neighbors = GetNeighbors(node);
        for(int i = 0; i < neighbors.Count; i++)
        {
            SearchNode neighbor = neighbors[i];
            if (path.Contains(neighbor))
                continue;

            path.Push(neighbor);

            float value = Search(path, g + c(node, neighbor), threshold);
            if (value == c_found)
                return c_found;

            if (value < min)
                min = value;

            path.Pop();
        }

        return min;
    }

    private float Heuristic(SearchNode node)
    {
        return SearchGrid.Instance.CalcHeuristic(node.Pos, SearchGrid.Instance.EndNode.Pos, m_weight);
    }
}