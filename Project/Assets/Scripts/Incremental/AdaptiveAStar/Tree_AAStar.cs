using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

/// <summary>
/// Tree Adaptive A*
/// 
/// 调整：
/// * Iteration表示论文里的Generated
/// </summary>
public class Tree_AAStar : BaseSearchAlgo
{
    private SearchNode m_start;
    private SearchNode m_goal;
    private readonly SimplePriorityQueue<SearchNode> m_open = new SimplePriorityQueue<SearchNode>();
    private readonly HashSet<SearchNode> m_close = new HashSet<SearchNode>();
    private readonly List<SearchNode> m_increaseCostNodes = new List<SearchNode>();

    /// <summary>
    /// 记录第几次执行A*
    /// </summary>
    private int m_counter;

    /// <summary>
    /// 记录路径x对应的注入路径
    /// </summary>
    private readonly Dictionary<int, HashSet<int>> m_paths = new Dictionary<int, HashSet<int>>();

    /// <summary>
    /// 记录路径x对应的最小h值
    /// </summary>
    private readonly Dictionary<int, float> m_minH = new Dictionary<int, float>();

    /// <summary>
    /// 记录路径x对应的最大h值
    /// </summary>
    private readonly Dictionary<int, float> m_maxH = new Dictionary<int, float>();

    public Tree_AAStar(SearchNode start, SearchNode goal, SearchNode[,] nodes, float showTime)
        : base(start, goal, nodes, showTime) { }

    public override IEnumerator Process()
    {
        m_start = m_mapStart;
        m_goal = m_mapGoal;
        m_increaseCostNodes.Clear();

        m_counter = 1;
        m_maxH[0] = -1; //终点id是0
        ForeachNode((s) =>
        {
            s.Iteration = s.Id = 0;
            s.Reusabletree = s.Parent = null;
        });

        while(m_start != m_goal)
        {
            //经典A*步骤
            InitializeState(m_start);
            m_start.G = 0;
            ClearOpen();
            ClearClose();
            AddToOpen(m_start, g(m_start) + h(m_start));
            if (ComputePath() == false)
            {
                Debug.LogError("找不到路径");
                yield break;
            }

            //一步一步移动
            while(h(m_start) <= m_maxH[m_start.Id]) //起点在复用树中且不是终点
            {
                //往后走一步
                m_start.SetSearchType(SearchType.Path, true);
                m_start = m_start.Reusabletree;
                m_start.SetSearchType(SearchType.CurtPos, true);

                //检查环境变化
                for(int i = 0; i < m_increaseCostNodes.Count; i++)
                {
                    SearchNode s = m_increaseCostNodes[i];
                    ForeachNeighbors(s, (n) =>
                    {
                        //因为是无向图，所以两个方向都要检查
                        if (s.Reusabletree == n)
                            RemovePaths(s); //增加代价的节点及之前的路径都要移除
                        if (n.Reusabletree == s)
                            RemovePaths(n);
                    });
                }
                m_increaseCostNodes.Clear();

                yield return new WaitForSeconds(m_showTime);
            }

            //当前路径发生了变化，准备进行下一次A*寻路
            m_counter++; 
        }

        Debug.Log("到达目标");
    }

    private void InitializeState(SearchNode s)
    {
        if(s.Iteration == 0)
        {
            s.G = m_infinite;
            s.H = CalcHeuristic(s, m_goal);
        }
        else if(s.Iteration != m_counter)
        {
            s.G = m_infinite;
        }

        s.Iteration = m_counter;
    }

    private void AddPath(SearchNode s)
    {
        //寻路结束的点不是终点，说明注入到其他路径了
        if (s != m_goal)
            m_paths[s.Id].Add(m_counter);

        m_minH[m_counter] = h(s);
        m_maxH[m_counter] = h(m_start);
        m_paths[m_counter] = new HashSet<int>();
        while(s != m_start)
        {
            SearchNode auxS = s;
            s = s.Parent;
            s.Id = m_counter;
            s.Reusabletree = auxS;
        }
    }

    private void RemovePaths(SearchNode s)
    {
        int x = s.Id;
        if (m_maxH[x] > h(s.Reusabletree))
            m_maxH[x] = h(s.Reusabletree);

        //把该节点之前注入的路径都清理掉
        Queue<int> queue = new Queue<int>();
        List<int> removeList = new List<int>();
        foreach (var nx in m_paths[x])
        {
            if(m_maxH[x] < m_maxH[nx])
            {
                queue.Enqueue(nx);
                removeList.Add(nx);
            }
        }

        for (int i = 0; i < removeList.Count; i++)
            m_paths[x].Remove(removeList[i]);
        removeList.Clear();

        //把注入路径和其自身的注入路径从复用树中移除
        while(queue.Count > 0)
        {
            x = queue.Dequeue();
            if(m_maxH[x] > m_minH[x])
            {
                m_maxH[x] = m_minH[x];
                foreach(var nx in m_paths[x])
                {
                    queue.Enqueue(nx);
                    removeList.Add(nx);
                }

                for (int i = 0; i < removeList.Count; i++)
                    m_paths[x].Remove(removeList[i]);
                removeList.Clear();
            }
        }
    }

    private bool ComputePath()
    {
        while(m_open.Count > 0)
        {
            SearchNode s = PopMinFromOpen();
            if(s == m_goal || h(s) <= m_maxH[s.Id])
            {
                foreach (var ns in m_close)
                    ns.H = g(s) + h(s) - g(ns); //Adaptive A*的优化h思想
                AddPath(s); //把本次寻路结果加到复用树中
                return true;
            }

            AddToClose(s);

            ForeachNeighbors(s, (ns) =>
            {
                InitializeState(ns);
                if(g(ns) > g(s) + c(s, ns))
                {
                    ns.G = g(s) + c(s, ns);
                    ns.Parent = s;
                    if (ns.Opened)
                        RemoveFromOpen(ns);
                    AddToOpen(ns, g(ns) + h(ns));
                }
            });
        }

        return false;
    }

    #region 事件监听
    public override void NotifyChangeNode(List<SearchNode> nodes, bool increaseCost)
    {
        //Tree AA*只能处理代价变大的情况
        if (increaseCost)
            m_increaseCostNodes.AddRange(nodes);
    }
    #endregion

    #region OPEN CLOSE
    private void AddToOpen(SearchNode s, float priority)
    {
        m_open.Enqueue(s, priority);
        s.Opened = true;
        s.SetSearchType(SearchType.Open, true, true);
    }

    private void RemoveFromOpen(SearchNode s)
    {
        m_open.Remove(s);
        s.Opened = false;
        s.SetSearchType(SearchType.Expanded, true, true);
    }

    private SearchNode PopMinFromOpen()
    {
        SearchNode s = m_open.Dequeue();
        s.Opened = false;
        s.SetSearchType(SearchType.Expanded, true, true);
        return s;
    }

    private void ClearOpen()
    {
        foreach(var s in m_open)
        {
            s.Opened = false;
            s.SetSearchType(SearchType.None, true, true);
        }

        m_open.Clear();
    }

    private void AddToClose(SearchNode s)
    {
        s.Closed = true;
        m_close.Add(s);
        s.SetSearchType(SearchType.Expanded, true, true);
    }

    private void ClearClose()
    {
        foreach(var s in m_close)
        {
            s.Closed = false;
            s.SetSearchType(SearchType.None, true, true);
        }

        m_close.Clear();
    }
    #endregion
}
