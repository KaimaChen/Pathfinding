using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generalized Fringe-Retrieving A*
/// 
/// 例子使用方式：调高showTime，然后移动目标点，观察结果
/// 注意：该算法没处理环境中新增/减少的阻挡
/// </summary>
public class GFRAStar : BaseSearchAlgo
{
    private readonly int m_noInitG; //未初始化时g的值，不用float.MaxValue是怕与h相加后溢出
    private int m_iteration;
    private SearchNode m_currPos; //当前机器人的位置
    private SearchNode m_previousStart; //上次搜索使用的起点
    private SearchNode m_currStart; //当前搜索使用的起点
    private SearchNode m_currGoal; //当前搜索使用的终点
    private readonly List<SearchNode> m_open = new List<SearchNode>();
    private readonly HashSet<SearchNode> m_deleted = new HashSet<SearchNode>(); //记录Step 2中移除的节点，以便Step 4遍历使用

    public GFRAStar(SearchNode start, SearchNode end, SearchNode[,] nodes, float showTime)
        : base(start, end, nodes, showTime)
    {
        m_noInitG = m_mapWidth * m_mapHeight * 10;
    }

    public override IEnumerator Process()
    {
        m_currPos = m_mapStart;
        m_currStart = m_mapStart;
        m_currGoal = m_mapGoal;

        ForeachNode((s) =>
        {
            s.Iteration = 0;
            s.Expanded = false;
            s.Parent = null;
        });

        m_iteration = 1;

        InitializeState(m_currStart);
        m_currStart.G = 0;
        AddToOpen(m_currStart);

        m_deleted.Clear();

        while (m_currStart != m_currGoal)
        {
            if(!ComputeCostMinimalPath())
            {
                Debug.LogError("寻路失败");
                yield break;
            }

            bool openListIncomplete = false;
            while(TestClosedList(m_currGoal))
            {
                //如果目标节点还在原本的最短路径上，那么直接利用上次的寻路结果
                while(IsOnTheMinimalPath(m_mapGoal, out SearchNode nextNode) && m_currPos != m_mapGoal)
                {
                    MoveForwardOneStep(nextNode);
                    yield return new WaitForSeconds(m_showTime);
                }

                if (m_currPos == m_mapGoal)
                {
                    Debug.LogError("到达目标");
                    yield break;
                }

                m_previousStart = m_currStart;
                m_currStart = m_currPos;
                m_currGoal = m_mapGoal;

                //起点发生了变化，则复用该新起点所在子树
                if(m_currStart != m_previousStart)
                {
                    GeneralizedStep2();
                    openListIncomplete = true;
                }
            }

            //如果复用了子树，那么还要补上相关节点来满足A*的属性1
            if(openListIncomplete)
            {
                m_iteration++;
                GeneralizedStep4();
            }
            //else 起点没变化，而终点离开了原本的最短路径，则重新进行寻路（原本的搜索树能全部复用，所以不用做任何处理）
        }

        yield break;
    }

    private void InitializeState(SearchNode s)
    {
        if(s.Iteration != m_iteration)
        {
            s.G = m_noInitG;
            s.Iteration = m_iteration;
            s.Expanded = false;
        }
    }

    /// <summary>
    /// 检查是否在关闭列表中
    /// </summary>
    private bool TestClosedList(SearchNode s)
    {
        return (s == m_currStart || (s.Expanded && s.Parent != null));
    }

    /// <summary>
    /// 使用A*算法计算最短路径
    /// </summary>
    private bool ComputeCostMinimalPath()
    {
        while(m_open.Count > 0)
        {
            SearchNode s = PopMinFromOpen();
            s.Expanded = true;
            ForeachSucc(s, (n) =>
            {
                if (!TestClosedList(n))
                {
                    InitializeState(n);
                    if(g(n) > g(s) + c(s, n))
                    {
                        n.G = g(s) + c(s, n);
                        n.Parent = s;
                        if (!m_open.Contains(n))
                            AddToOpen(n);
                    }
                }
            });

            if (s == m_currGoal)
                return true;
        }

        return false;
    }

    private bool CheckSubRoot(SearchNode s, SearchNode subRoot)
    {
        if (s == subRoot)
            return true;

        while (s.Parent != null)
        {
            s = s.Parent;

            if (s == subRoot)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 复用根为新起点的子树，把其他部分删掉
    /// </summary>
    private void GeneralizedStep2()
    {
        m_currStart.Parent = null;

        ForeachNode((s) =>
        {
            if (!CheckSubRoot(s, m_currStart))
            {
                m_deleted.Add(s);
                s.Parent = null;
                if (s.Opened)
                    RemoveFromOpen(s);
            }
        });
    }

    /// <summary>
    /// 构建完整的OPEN，需要覆盖CLOSED的外围，从而满足A*算法的属性1
    /// </summary>
    private void GeneralizedStep4()
    {
        foreach (var s in m_open)
            s.Iteration = m_iteration;

        foreach(var s in m_deleted)
        {
            if(IsAnyPredClosed(s))
            {
                InitializeState(s);
                ForeachPred(s, (n) =>
                {
                    if(TestClosedList(n) && g(s) > g(n) + c(n, s))
                    {
                        s.G = g(n) + c(n, s);
                        s.Parent = n;
                    }
                });

                AddToOpen(s); //s肯定不在Open中，所以这里不用先判断s是否在Open表
            }
        }

        m_deleted.Clear();
    }

    private bool IsAnyPredClosed(SearchNode s)
    {
        bool result = false;
        List<SearchNode> neighbors = GetNeighbors(s); //对于无向图，前任节点就是所有邻居
        for (int i = 0; i < neighbors.Count; i++)
        {
            if (TestClosedList(neighbors[i]))
            {
                result = true;
                break;
            }
        }

        return result;
    }

    /// <summary>
    /// 指定节点是否在当前的最短路径上
    /// </summary>
    /// <param name="s">要检查的节点</param>
    /// <param name="nextNode">机器人当前位置的下一个位置</param>
    /// <returns></returns>
    private bool IsOnTheMinimalPath(SearchNode s, out SearchNode nextNode)
    {
        nextNode = m_currPos;

        if (s == m_currPos)
            return true;

        SearchNode lastNode = s;
        while(lastNode.Parent != null)
        {
            SearchNode oldNode = lastNode;
            lastNode = lastNode.Parent;
            if (lastNode == m_currPos)
            {
                nextNode = oldNode;
                return true;
            }
        }

        return false;
    }

    private void MoveForwardOneStep(SearchNode nextNode)
    {
        m_currPos.SetSearchType(SearchType.Path, true);
        m_currPos = nextNode;
        m_currPos.SetSearchType(SearchType.CurtPos, true);
    }

    private void ForeachSucc(SearchNode s, Action<SearchNode> func)
    {
        ForeachNeighbors(s, func); //对于无向图，后继节点就是所有邻居
    }

    private void ForeachPred(SearchNode s, Action<SearchNode> func)
    {
        ForeachNeighbors(s, func); //对于无向图，前任节点就是所有邻居
    }

    protected override float h(SearchNode s)
    {
        if (s.H < 0)
            s.H = CalcHeuristic(s, m_currGoal);

        return s.H;
    }

    private float h(SearchNode s, SearchNode goal)
    {
        return CalcHeuristic(s, goal);
    }

    #region 事件监听
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
    private void AddToOpen(SearchNode node)
    {
        m_open.Add(node);
        node.Opened = true;
        node.SetSearchType(SearchType.Open, true, true);
    }

    private void RemoveFromOpen(SearchNode node)
    {
        m_open.Remove(node);
        node.Opened = false;
        node.SetSearchType(SearchType.Expanded, true, true);
    }

    private SearchNode PopMinFromOpen()
    {
        SearchNode minNode = m_open[0];
        float minKey = g(minNode) + h(minNode, m_currGoal);
        for(int i = 1; i < m_open.Count; i++)
        {
            SearchNode s = m_open[i];
            float f = g(s) + h(s, m_currGoal);
            if(f < minKey)
            {
                minKey = f;
                minNode = s;
            }
        }

        RemoveFromOpen(minNode);
        return minNode;
    }
    #endregion
}