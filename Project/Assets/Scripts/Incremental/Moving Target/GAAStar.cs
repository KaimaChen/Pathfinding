using Priority_Queue;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generalized Adaptive A*
/// 
/// 例子使用方式：移动起点或终点，增加移除障碍后等一会看结果
/// </summary>
public class GAAStar : BaseSearchAlgo
{
    private int m_counter;
    private SearchNode m_currStart;
    private SearchNode m_currGoal;
    private readonly Dictionary<int, float> m_deltaH = new Dictionary<int, float>();
    private readonly Dictionary<int, float> m_pathCost = new Dictionary<int, float>();
    private readonly SimplePriorityQueue<SearchNode, float> m_open = new SimplePriorityQueue<SearchNode, float>();

    private readonly HashSet<SearchNode> m_decreaseNodes = new HashSet<SearchNode>();

    public GAAStar(SearchNode start, SearchNode goal, SearchNode[,] nodes, float showTime)
        : base(start, goal, nodes, showTime) { }

    public override IEnumerator Process()
    {
        m_decreaseNodes.Clear();

        m_counter = 1;
        m_pathCost.Clear();
        m_deltaH.Clear();
        m_deltaH[1] = 0;

        ForeachNode((s) =>
        {
            s.Iteration = 0;
        });

        m_currStart = m_mapStart;
        m_currGoal = m_mapGoal;
        while(m_currStart != m_currGoal)
        {
            InitializeState(m_currStart);
            InitializeState(m_currGoal);
            
            m_currStart.G = 0;
            m_currStart.Parent = null;
            ClearOpen(true);

            AddToOpen(m_currStart, g(m_currStart) + h(m_currStart));

            ComputePath();
            if (m_open.Count <= 0)
                m_pathCost[m_counter] = float.MaxValue;
            else
                m_pathCost[m_counter] = m_currGoal.G;

            yield return ShowPath();

            //修改起点
            if (m_currStart != m_mapStart)
                m_currStart = m_mapStart;

            //修改终点
            if(m_currGoal != m_mapGoal)
            {
                InitializeState(m_mapGoal);
                if (g(m_mapGoal) + h(m_mapGoal) < m_pathCost[m_counter])
                    m_mapGoal.H = m_pathCost[m_counter] - g(m_mapGoal);
                m_deltaH[m_counter + 1] = m_deltaH[m_counter] + h(m_mapGoal);
                m_currGoal = m_mapGoal;
            }
            else
            {
                m_deltaH[m_counter + 1] = m_deltaH[m_counter];
            }

            m_counter++;
            ConsistencyProcedure();
        }

        yield break;
    }

    private void InitializeState(SearchNode s)
    {
        if(s.Iteration != m_counter && s.Iteration != 0)
        {
            if (g(s) + h(s) < m_pathCost[s.Iteration])
                s.H = m_pathCost[s.Iteration] - g(s);
            s.H = h(s) - (m_deltaH[m_counter] - m_deltaH[s.Iteration]);
            s.H = Mathf.Max(h(s), CalcHeuristic(s, m_currGoal));
            s.G = float.MaxValue;
        }
        else if(s.Iteration == 0)
        {
            s.G = float.MaxValue;
            s.H = CalcHeuristic(s, m_currGoal);
        }

        s.Iteration = m_counter;
    }

    /// <summary>
    /// 使用经典的A*来计算路径
    /// </summary>
    private void ComputePath()
    {
        while(g(m_currGoal) > MinKey())
        {
            SearchNode s = PopFromOpen();
            ForeachNeighbors(s, (a) =>
            {
                InitializeState(a);
                if(g(a) > g(s) + c(s, a))
                {
                    a.G = g(s) + c(s, a);
                    a.Parent = s;

                    if (a.Opened) RemoveFromOpen(a);
                    AddToOpen(a, g(a) + h(a));
                }
            });
        }
    }

    private void ConsistencyProcedure()
    {
        ClearOpen(false);

        foreach(SearchNode s in m_decreaseNodes)
        {
            s.H = float.MaxValue; //这里代价减少的节点都是从阻挡变成可行走，根据论文要把h值设置为无限大

            ForeachNeighbors(s, (a) =>
            {
                InitializeState(s);
                InitializeState(a);
                if(h(s) > c(s, a) + h(a))
                {
                    s.H = c(s, a) + h(a);
                    if (s.Opened) RemoveFromOpen(s);
                    AddToOpen(s, h(s));
                }
            });
        }
        m_decreaseNodes.Clear();

        while(m_open.Count > 0)
        {
            SearchNode n = PopFromOpen();
            ForeachNeighbors(n, (s) =>
            {
                InitializeState(s);
                if(h(s) > c(s, n) + h(n))
                {
                    s.H = c(s, n) + h(n);
                    if (s.Opened) RemoveFromOpen(s);
                    AddToOpen(s, h(s));
                }
            });
        }
    }

    private IEnumerator ShowPath()
    {
        ForeachNode((n) =>
        {
            if (!n.IsObstacle() && n.SearchType == SearchType.Path)
                n.SetSearchType(SearchType.None, true);
        });

        if(Mathf.Approximately(m_pathCost[m_counter], float.MaxValue) == false)
        {
            SearchNode lastNode = m_currGoal;
            while (lastNode != null && lastNode != m_mapStart)
            {
                lastNode.SetSearchType(SearchType.Path, true);
                lastNode = lastNode.Parent;
            }
        }
        else
        {
            Debug.LogError("找不到路径");
        }

        yield return new WaitForSeconds(1);
    }

    protected override float h(SearchNode s)
    {
        if (s.H < 0)
            s.H = CalcHeuristic(s, m_currGoal);

        return s.H;
    }

    #region 监听事件
    public override void NotifyChangeNode(List<SearchNode> nodes, bool increaseCost)
    {
        for(int i = 0; i < nodes.Count; i++)
        {
            if (increaseCost)
                m_decreaseNodes.Remove(nodes[i]);
            else
                m_decreaseNodes.Add(nodes[i]);
        }
    }

    public override void NotifyChangeStart(SearchNode startNode)
    {
        m_mapStart = startNode;
    }

    public override void NotifyChangeGoal(SearchNode goalNode)
    {
        m_mapGoal = goalNode;
    }
    #endregion

    #region 处理开放列表
    private void AddToOpen(SearchNode node, float priority)
    {
        m_open.Enqueue(node, priority);
        node.Opened = true;
        node.SetSearchType(SearchType.Open, true, true);
    }

    private void RemoveFromOpen(SearchNode node)
    {
        m_open.Remove(node);
        node.Opened = false;
        node.SetSearchType(SearchType.Expanded, true, true);
    }

    private SearchNode PopFromOpen()
    {
        SearchNode node = m_open.Dequeue();
        node.Opened = false;
        node.SetSearchType(SearchType.Expanded, true, true);
        return node;
    }

    private float MinKey()
    {
        if (m_open.Count > 0)
            return m_open.FirstPriority;
        else
            return float.MaxValue;
    }

    private void ClearOpen(bool clearShow)
    {
        m_open.Clear();

        ForeachNode((n) =>
        {
            n.Opened = false;

            if (!n.IsObstacle() && n.SearchType == SearchType.Open)
                n.SetSearchType(SearchType.None, true, true);

            if (clearShow && !n.IsObstacle() && n.SearchType == SearchType.Expanded)
                n.SetSearchType(SearchType.None, true, true);
        });
    }
    #endregion
}