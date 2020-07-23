using UnityEngine;

/// <summary>
/// 启发式函数
/// </summary>
public static class Heuristic
{
    /// <summary>
    /// 曼哈顿距离
    /// 适用场景：四方向网格移动
    /// </summary>
    public static float Manhattan(int dx, int dy)
    {
        return dx + dy;
    }

    public static float Manhattan(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return Manhattan(dx, dy);
    }

    /// <summary>
    /// 切比雪夫距离
    /// 使用场景：八方向网格移动（斜线代价为1）
    /// </summary>
    public static float Chebyshev(int dx, int dy)
    {
        return Mathf.Max(dx, dy);
    }

    public static float Chebyshev(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return Chebyshev(dx, dy);
    }

    /// <summary>
    /// Oct tile 八方向网格距离
    /// 使用场景：八方向移动（斜线代价为Sqrt(2)）
    /// </summary>
    public static float Octile(int dx, int dy)
    {
        //做法：先走45度斜线，然后走剩余的x或y方向
        float f = Define.c_sqrt2 - 1;
        return (dx < dy) ? f * dx + dy : f * dy + dx;
    }

    public static float Octile(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return Octile(dx, dy);
    }

    /// <summary>
    /// 欧几里德距离
    /// 使用场景：没有移动方向限制（路点或Navmesh）
    /// </summary>
    public static float Euclidean(int dx, int dy)
    {
        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    public static float Euclidean(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return Euclidean(dx, dy);
    }
}

public enum HeuristicType
{
    Manhattan,
    Chebyshev,
    Octile,
    Euclidean
}