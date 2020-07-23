using UnityEngine;
using UnityEditor;
using System;

public class SearchNode : BaseNode
{
    [SerializeField] private float m_g = float.MaxValue;
    [SerializeField] private float m_h = -1;
    [SerializeField] private SearchNode m_parent;
    [SerializeField] private SearchType m_searchType;
    [SerializeField] private bool m_opened;
    [SerializeField] private bool m_closed;

    private MeshRenderer m_renderer;
    private Material m_mat;
    private TextMesh m_valueText;

    #region get-set
    public SearchType SearchType
    {
        get { return m_searchType; }
    }

    public SearchNode Parent
    {
        get { return m_parent; }
        set { m_parent = value; }
    }

    public bool Opened
    {
        get { return m_opened; }
        set
        {
            m_opened = value;
            if (m_opened)
                m_closed = false;
        }
    }

    public bool Closed
    {
        get { return m_closed; }
        set
        {
            m_closed = value;
            if (m_closed)
                m_opened = false;
        }
    }

    public float G 
    { 
        get { return m_g; } 
        set { m_g = value; }
    }

    public float H
    {
        get { return m_h; }
        set { m_h = value; }
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        Handles.color = Color.black;

        bool useBackState = SearchGrid.Instance.m_searchAlgo == SearchAlgo.Path_AA_Star;

        int max = 100;
        SearchNode curt = this;
        SearchNode prev = useBackState ? m_backState : m_parent;
        while(curt != null && prev != null && --max > 0)
        {
            Handles.DrawLine(curt.transform.position, prev.transform.position);

            curt = prev;
            prev = useBackState ? prev.m_backState : prev.m_parent;
        }

        if (max <= 0)
            Debug.LogError("画线怎么会超过100，是不是哪里出错了？");
    }

    public override void Init(int x, int y, byte cost)
    {
        base.Init(x, y, cost);

        m_renderer = GetComponent<MeshRenderer>();
        m_mat = m_renderer.material;
        m_mat.color = Define.Cost2Color(cost);

        m_valueText = transform.Find("Value").GetComponent<TextMesh>();
        m_valueText.gameObject.SetActive(false);

        InitJPSPlus();
    }

    public void Reset()
    {
        m_g = float.MaxValue;
        m_h = -1;
        m_parent = null;
        m_opened = m_closed = false;

        m_mat.color = Define.Cost2Color(m_cost);
        m_searchType = SearchType.None;

        m_trueClearance = 0;
        m_valueText.gameObject.SetActive(false);
    }

    public override void SetCost(byte cost)
    {
        base.SetCost(cost);
        m_mat.color = Define.Cost2Color(cost);
    }

    public void SetSearchType(SearchType type, bool excludeStartEnd, bool excludeSpecial = false)
    {
        if (excludeStartEnd && (m_searchType == SearchType.Start || m_searchType == SearchType.Goal))
            return;

        if (excludeSpecial && (IsObstacle() || m_searchType == SearchType.Path || m_searchType == SearchType.CurtPos))
            return;

        m_searchType = type;
        m_mat.color = Define.SearchType2Color(type);
    }

    public void SetParent(SearchNode parent, float g)
    {
        m_parent = parent;
        m_g = g;
    }

    public float ValidH(float weight = 1)
    {
        if (m_h < 0)
            m_h = SearchGrid.Instance.CalcHeuristic(Pos, SearchGrid.Instance.EndNode.Pos, weight);

        return m_h;
    }

    public float F(float weight = 1)
    {
        return m_g + ValidH(weight);
    }

    #region JPSPlus
    private TextMesh m_JPSPlusEast;
    private TextMesh m_JPSPlusWest;
    private TextMesh m_JPSPlusNorth;
    private TextMesh m_JPSPlusSouth;
    private TextMesh m_JPSPlusNorthEast;
    private TextMesh m_JPSPlusNorthWest;
    private TextMesh m_JPSPlusSouthEast;
    private TextMesh m_JPSPlusSouthWest;

    private void InitJPSPlus()
    {
        Transform jpsPlus = transform.Find("JPSPlus");
        m_JPSPlusEast = jpsPlus.Find("JPSEast").GetComponent<TextMesh>();
        m_JPSPlusEast.gameObject.SetActive(false);
        m_JPSPlusWest = jpsPlus.Find("JPSWest").GetComponent<TextMesh>();
        m_JPSPlusWest.gameObject.SetActive(false);
        m_JPSPlusNorth = jpsPlus.Find("JPSNorth").GetComponent<TextMesh>();
        m_JPSPlusNorth.gameObject.SetActive(false);
        m_JPSPlusSouth = jpsPlus.Find("JPSSouth").GetComponent<TextMesh>();
        m_JPSPlusSouth.gameObject.SetActive(false);
        m_JPSPlusNorthEast = jpsPlus.Find("JPSNorthEast").GetComponent<TextMesh>();
        m_JPSPlusNorthEast.gameObject.SetActive(false);
        m_JPSPlusNorthWest = jpsPlus.Find("JPSNorthWest").GetComponent<TextMesh>();
        m_JPSPlusNorthWest.gameObject.SetActive(false);
        m_JPSPlusSouthEast = jpsPlus.Find("JPSSouthEast").GetComponent<TextMesh>();
        m_JPSPlusSouthEast.gameObject.SetActive(false);
        m_JPSPlusSouthWest = jpsPlus.Find("JPSSouthWest").GetComponent<TextMesh>();
        m_JPSPlusSouthWest.gameObject.SetActive(false);
    }

    public void ShowDistance(int east, int west, int north, int south, int northEast, int northWest, int southEast, int southWest)
    {
        Color positiveColor = new Color(0, 0.8f, 0); 
        Color otherColor = Color.black;

        m_JPSPlusEast.gameObject.SetActive(true);
        m_JPSPlusEast.text = east.ToString();
        m_JPSPlusEast.color = east > 0 ? positiveColor : otherColor;

        m_JPSPlusWest.gameObject.SetActive(true);
        m_JPSPlusWest.text = west.ToString();
        m_JPSPlusWest.color = west > 0 ? positiveColor : otherColor;

        m_JPSPlusNorth.gameObject.SetActive(true);
        m_JPSPlusNorth.text = north.ToString();
        m_JPSPlusNorth.color = north > 0 ? positiveColor : otherColor;

        m_JPSPlusSouth.gameObject.SetActive(true);
        m_JPSPlusSouth.text = south.ToString();
        m_JPSPlusSouth.color = south > 0 ? positiveColor : otherColor;

        m_JPSPlusNorthEast.gameObject.SetActive(true);
        m_JPSPlusNorthEast.text = northEast.ToString();
        m_JPSPlusNorthEast.color = northEast > 0 ? positiveColor : otherColor;

        m_JPSPlusNorthWest.gameObject.SetActive(true);
        m_JPSPlusNorthWest.text = northWest.ToString();
        m_JPSPlusNorthWest.color = northWest > 0 ? positiveColor : otherColor;

        m_JPSPlusSouthEast.gameObject.SetActive(true);
        m_JPSPlusSouthEast.text = southEast.ToString();
        m_JPSPlusSouthEast.color = southEast > 0 ? positiveColor : otherColor;

        m_JPSPlusSouthWest.gameObject.SetActive(true);
        m_JPSPlusSouthWest.text = southWest.ToString();
        m_JPSPlusSouthWest.color = southWest > 0 ? positiveColor : otherColor;
    }
    #endregion

    #region Goal Bounding
    private SearchNode m_gbStartNode;

    public SearchNode GBStartNode
    {
        get { return m_gbStartNode; }
        set { m_gbStartNode = value; }
    }
    #endregion

    #region Bidirection
    private const int c_startOpenValue = 1;
    private const int c_endOpenValue = 2;
    private int m_openValue;

    public void SetStartOpen()
    {
        m_openValue = c_startOpenValue;
    }

    public bool IsStartOpen()
    {
        return m_openValue == c_startOpenValue;
    }

    public void SetEndOpen()
    {
        m_openValue = c_endOpenValue;
    }

    public bool IsEndOpen()
    {
        return m_openValue == c_endOpenValue;
    }
    #endregion

    #region LPA*
    [SerializeField] private float m_rhs;
    [SerializeField] private SearchNode m_rhsSource; //rhs是根据哪个节点计算得来的
    [SerializeField] private LPAKey m_LPAKey;
    [SerializeField] private int m_iteration;

    public float Rhs
    {
        get { return m_rhs; }
    }

    public SearchNode RhsSource
    {
        get { return m_rhsSource; }
    }

    public LPAKey LPAKey
    {
        get { return m_LPAKey; }
        set { m_LPAKey = value; }
    }

    public int Iteration
    {
        get { return m_iteration; }
        set { m_iteration = value; }
    }

    public void SetRhs(float value, SearchNode source)
    {
        m_rhs = value;
        m_rhsSource = source;
    }
    #endregion

    #region HAA*
    //到障碍或地图边缘的一种衡量方式
    //如果有多种地形（如陆地、湖水等），则改为使用HashSet<TerrainType, int>
    [SerializeField] private int m_trueClearance; 

    public int TrueClearance
    {
        get { return m_trueClearance; }
    }

    public void SetTrueClearance(int value)
    {
        m_trueClearance = value;

        m_valueText.gameObject.SetActive(true);
        m_valueText.text = value.ToString();
    }
    #endregion

    #region D*
    private float m_key;

    public float Key
    {
        get { return m_key; }
        set { m_key = value; }
    }

    public bool IsNew
    {
        get { return !m_opened && !m_closed; }
    }
    #endregion

    #region FD*
    private float m_fb;
    private float m_fx;
    private SearchNode m_r; //插入或更新开放列表时机器人的位置

    public float Fb
    {
        get { return m_fb; }
        set { m_fb = value; }
    }

    public float Fx
    {
        get { return m_fx; }
        set { m_fx = value; }
    }

    public SearchNode R
    {
        get { return m_r; }
        set { m_r = value; }
    }
    #endregion

    #region GFRA*
    private bool m_expanded;

    public bool Expanded
    {
        get { return m_expanded; }
        set { m_expanded = value; }
    }
    #endregion

    #region Path Adaptive A*
    private SearchNode m_nextState;
    private SearchNode m_backState;

    public SearchNode NextState
    {
        get { return m_nextState; }
        set { m_nextState = value; }
    }

    public SearchNode BackState
    {
        get { return m_backState; }
        set { m_backState = value; }
    }
    #endregion
}