using UnityEngine;

public interface INode
{
    int Id { get; }
    Vector2Int Pos { get; }
}

public struct PathNode : INode
{
    public int Id { get; }
    public Vector2Int Pos { get; }
    public int Level { get; }

    public PathNode(int id, Vector2Int pos, int level)
    {
        Id = id;
        Pos = pos;
        Level = level;
    }

    public PathNode(Vector2Int pos)
    {
        Pos = pos;
        Id = Level = 0;
    }
}