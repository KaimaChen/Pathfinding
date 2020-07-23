using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

/// <summary>
/// D* 算法
/// 
/// 一些调整：
/// * 论文里使用的h容易误解为启发函数，但实际表示到目标点的最短距离
/// * 我这里用parent表示论文里的backpointer
/// 
/// 注意事项：
/// 论文里阻挡并不是完全不可走的，只移动代价非常大
/// 
/// Demo的玩耍方式：把Show Time参数弄大点，这样可以看到格子的移动，然后在它前面加上阻挡看看效果
/// </summary>
public class DStar : BaseDStar
{
    private const float c_noVal = -1;

    private readonly SimplePriorityQueue<SearchNode> m_openQueue = new SimplePriorityQueue<SearchNode>();

    public DStar(SearchNode start, SearchNode goal, SearchNode[,] nodes, float showTime)
        : base(start, goal, nodes, showTime) { }

    public override IEnumerator Process()
    {
        yield return MoveRobot();
    }

    private float ProcessState()
    {
        SearchNode X = MinState();
        if (X == null)
            return c_noVal;

        float kOld = GetKMin();
        Delete(X);

        //发现RAISE，则检查能否通过邻居获得更短的路径
        if(Less(kOld, X.H))
        {
            ForeachNeighbors(X, (Y) =>
            {
                if (LessEqual(Y.H, kOld) && Bigger(X.H, (Y.H + C(Y, X))))
                {
                    X.Parent = Y;
                    X.H = Y.H + C(Y, X);
                }
            });
        }

        if(Equal(kOld, X.H)) //LOWER state
        {
            ForeachNeighbors(X, (Y) =>
            {
                if (Y.IsNew ||
                    (Y.Parent == X && NotEqual(Y.H, (X.H + C(X, Y)))) ||
                    (Y.Parent != X && Bigger(Y.H, (X.H + C(X, Y)))))
                {
                    Y.Parent = X;
                    Insert(Y, X.H + C(X, Y));
                }
            });
        }
        else //RAISE state
        {
            ForeachNeighbors(X, (Y) =>
            {
                if (Y.IsNew || (Y.Parent == X && NotEqual(Y.H, (X.H + C(X, Y)))))
                {
                    Y.Parent = X;
                    Insert(Y, X.H + C(X, Y));
                }
                else
                {
                    if (Y.Parent != X && Bigger(Y.H, (X.H + C(X, Y))))
                    {
                        Insert(X, X.H);
                    }
                    else
                    {
                        if (Y.Parent != X && Bigger(X.H, (Y.H + C(Y, X))) && Y.Closed && Bigger(Y.H, kOld))
                            Insert(Y, Y.H);
                    }
                }
            });
        }

        return GetKMin();
    }

    private SearchNode MinState()
    {
        if (m_openQueue.Count > 0)
            return m_openQueue.First;
        else
            return null;
    }

    private float GetKMin()
    {
        if (m_openQueue.Count > 0)
            return m_openQueue.FirstPriority;
        else
            return c_noVal;
    }

    private void Delete(SearchNode X)
    {
        m_openQueue.Remove(X);
        X.Closed = true;
    }

    private void Insert(SearchNode X, float newH)
    {
        if (X.IsNew)
            X.Key = newH;
        else if (X.Opened)
            X.Key = Mathf.Min(X.Key, newH);
        else if (X.Closed)
            X.Key = Mathf.Min(X.H, newH);

        X.Opened = true;
        X.H = newH;

        if (m_openQueue.Contains(X))
            m_openQueue.UpdatePriority(X, X.Key);
        else
            m_openQueue.Enqueue(X, X.Key);
    }

    private float ModifyCost(SearchNode node, byte cost)
    {
        node.SetCost(cost);

        if (node.Closed)
            Insert(node, node.H);

        return GetKMin();
    }

    private IEnumerator MoveRobot()
    {
        //假设一开始通过卫星等方式获取了原始地图
        //这里我通过记录原本的格子代价来判断之后有没有发生过变化
        //如果是通过其他方式检测格子变化，则可以去掉这部分代码从而节省内存
        for (int y = 0; y < m_mapHeight; y++)
            for (int x = 0; x < m_mapWidth; x++)
                m_foundMap[y, x] = m_nodes[y, x].Cost;

        //初始化
        Insert(m_mapGoal, 0);
        float val = 0;

        //第一次寻路
        while (!m_mapStart.Closed && val >= 0)
            val = ProcessState();

        if (m_mapStart.IsNew)
        {
            Debug.LogError("找不到路径");
            yield break;
        }

        //移动并检查环境变化
        SearchNode R = m_mapStart;
        while(R != m_mapGoal)
        {
            List<SearchNode> nearChanged = new List<SearchNode>();

            //假设检测器只能检查附近的点
            List<SearchNode> nearNodes = SensorDetectNodes(R, c_sensorRadius);
            for(int i = 0; i < nearNodes.Count; i++)
            {
                Vector2Int pos = nearNodes[i].Pos;
                if(nearNodes[i].Cost != m_foundMap[pos.y, pos.x])
                {
                    m_foundMap[pos.y, pos.x] = nearNodes[i].Cost;
                    nearChanged.Add(nearNodes[i]);
                }
            }

            //周围有格子发生变化
            if(nearChanged.Count > 0)
            {
                for (int i = 0; i < nearChanged.Count; i++)
                {
                    SearchNode node = nearChanged[i];
                    ModifyCost(node, node.Cost);
                    ForeachNeighbors(node, (n) => { ModifyCost(n, n.Cost); });
                }

                //重新规划路径
                while (val >= 0)
                    val = ProcessState();
            }

            //前进一步
            if(MoveForwardOneStep(ref R) == false)
                yield break;

            yield return new WaitForSeconds(m_showTime);
        }

        yield break;
    }
}