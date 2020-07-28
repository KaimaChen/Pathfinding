using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moving Target D* Lite
/// 作用：处理动态环境+起点终点同时变化的情景
/// 思路：D* Lite + G-FRA*
/// </summary>
public class MT_DStarLite : DStarLite
{
    private SearchNode m_currPos;
    private SearchNode m_currGoal;
    private readonly HashSet<SearchNode> m_deleted = new HashSet<SearchNode>();

    public MT_DStarLite(SearchNode start, SearchNode goal, SearchNode[,] nodes, float showTime)
        : base(start, goal, nodes, showTime) { }

    public override IEnumerator Process()
    {
        m_currPos = m_mapStart;
        m_currStart = m_mapStart;
        m_currGoal = m_mapGoal;
        
        Initialize();
        while(BeginNode() != EndNode())
        {
            SearchNode oldStart = m_currStart;
            SearchNode oldGoal = m_currGoal;

            ComputeShortestPath();
            if (m_currGoal.Rhs >= c_large)
            {
                Debug.LogError("找不到路径");
                yield break;
            }

            List<SearchNode> path = GetPath();
            List<SearchNode> nearChanged = new List<SearchNode>();
            //如果终点仍在路径上且环境没检测到变化，则继续往前走直到到达终点
            while(m_currPos != m_mapGoal && path.Contains(m_mapGoal) && nearChanged.Count <= 0)
            {
                MoveOneStep(path, nearChanged);
                yield return new WaitForSeconds(m_showTime);
            }
            if(m_currPos == m_currGoal)
            {
                Debug.LogError("到达目标");
                yield break;
            }

            m_currStart = m_currPos;
            m_currGoal = m_mapGoal;
            m_km += c(oldGoal, m_currGoal);

            if (oldStart != m_currStart)
            {
                //BasicDeletion(oldStart); //Basic MT-D* Lite
                OptimizedDeletion(); //MT-D* Lite
            }

            HandleChangedNode(nearChanged);
        }

        yield break;
    }

    //因为起点和终点都会发生变化，因此没必要进行反向寻路
    protected override SearchNode BeginNode()
    {
        return m_currStart;
    }

    //因为起点和终点都会发生变化，因此没必要进行反向寻路
    protected override SearchNode EndNode()
    {
        return m_currGoal;
    }

    protected override void Initialize()
    {
        m_currGoal = m_mapGoal;

        base.Initialize();
    }

    private void BasicDeletion(SearchNode oldStart)
    {
        m_currStart.SetRhs(m_currStart.Rhs, null); //新起点的parent(这里是rhsSource)要按照起点的要求设置为空
        UpdateVertex(oldStart); //将旧起点视为普通节点进行处理
    }

    /// <summary>
    /// 利用G-FRA* 的思想进行优化
    /// 尽早处理根不在当前起点的其他节点，而不是等到ComputeShortestPath再处理，因为那时就需要更多操作了
    /// </summary>
    private void OptimizedDeletion()
    {
        m_deleted.Clear();
        BeginNode().SetRhs(BeginNode().Rhs, null);

        //类似FRA*的Step 2
        ForeachNode((s) =>
        {
            //处理搜索树中不在根为当前起点的其他节点
            if(IsInSearchTree(s) && IsSubRoot(s, BeginNode()) == false)
            {
                s.SetRhs(c_large, null);
                s.G = c_large;
                RemoveFromOpenQueue(s);
                m_deleted.Add(s);
            }
        });

        //类似FRA*的Step 4
        foreach(var s in m_deleted)
        {
            UpdateRhs(s);

            if (s.Rhs < c_large)
            {
                s.LPAKey = CalculateKey(s);
                AddOrUpdateOpenQueue(s);
            }
        }
    }

    /// <summary>
    /// 在OPEN或CLOSE的节点属于搜索树
    /// </summary>
    private bool IsInSearchTree(SearchNode s)
    {
        return s.Opened || s.RhsSource != null;
    }

    private bool IsSubRoot(SearchNode s, SearchNode subRoot)
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

    private List<SearchNode> GetPath()
    {
        List<SearchNode> path = new List<SearchNode>() { EndNode() };

        SearchNode s = EndNode();
        while(s.RhsSource != null)
        {
            s = s.RhsSource;
            if (s == BeginNode())
                break;

            path.Add(s);
        }

        path.Reverse();
        return path;
    }

    private void MoveOneStep(List<SearchNode> path, List<SearchNode> nearChanged)
    {
        m_currPos.SetSearchType(SearchType.Path, true);
        m_currPos = path[0];
        path.RemoveAt(0);
        m_currPos.SetSearchType(SearchType.CurtPos, true);

        //假设检测器只能检查附近的点
        List<SearchNode> neighbors = GetNeighbors(m_currPos);
        for (int i = 0; i < neighbors.Count; i++)
        {
            Vector2Int pos = neighbors[i].Pos;
            if (neighbors[i].Cost != m_foundMap[pos.y, pos.x])
            {
                m_foundMap[pos.y, pos.x] = neighbors[i].Cost;
                nearChanged.Add(neighbors[i]);
            }
        }
    }

    #region 事件监听
    public override void NotifyChangeGoal(SearchNode goalNode)
    {
        m_mapGoal = goalNode;
    }
    #endregion
}