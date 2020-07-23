using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// D*和FD*十分相近，因此弄一个基类把共同部分抽离出来
/// </summary>
public abstract class BaseDStar : BaseSearchAlgo
{
    protected const int c_sensorRadius = 2; //机器人传感器的探测范围
    protected readonly int m_largeValue; //用于阻挡的代价，普通算出来的移动代价一定要比该值小
    protected readonly int[,] m_foundMap; //目前通过传感器发现的地图

    public BaseDStar(SearchNode start, SearchNode goal, SearchNode[,] nodes, float showTime)
        : base(start, goal, nodes, showTime)
    {
        m_largeValue = m_mapWidth * m_mapHeight * 10;
        m_foundMap = new int[nodes.GetLength(0), nodes.GetLength(1)];
    }

    protected bool IsFoundObstacle(int x, int y)
    {
        //假设并不知道整张地图的情况，那么只能依赖当前发现的格子代价来作为判断依据
        return m_foundMap[y, x] == Define.c_costObstacle;
    }

    /// <summary>
    /// 从节点b到节点a的代价（对于有向图来说，顺序很重要）
    /// </summary>
    protected float C(SearchNode a, SearchNode b)
    {
        if (IsFoundObstacle(a.X, a.Y) || IsFoundObstacle(b.X, b.Y))
        {
            return m_largeValue;
        }
        else
        {
            //走斜线时，如果两边都是阻挡，那么该斜线的代价也是阻挡那么大
            int dx = a.X - b.X;
            int dy = a.Y - b.Y;
            if (Mathf.Abs(dx) != 0 && Mathf.Abs(dy) != 0)
            {
                if (IsFoundObstacle(b.X + dx, b.Y) && IsFoundObstacle(b.X, b.Y + dy))
                    return m_largeValue;
            }

            return c(a, b);
        }
    }

    /// <summary>
    /// 传感器能检测到的格子
    /// </summary>
    /// <param name="radius">检测的范围</param>
    /// <returns>能检测到的格子</returns>
    protected List<SearchNode> SensorDetectNodes(SearchNode R, int radius)
    {
        List<SearchNode> result = new List<SearchNode>();

        for (int dx = -radius; dx <= radius; dx++)
            for (int dy = -radius; dy <= radius; dy++)
                TryAddNode(R.Pos, dx, dy, result);

        return result;
    }

    protected override bool TryAddNode(Vector2Int curtPos, int dx, int dy, List<SearchNode> result)
    {
        int x = curtPos.x + dx;
        int y = curtPos.y + dy;
        SearchNode node = GetNode(x, y);
        if (node != null) //原始论文中障碍物只是代价非常高，但还是可以作为邻居
        {
            result.Add(node);
            return true;
        }
        else
        {
            return false;
        }
    }

    protected bool MoveForwardOneStep(ref SearchNode R)
    {
        if (R.Parent == null)
        {
            Debug.LogError("前进失败，没有路径");
            return false;
        }

        //路上遇到阻挡则认为没有可走路径了
        if (C(R, R.Parent) >= m_largeValue)
        {
            Debug.LogError("前进失败，遇见障碍");
            return false;
        }

        R.SetSearchType(SearchType.Path, true);
        R = R.Parent;
        R.SetSearchType(SearchType.CurtPos, true);

        return true;
    }
}