using System.Collections;
using System.Collections.Generic;
using Priority_Queue;
using UnityEngine;

//TODO Optimization of Tie Breaking

/// <summary>
/// Path Adaptive A*
/// </summary>
public class Path_AAStar : BaseSearchAlgo
{
    private int m_iteration;
    private SearchNode m_start;
    private SearchNode m_goal;
    private readonly Dictionary<int, float> m_pathCost = new Dictionary<int, float>();
    private readonly SimplePriorityQueue<SearchNode> m_open = new SimplePriorityQueue<SearchNode>();
    private readonly List<SearchNode> m_increaseCostNodes = new List<SearchNode>();

    public Path_AAStar(SearchNode start, SearchNode goal, SearchNode[,] nodes, float showTime)
        : base(start, goal, nodes, showTime) { }

    public override IEnumerator Process()
    {
        m_start = m_mapStart;
        m_goal = m_mapGoal;

        m_iteration = 1;
        ForeachNode((s) =>
        {
            s.Iteration = 0;
            s.NextState = null;
            s.BackState = null;
        });

        while(m_start != m_goal)
        {
            InitializeState(m_start);
            InitializeState(m_goal);

            m_start.G = 0;
            ClearOpen();
            AddOrUpdateOpen(m_start, m_start.G + m_start.H);

            if(ComputePath() == false)
            {
                Debug.LogError("寻找路径失败");
                yield break;
            }

            //不使用Parent，而是使用NextState来寻找路径
            while(m_start.NextState != null)
            {
                m_start.SetSearchType(SearchType.Path, true);
                m_start = m_start.NextState;
                m_start.SetSearchType(SearchType.CurtPos, true);

                //检查环境变化
                for(int i = 0; i < m_increaseCostNodes.Count; i++)
                {
                    SearchNode s = m_increaseCostNodes[i];
                    ForeachNeighbors(s, (n) =>
                    {
                        //因为是无向图，所以两个方向都要检查
                        if (s.BackState == n)
                            CleanPath(s);
                        if (n.BackState == s)
                            CleanPath(n);
                    });
                }
                m_increaseCostNodes.Clear();

                yield return new WaitForSeconds(m_showTime);
            }

            m_iteration++;
        }

        yield break;
    }

    private void InitializeState(SearchNode s)
    {
        if(s.Iteration == 0)
        {
            s.H = H(s, m_goal);
            s.G = m_infinite;
        }
        else if(s.Iteration != m_iteration)
        {
            if (s.G + s.H < m_pathCost[s.Iteration])
                s.H = m_pathCost[s.Iteration] - s.G;
            s.G = m_infinite;
        }

        s.Iteration = m_iteration;
    }

    /// <summary>
    /// 设置好前指针和后指针，形成双向的一条链
    /// </summary>
    private void MakePath(SearchNode s)
    {
        while(s != m_start)
        {
            SearchNode auxS = s;
            s = s.Parent;
            s.NextState = auxS;
            auxS.BackState = s;
        }
    }

    /// <summary>
    /// 清理起点到s的路径
    /// </summary>
    private void CleanPath(SearchNode s)
    {
        while(s.BackState != null)
        {
            SearchNode auxS = s.BackState;
            s.BackState = null;
            auxS.NextState = null;
            s = auxS;
        }
    }

    private bool ComputePath()
    {
        while(m_open.Count > 0)
        {
            SearchNode s = PopMinFromOpen();

            //到达目标或到达之前的最短路径就结束算法
            if(s == m_goal || s.NextState != null)
            {
                m_pathCost[m_iteration] = s.G + s.H;
                CleanPath(s); //只保留从s到goal的最短路径
                MakePath(s); //构建s之前的最短路径，因为前后指针的存在，这时s已经连接了前面新计算的路径和后面旧的最短路径
                return true;
            }

            //A*算法
            ForeachNeighbors(s, (n) =>
            {
                InitializeState(n);
                if(n.G > s.G + c(s, n))
                {
                    n.G = s.G + c(s, n);
                    n.Parent = s;
                    AddOrUpdateOpen(n, n.G + n.H);
                }
            });
        }

        return false;
    }

    private float H(SearchNode a, SearchNode b)
    {
        return SearchGrid.Instance.CalcHeuristic(a.Pos, b.Pos, 1);
    }

    #region 事件监听
    public override void NotifyChangeNode(List<SearchNode> nodes, bool increaseCost)
    {
        //Path AA*只能处理代价变大的情况
        if(increaseCost)
            m_increaseCostNodes.AddRange(nodes);
    }
    #endregion

    #region 开放列表
    private SearchNode PopMinFromOpen()
    {
        SearchNode s = m_open.Dequeue();
        s.Closed = true;
        s.SetSearchType(SearchType.Expanded, true, true);
        return s;
    }

    private void AddOrUpdateOpen(SearchNode s, float key)
    {
        if (s.Opened)
        {
            m_open.UpdatePriority(s, key);
        }
        else
        {
            m_open.Enqueue(s, key);
            s.Opened = true;
            s.SetSearchType(SearchType.Open, true, true);
        }
    }

    private void ClearOpen()
    {
        foreach(var s in m_open)
        {
            s.Opened = s.Closed = false;
            s.SetSearchType(SearchType.None, true);
        }

        m_open.Clear();
    }
    #endregion
}