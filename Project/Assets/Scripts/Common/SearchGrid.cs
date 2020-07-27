using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 操作：
/// 拖动起点和终点位置
/// </summary>
public class SearchGrid : BaseGrid<SearchNode>
{
    private static SearchGrid m_instance;

    public SearchAlgo m_searchAlgo;
    public HeuristicType m_heuristicType;
    public int m_unitSize = 1;
    public float m_weight = 1;
    public float m_showTime = 0.1f;

    private SearchNode m_startNode;
    private SearchNode m_goalNode;

    private bool m_dragStartNode;
    private bool m_dragGoalNode;

    private BaseSearchAlgo m_algo;

    #region get-set
    public static SearchGrid Instance { get { return m_instance; } }

    public SearchNode StartNode { get { return m_startNode; } }

    public SearchNode EndNode { get { return m_goalNode; } }
    #endregion

    protected override void Awake()
    {
        base.Awake();

        m_instance = this;

        m_startNode = GetNode(0, m_row / 2);
        m_startNode.SetSearchType(SearchType.Start, false);
        m_goalNode = GetNode(m_col - 1, m_row / 2);
        m_goalNode.SetSearchType(SearchType.Goal, false);
    }

    protected override void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            BaseNode node = GetMouseOverNode();
            if (node == m_startNode)
                m_dragStartNode = true;
            else if (node == m_goalNode)
                m_dragGoalNode = true;
        }
        else if(Input.GetMouseButtonUp(0))
        {
            m_dragStartNode = m_dragGoalNode = false;
        }
        else if(Input.GetMouseButton(0))
        {
            if (m_dragStartNode)
                DragStartNode();
            else if (m_dragGoalNode)
                DragGoalNode();
            else
                AddObstacle();
        }
        else if(Input.GetMouseButton(1))
        {
            RemoveObstacle();
        }
        else if(Input.GetKeyDown(KeyCode.Space))
        {
            Generate();
        }
    }

    protected override void Generate()
    {
        StopAllCoroutines();

        Reset();

        if (m_algo == null)
            m_algo = GetAlgorithm();
        if (m_algo != null)
            StartCoroutine(m_algo.Process());
    }

    protected override bool AddObstacle()
    {
        SearchNode node = GetMouseOverNode();
        if (node != null && node != m_startNode && node != m_goalNode)
        {
            if(node.Cost != Define.c_costObstacle)
            {
                node.SetCost(Define.c_costObstacle);

                if (m_algo != null)
                    m_algo.NotifyChangeNode(new List<SearchNode>() { node }, true);

                return true;
            }
        }

        return false;
    }

    protected override bool RemoveObstacle()
    {
        SearchNode node = GetMouseOverNode();
        if(node != null && node != m_startNode && node != m_goalNode)
        {
            if(node.Cost != Define.c_costRoad)
            {
                node.SetCost(Define.c_costRoad);

                if(m_algo != null)
                    m_algo.NotifyChangeNode(new List<SearchNode>() { node }, false);

                return true;
            }
        }

        return false;
    }

    private SearchNode DragNode()
    {
        SearchNode node = GetMouseOverNode();
        if (node != null && node != m_startNode && node != m_goalNode && node.IsObstacle() == false)
            return node;
        else
            return null;
    }

    private void DragStartNode()
    {
        SearchNode node = DragNode();
        if (node != null)
        {
            m_startNode.SetSearchType(SearchType.None, false);
            m_startNode = node;
            m_startNode.SetSearchType(SearchType.Start, false);

            if (m_algo != null)
                m_algo.NotifyChangeStart(m_startNode);
        }
    }

    private void DragGoalNode()
    {
        SearchNode node = DragNode();
        if (node != null)
        {
            m_goalNode.SetSearchType(SearchType.None, false);
            m_goalNode = node;
            m_goalNode.SetSearchType(SearchType.Goal, false);

            if (m_algo != null)
                m_algo.NotifyChangeGoal(m_goalNode);
        }
    }

    private void Reset()
    {
        for(int y = 0; y < m_row; y++)
        {
            for(int x = 0; x < m_col; x++)
            {
                m_nodes[y, x].Reset();
            }
        }

        m_startNode.SetSearchType(SearchType.Start, false);
        m_goalNode.SetSearchType(SearchType.Goal, false);

        m_algo = null;
    }

    private BaseSearchAlgo GetAlgorithm()
    {
        BaseSearchAlgo algo = null;

        switch(m_searchAlgo)
        {
            case SearchAlgo.A_Star:
                algo = new AStar(m_startNode, m_goalNode, m_nodes, m_weight, m_showTime);
                break;
            case SearchAlgo.Theta_Star:
                algo = new ThetaStar(m_startNode, m_goalNode, m_nodes, m_weight, m_showTime);
                break;
            case SearchAlgo.LazyTheta_Star:
                algo = new LazyThetaStar(m_startNode, m_goalNode, m_nodes, m_weight, m_showTime);
                break;
            case SearchAlgo.BestFirstSearch:
                algo = new BestFirstSearch(m_startNode, m_goalNode, m_nodes, m_weight, m_showTime);
                break;
            case SearchAlgo.BreadthFirstSearch:
                algo = new BreadthFirstSearch(m_startNode, m_goalNode, m_nodes, m_weight, m_showTime);
                break;
            case SearchAlgo.DijkstraSearch:
                algo = new DijkstraSearch(m_startNode, m_goalNode, m_nodes, m_weight, m_showTime);
                break;
            case SearchAlgo.JPS:
                algo = new JumpPointSearch(m_startNode, m_goalNode, m_nodes, m_weight, m_showTime);
                break;
            case SearchAlgo.JPSPlus:
                algo = new JPSPlus(m_startNode, m_goalNode, m_nodes, m_weight, m_showTime);
                break;
            case SearchAlgo.BiA_Star:
                algo = new BiAStar(m_startNode, m_goalNode, m_nodes, m_weight, m_showTime);
                break;

            #region Incremental
            case SearchAlgo.D_Star:
                algo = new DStar(m_startNode, m_goalNode, m_nodes, m_showTime);
                break;
            case SearchAlgo.FocussedD_Star:
                algo = new FocussedDStar(m_startNode, m_goalNode, m_nodes, m_showTime);
                break;
            case SearchAlgo.LPA_Star:
                algo = new LPAStar(m_startNode, m_goalNode, m_nodes, m_showTime);
                //algo = new LPAStar_Optimized(m_startNode, m_endNode, m_nodes, m_showTime);
                break;
            case SearchAlgo.DstarLite:
                algo = new DStarLite(m_startNode, m_goalNode, m_nodes, m_showTime);
                break;
            case SearchAlgo.Path_AA_Star:
                algo = new Path_AAStar(m_startNode, m_goalNode, m_nodes, m_showTime);
                break;
            case SearchAlgo.Tree_AA_Star:
                algo = new Tree_AAStar(m_startNode, m_goalNode, m_nodes, m_showTime);
                break;
            #endregion

            #region Moving Target
            case SearchAlgo.GAA_Star:
                algo = new GAAStar(m_startNode, m_goalNode, m_nodes, m_showTime);
                break;
            case SearchAlgo.GFRA_Star:
                algo = new GFRAStar(m_startNode, m_goalNode, m_nodes, m_showTime);
                break;
            case SearchAlgo.MT_DstarLite:
                algo = new MT_DStarLite(m_startNode, m_goalNode, m_nodes, m_showTime);
                break;
            #endregion

            case SearchAlgo.AnnotatedA_Star:
                algo = new AnnotatedAStar(m_startNode, m_goalNode, m_nodes, m_weight, m_showTime, m_unitSize);
                break;
            default:
                Debug.LogError($"No code for SearchAlgo={m_searchAlgo}");
                break;
        }

        return algo;
    }

    public float CalcHeuristic(Vector2Int a, Vector2Int b, float weight)
    {
        switch (m_heuristicType)
        {
            case HeuristicType.Manhattan:
                return Heuristic.Manhattan(a, b) * weight;
            case HeuristicType.Chebyshev:
                return Heuristic.Chebyshev(a, b) * weight;
            case HeuristicType.Octile:
                return Heuristic.Octile(a, b) * weight;
            case HeuristicType.Euclidean:
                return Heuristic.Euclidean(a, b) * weight;
            default:
                Debug.LogError($"No code for HeuristicType={m_heuristicType}");
                return 0;
        }
    }
}