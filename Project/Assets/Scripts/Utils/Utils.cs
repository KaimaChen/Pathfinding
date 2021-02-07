using UnityEngine;

public static class Utils
{
    #region Vector2
    /// <summary>
    /// 假设圆心在原点，求出特定角度对应的圆边上的点
    /// </summary>
    /// <param name="radius">圆半径</param>
    /// <param name="angle">所求角度</param>
    /// <returns>特定角度对应的圆边上的点</returns>
    public static Vector2 Polar(float radius, float angle)
    {
        return new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));
    }

    /// <summary>
    /// 假设圆心在start，求出特定角度对应的圆边上的点
    /// </summary>
    public static Vector2 DirectionStep(Vector2 start, float distance, float angle)
    {
        return start + Polar(distance, angle);
    }

    public static float Cross(Vector2 p, Vector2 q)
    {
        return p.x * q.y - p.y * q.x;
    }

    /// <summary>
    /// 向量 p->q 与x轴正向的夹角
    /// </summary>
    public static float Facing(Vector2 p, Vector2 q)
    {
        float dx = q.x - p.x;
        float dy = q.y - p.y;
        return Mathf.Atan2(dy, dx);
    }

    public static Vector2 Interpolate(Vector2 p, Vector2 q, float t)
    {
        float x = p.x + (q.x - p.x) * t;
        float y = p.y + (q.y - p.y) * t;
        return new Vector2(x, y);
    }
    #endregion

    #region 交点
    public static bool SegmentCircleIntersection(Vector2 a, Vector2 b, Vector2 center, float radius)
    {
        float u = Vector2.Dot(center - a, b - a) / Vector2.Dot(b - a, b - a);
        Vector2 e = a + Mathf.Clamp01(u) * (b - a);
        float d = Vector2.Distance(e, center);
        return Vector2.Distance(e, center) < radius;
    }
    #endregion

    public static float AngleDifference(float a, float b)
    {
        return Mathf.Abs(b - a) % (2 * Mathf.PI);
    }
}