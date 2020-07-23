using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 宽度优先搜索（泛洪）
/// </summary>
public class BreadthFirstSearch : AStar
{
    private readonly Queue<Vector2Int> m_openQueue = new Queue<Vector2Int>();

    public BreadthFirstSearch(SearchNode start, SearchNode goal, SearchNode[,] nodes, float weight, float showTime)
        : base(start, goal, nodes, weight, showTime)
    { }

    protected override void AddToOpenList(SearchNode node)
    {
        m_openQueue.Enqueue(node.Pos);
        node.Opened = true;
        node.SetSearchType(SearchType.Open, true);
    }

    protected override Vector2Int PopOpenList()
    {
        return m_openQueue.Dequeue();
    }

    protected override int OpenListSize()
    {
        return m_openQueue.Count;
    }
}
