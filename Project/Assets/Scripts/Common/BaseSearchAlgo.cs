using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseSearchAlgo
{
    private const float c_minDelta = 0.0001f; //误差小于该值则认为两个Float相等

    protected readonly int m_infinite; //用于表示论文中的无限，不用int.MaxValue是怕运算溢出

    protected SearchNode m_mapStart;
    protected SearchNode m_mapGoal;
    protected readonly SearchNode[,] m_nodes;
    protected readonly int m_mapWidth;
    protected readonly int m_mapHeight;

    protected readonly float m_showTime;

    public BaseSearchAlgo(SearchNode start, SearchNode end, SearchNode[,] nodes, float showTime)
    {
        m_mapStart = start;
        m_mapGoal = end;
        m_nodes = nodes;
        m_showTime = showTime;

        m_mapHeight = nodes.GetLength(0);
        m_mapWidth = nodes.GetLength(1);
        m_infinite = m_mapWidth * m_mapHeight * 10;
    }

    public abstract IEnumerator Process();

    protected virtual List<SearchNode> GetNeighbors(SearchNode node)
    {
        List<SearchNode> result = new List<SearchNode>();
        Vector2Int pos = node.Pos;

        bool left = TryAddNode(pos, -1, 0, result);
        bool right = TryAddNode(pos, 1, 0, result);
        bool top = TryAddNode(pos, 0, 1, result);
        bool bottom = TryAddNode(pos, 0, -1, result);

        if (left || top) TryAddNode(pos, -1, 1, result);
        if (left || bottom) TryAddNode(pos, -1, -1, result);
        if (right || bottom) TryAddNode(pos, 1, -1, result);
        if (right || top) TryAddNode(pos, 1, 1, result);

        return result;
    }

    protected void ForeachNeighbors(SearchNode node, Action<SearchNode> func)
    {
        List<SearchNode> neighbors = GetNeighbors(node);
        for (int i = 0; i < neighbors.Count; i++)
            func(neighbors[i]);
    }

    protected virtual bool TryAddNode(Vector2Int curtPos, int dx, int dy, List<SearchNode> result)
    {
        int x = curtPos.x + dx;
        int y = curtPos.y + dy;
        SearchNode node = GetNode(x, y);
        if(node != null && node.IsObstacle() == false)
        {
            result.Add(node);
            return true;
        }
        else
        {
            return false;
        }
    }

    protected bool IsWalkableAt(int x, int y)
    {
        SearchNode node = GetNode(x, y);
        return node != null && !node.IsObstacle();
    }

    protected bool IsInside(int x, int y)
    {
        return (x >= 0 && x < m_nodes.GetLength(1) && y >= 0 && y < m_nodes.GetLength(0));
    }

    protected SearchNode GetNode(int x, int y)
    {
        if (IsInside(x, y))
            return m_nodes[y, x];
        else
            return null;
    }

    protected SearchNode GetNode(Vector2Int pos)
    {
        return GetNode(pos.x, pos.y);
    }

    protected float g(SearchNode s)
    {
        return s.G;
    }

    protected virtual float h(SearchNode s)
    {
        if (s.H < 0)
            s.H = CalcHeuristic(s, m_mapGoal);

        return s.H;
    }

    protected virtual float c(SearchNode a, SearchNode b)
    {
        Vector2Int ap = a.Pos;
        Vector2Int bp = b.Pos;
        return SearchGrid.Instance.CalcHeuristic(ap, bp, b.Cost); //TODO 对于FlowField，需要把直线上的所有格子代价都加进来
    }

    protected float CalcHeuristic(SearchNode a, SearchNode b, float weight = 1)
    {
        return SearchGrid.Instance.CalcHeuristic(a.Pos, b.Pos, weight);
    }

    public virtual void NotifyChangeNode(List<SearchNode> nodes, bool increaseCost) { }

    public virtual void NotifyChangeStart(SearchNode startNode) { }

    public virtual void NotifyChangeGoal(SearchNode goalNode) { }

    protected void ForeachNode(Action<SearchNode> callback)
    {
        for(int y = 0; y < m_mapHeight; y++)
            for(int x = 0; x < m_mapWidth; x++)
                callback(m_nodes[y, x]);
    }

    //TODO 想想更好的处理方式，不用float
    #region Handle float
    protected bool Equal(float a, float b)
    {
        return Mathf.Abs(a - b) <= c_minDelta;
    }

    protected bool NotEqual(float a, float b)
    {
        return !Equal(a, b);
    }

    protected bool Bigger(float a, float b)
    {
        return (a - b) > c_minDelta;
    }

    protected bool Less(float a, float b)
    {
        return Bigger(b, a);
    }

    protected bool LessEqual(float a, float b)
    {
        return Equal(a, b) || Less(a, b);
    }
    #endregion
}