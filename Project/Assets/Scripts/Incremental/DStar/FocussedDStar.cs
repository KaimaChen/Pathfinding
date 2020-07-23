using Priority_Queue;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocussedDStar : BaseDStar
{
    private const float c_epsilon = 0.01f;

    private readonly SimplePriorityQueue<SearchNode, FDKey> m_openQueue = new SimplePriorityQueue<SearchNode, FDKey>();

    private SearchNode m_currR;
    private float m_currD;

    public FocussedDStar(SearchNode start, SearchNode goal, SearchNode[,] nodes, float showTime)
        : base(start, goal, nodes, showTime) { }

    public override IEnumerator Process()
    {
        yield return MoveRobot();
    }

    private float GVal(SearchNode X, SearchNode Y)
    {
        return SearchGrid.Instance.CalcHeuristic(X.Pos, Y.Pos, 1);
    }

    private bool Less(Vector2 a, Vector2 b)
    {
        return a.x < b.x || (Mathf.Approximately(a.x, b.x) && a.y < b.y);
    }

    private bool LessEq(Vector2 a, Vector2 b)
    {
        return a.x < b.x || (Mathf.Approximately(a.x, b.x) && a.y <= b.y);
    }

    private Vector2 Cost(SearchNode X)
    {
        float f = X.H + GVal(X, m_currR);
        return new Vector2(f, X.H);
    }

    private void Delete(SearchNode X)
    {
        m_openQueue.Remove(X);
        X.Closed = true;
    }

    private void PutState(SearchNode X)
    {
        X.Opened = true;
        FDKey key = new FDKey(X.Fb, X.Fx, X.Key);
        m_openQueue.Enqueue(X, key);
    }

    private SearchNode GetState()
    {
        if (m_openQueue.Count > 0)
            return m_openQueue.First;
        else
            return null;
    }

    private void Insert(SearchNode X, float newH)
    {
        if(X.IsNew)
        {
            X.Key = newH;
        }
        else
        {
            if(X.Opened)
            {
                X.Key = Mathf.Min(X.Key, newH);
                Delete(X);
            }
            else
            {
                X.Key = Mathf.Min(X.H, newH);
            }
        }

        X.H = newH;
        X.R = m_currR;
        X.Fx = X.Key + GVal(X, m_currR);
        X.Fb = X.Fx + m_currD;
        PutState(X);
    }

    private SearchNode MinState()
    {
        SearchNode X;
        while((X = GetState()) != null)
        {
            if(X.R != m_currR)
            {
                float newH = X.H;
                X.H = X.Key;
                Delete(X);
                Insert(X, newH);
            }
            else
            {
                return X;
            }
        }

        return null;
    }

    private Vector2? MinVal()
    {
        SearchNode X = MinState();
        if (X == null)
            return null;
        else
            return new Vector2(X.Fx, X.Key);
    }

    private Vector2? ProcessState()
    {
        SearchNode X = MinState();
        if (X == null)
            return null;

        Vector2 val = new Vector2(X.Fx, X.Key);
        float kVal = X.Key;
        Delete(X);

        if(kVal < X.H)
        {
            ForeachNeighbors(X, (Y) =>
            {
                if(!Y.IsNew && LessEq(Cost(Y), val) && (X.H > (Y.H + C(Y, X))))
                {
                    X.Parent = Y;
                    X.H = Y.H + C(Y, X);
                }
            });
        }

        if(Equal(kVal, X.H))
        {
            ForeachNeighbors(X, (Y) =>
            {
                if(Y.IsNew || (Y.Parent == X && !Equal(Y.H, X.H + C(X, Y))) || (Y.Parent != X && Bigger(Y.H, X.H + C(X, Y))))
                {
                    Y.Parent = X;
                    Insert(Y, X.H + C(X, Y));
                }
            });
        }
        else
        {
            ForeachNeighbors(X, (Y) =>
            {
                if(Y.IsNew || (Y.Parent == X && !Equal(Y.H, X.H + C(X, Y))))
                {
                    Y.Parent = X;
                    Insert(Y, X.H + C(X, Y));
                }
                else
                {
                    if (Y.Parent != X && Bigger(X.H, Y.H + C(Y, X)) && Y.Closed && Less(val, Cost(Y)))
                        Insert(Y, Y.H);
                }
            });
        }

        return MinVal();
    }

    private Vector2? ModifyCost(SearchNode X, byte cost)
    {
        X.SetCost(cost);

        if (X.Closed)
            Insert(X, X.H);

        return MinVal();
    }

    private IEnumerator MoveRobot()
    {
        //记录原始的地图信息
        for (int y = 0; y < m_mapHeight; y++)
            for (int x = 0; x < m_mapWidth; x++)
                m_foundMap[y, x] = m_nodes[y, x].Cost;

        //初始化
        m_currD = 0;
        m_currR = m_mapStart;
        Insert(m_mapGoal, 0);
        Vector2? val = Vector2.zero;

        //第一次寻路
        while (!m_mapStart.Closed && val != null)
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
            for (int i = 0; i < nearNodes.Count; i++)
            {
                Vector2Int pos = nearNodes[i].Pos;
                if (nearNodes[i].Cost != m_foundMap[pos.y, pos.x])
                {
                    m_foundMap[pos.y, pos.x] = nearNodes[i].Cost;
                    nearChanged.Add(nearNodes[i]);
                }
            }

            //周围有格子发生变化
            if(nearChanged.Count > 0)
            {
                if (m_currR != R)
                {
                    m_currD += GVal(R, m_currR) + c_epsilon;
                    m_currR = R;
                }

                for (int i = 0; i < nearChanged.Count; i++)
                {
                    SearchNode X = nearChanged[i];
                    val = ModifyCost(X, X.Cost);
                    ForeachNeighbors(X, (n) => { ModifyCost(n, n.Cost); }); //原论文中没有这句，如果移除了阻挡，并不能正常处理
                }

                //重新规划路径
                while (val != null && Less(val.Value, Cost(R)))
                    val = ProcessState();
            }

            //前进一步
            if (MoveForwardOneStep(ref R) == false)
                yield break;

            yield return new WaitForSeconds(m_showTime);
        }

        yield break;
    }
}

public struct FDKey : IComparable<FDKey>
{
    public float m_fb;
    public float m_f;
    public float m_k;

    public FDKey(float fb, float f, float k)
    {
        m_fb = fb;
        m_f = f;
        m_k = k;
    }

    public int CompareTo(FDKey other)
    {
        if(Mathf.Approximately(m_fb, other.m_fb))
        {
            if(Mathf.Approximately(m_f, other.m_f))
                return m_k.CompareTo(other.m_k);
            else
                return m_f.CompareTo(other.m_f);
        }
        else
        {
            return m_fb.CompareTo(other.m_fb);
        }
    }
}