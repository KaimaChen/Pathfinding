using System.Collections.Generic;
using UnityEngine;

public class ConcreteMap : IMap
{
    public int Width { get; }
    public int Height { get; }
    private readonly ConcreteNode[,] m_nodes;

    public ConcreteMap(int width, int height)
    {
        Width = width;
        Height = height;
        m_nodes = new ConcreteNode[height, width];
    }

    public void AddNode(int x, int y, ConcreteNode node)
    {
        m_nodes[y, x] = node;
    }

    public ConcreteNode Get(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return null;

        return m_nodes[y, x];
    }

    public ConcreteNode Get(Vector2Int pos)
    {
        return Get(pos.x, pos.y);
    }

    public int NodeCount()
    {
        return Width * Height;
    }

    public List<INode> GetSuccessors(INode source)
    {
        List<INode> result = new List<INode>();
        Vector2Int pos = source.Pos;

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

    private bool TryAddNode(Vector2Int curtPos, int dx, int dy, List<INode> result)
    {
        int x = curtPos.x + dx;
        int y = curtPos.y + dy;
        ConcreteNode node = Get(x, y);
        if(node != null && !node.IsObstacle)
        {
            result.Add(node);
            return true;
        }
        else
        {
            return false;
        }
    }

    public float CalcHeuristic(INode a, INode b)
    {
        return Heuristic.Octile(a.Pos, b.Pos);
    }
}