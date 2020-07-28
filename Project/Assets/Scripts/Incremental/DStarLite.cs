using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 完全基于LPA*发展出来的算法
/// 1. 方向改为从目标到起点
/// 2. 调整了开放列表的操作方式，避免频繁的重新排序操作
/// 
/// 注意事项：如果传感器的检测范围很小，那么即使很远的地方有个障碍清除了，这个算法还是可能判断为没有路径
/// （可以用Demo弄成一开始有路径，在它走的时候关上路径，当它回头找其他路径时再把路径打开来看效果）
/// 
/// Demo的玩耍方式：把Show Time参数弄大点，这样可以看到格子的移动，然后在它前面加上阻挡看看效果
/// </summary>
public class DStarLite : LPAStar
{
    protected float m_km = 0;

    protected SearchNode m_currStart;
    protected readonly int[,] m_foundMap; //目前通过传感器发现的地图

    public DStarLite(SearchNode start, SearchNode goal, SearchNode[,] nodes, float showTime)
        : base(start, goal, nodes, showTime)
    {
        m_foundMap = new int[nodes.GetLength(0), nodes.GetLength(1)];
    }

    public override IEnumerator Process()
    {
        Initialize();
        ComputeShortestPath();

        while (m_currStart != m_mapGoal)
        {
            SearchNode oldEnd = EndNode();

            if (MoveOneStep() == false) //假设每次走一步
                yield break; //如果没找到路径，则直接结束（实际使用时，因为传感器的范围有限，可能别的地方此时移除了障碍，因此可以让算法别这么快结束，让它再去别的地方找）

            CheckNearChanged(oldEnd); //看看附近有没有格子发生变化

            yield return new WaitForSeconds(m_showTime);
        }
        
        yield break;
    }

    //与LPA*的搜索方向相反
    protected override SearchNode BeginNode()
    {
        return m_mapGoal;
    }

    //与LPA*的搜索方向相反
    protected override SearchNode EndNode()
    {
        return m_currStart;
    }

    protected override void Initialize()
    {
        //假设一开始通过卫星等方式获取了原始地图
        //这里我通过记录原本的格子代价来判断之后有没有发生过变化
        //如果是通过其他方式检测格子变化，则可以去掉这部分代码从而节省内存
        for (int y = 0; y < m_mapHeight; y++)
            for (int x = 0; x < m_mapWidth; x++)
                m_foundMap[y, x] = m_nodes[y, x].Cost;

        m_currStart = m_mapStart;
        m_km = 0;

        base.Initialize();
    }

    protected override LPAKey CalculateKey(SearchNode node)
    {
        float key2 = Mathf.Min(node.G, node.Rhs);
        float key1 = key2 + node.ValidH() + m_km; //这里相对LPA*多加了m_km值
        return new LPAKey(key1, key2);
    }

    protected override void ComputeShortestPath()
    {
        while (m_openQueue.Count > 0 && (TopKey() < CalculateKey(EndNode())) || EndNode().Rhs > EndNode().G)
        {
            LPAKey kOld = TopKey();
            SearchNode curtNode = PopOpenQueue();
            LPAKey kCurt = CalculateKey(curtNode);

            if(kOld < kCurt) //新的Key值更大，表示该Key值可能已经并不真的应该在顶层
            {
                curtNode.LPAKey = kCurt;
                m_openQueue.Enqueue(curtNode.Pos, kCurt);
            }
            else if (curtNode.G > curtNode.Rhs)
            {
                UpdateOverConsistent(curtNode);
            }
            else
            {
                UpdateUnderConsistent(curtNode);
            }
        }

        GeneratePath();
    }

    /// <summary>
    /// 往前走一步
    /// </summary>
    private bool MoveOneStep()
    {
        if(EndNode().Rhs == c_large)
        {
            Debug.LogError("找不到路径");
            return false;
        }

        float min = float.MaxValue;
        SearchNode minNode = null;
        List<SearchNode> neighbors = GetNeighbors(EndNode());
        for(int i = 0; i < neighbors.Count; i++)
        {
            float g = neighbors[i].G + c(EndNode(), neighbors[i]);
            if(g < min)
            {
                min = g;
                minNode = neighbors[i];
            }
        }

        if(minNode != null)
        {
            #region show
            m_currStart.SetSearchType(SearchType.Path, true);
            minNode.SetSearchType(SearchType.CurtPos, true);
            #endregion

            m_currStart = minNode;
        }
        else
        {
            Debug.LogError($"生成路径失败，在{m_currStart.Pos}处中断");
            return false;
        }

        return true;
    }

    protected void CheckNearChanged(SearchNode oldEnd)
    {
        List<SearchNode> nearChanged = new List<SearchNode>();

        //假设检测器只能检查附近的点
        List<SearchNode> neighbors = GetNeighbors(m_currStart);
        for(int i = 0; i < neighbors.Count; i++)
        {
            Vector2Int pos = neighbors[i].Pos;
            if (neighbors[i].Cost != m_foundMap[pos.y, pos.x])
            {
                m_foundMap[pos.y, pos.x] = neighbors[i].Cost;
                nearChanged.Add(neighbors[i]);
            }
        }

        if(nearChanged.Count > 0)
        {
            m_km += c(oldEnd, EndNode());
            oldEnd = EndNode();

            HandleChangedNode(nearChanged);
        }
    }

    protected override bool TryAddNode(Vector2Int curtPos, int dx, int dy, List<SearchNode> result)
    {
        int x = curtPos.x + dx;
        int y = curtPos.y + dy;
        SearchNode node = GetNode(x, y);
        if (node != null && m_foundMap[y, x] != Define.c_costObstacle) //假设并不知道整张地图的情况，那么只能依赖当前发现的格子代价来作为判断依据
        {
            result.Add(node);
            return true;
        }
        else
        {
            return false;
        }
    }

    protected override void GeneratePath()
    {
        //假设每次只能走一步，因此不能像以前那样直接生成整条路径
    }

    public override void NotifyChangeNode(List<SearchNode> nodes, bool increaseCost)
    {
        //不使用通知方式，而是通过检查附近的点是否发生变化来模拟检测器的工作方式
    }
}