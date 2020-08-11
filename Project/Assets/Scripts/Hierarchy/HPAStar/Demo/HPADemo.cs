using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPADemo : MonoBehaviour
{
    public static HPADemo Instance;

    public Dropdown m_levelChooser;
    public GameObject m_concreteNodePrefab;
    public GameObject m_clusterPrefab;
    public GameObject m_abstractNodePrefab;
    public GameObject m_edgePrefab;

    /// <summary>
    /// 横向多少格子
    /// </summary>
    public int m_width = 20;
    /// <summary>
    /// 纵向多少格子
    /// </summary>
    public int m_height = 20;

    public int m_clusterSize = 5;

    /// <summary>
    /// 一共划分多少层
    /// </summary>
    public int m_maxLevel = 2;

    private ConcreteMap m_concreteMap;

    private readonly Dictionary<int, AbstractNode> m_levelToNodes = new Dictionary<int, AbstractNode>();

    private Transform m_concreteNodeContainer;
    private Transform m_clusterContainer;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        m_concreteNodeContainer = transform.Find("ConcreteNodeContainer");
        m_clusterContainer = transform.Find("ClusterContainer");

        m_levelChooser.onValueChanged.AddListener(OnChooseLevel);

        m_concreteMap = new ConcreteMap(m_width, m_height);
        int nodeId = 0;
        for(int y = 0; y < m_height; y++)
        {
            for(int x = 0; x < m_width; x++)
            {
                GameObject go = GameObject.Instantiate(m_concreteNodePrefab);
                go.name = $"ConcreteNode({x}, {y})";
                go.transform.SetParent(m_concreteNodeContainer);
                go.transform.localPosition = new Vector3(x, y, 0);

                ConcreteNode node = go.GetComponent<ConcreteNode>();
                node.Init(nodeId++, x, y);
                m_concreteMap.AddNode(x, y, node);
            }
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Reset();

            HierarchicalMapFactory factory = new HierarchicalMapFactory();
            HierarchicalMap hierarchicalMap = factory.CreateHierarchicalMap(m_concreteMap, m_clusterSize, m_maxLevel);
        }
    }

    private void Reset()
    {
        List<string> options = new List<string>();
        for (int i = 1; i <= m_maxLevel; i++)
            options.Add(i.ToString());
        options.Add("All");
        m_levelChooser.ClearOptions();
        m_levelChooser.AddOptions(options);

        for(int level = 1; level <= m_maxLevel; level++)
        {
            Transform container = transform.Find(level.ToString());
            if (container == null)
            {
                container = new GameObject(level.ToString()).transform;
                container.SetParent(transform);
            }
            container.gameObject.SetActive(level == m_levelChooser.value+1);
        }

        for (int i = m_clusterContainer.childCount - 1; i >= 0 ; i--)
            Destroy(m_clusterContainer.GetChild(i).gameObject);
    }

    public Cluster CreateCluster(Vector2Int pos)
    {
        GameObject go = GameObject.Instantiate(m_clusterPrefab, m_clusterContainer);
        go.name = $"Cluster({pos.x},{pos.y})";
        Cluster cluster = go.GetComponent<Cluster>();
        return cluster;
    }

    private Transform GetContainer(int level, string name)
    {
        Transform levelContainer = transform.Find(level.ToString());
        if(levelContainer == null)
        {
            levelContainer = new GameObject(level.ToString()).transform;
            levelContainer.SetParent(transform);
        }

        Transform container = levelContainer.Find(name);
        if(container == null)
        {
            container = new GameObject(name).transform;
            container.SetParent(levelContainer);
        }

        return container;
    }

    public AbstractNode CreateAbstractNode(int level, Vector2Int pos)
    {
        Transform container = GetContainer(level, "NodeContainer");
        GameObject go = GameObject.Instantiate(m_abstractNodePrefab, container);
        go.transform.localPosition = new Vector3(pos.x + 0.5f, pos.y + 0.5f, -0.2f);
        go.GetComponent<Renderer>().material.color = new Color(0, 0, 1, 0.5f);
        return go.GetComponent<AbstractNode>();
    }

    public AbstractEdge CreateEdge(Vector2Int start, Vector2Int target, int level, bool isInter)
    {
        Transform container = GetContainer(level, "EdgeContainer");
        GameObject go = GameObject.Instantiate(m_edgePrefab, container);
        go.transform.localPosition = new Vector3(0, 0, -2);
        go.GetComponent<Renderer>().material.color = isInter ? Color.black : Color.red;

        LineRenderer line = go.GetComponent<LineRenderer>();
        Vector3[] positions = new Vector3[2];
        positions[0] = new Vector3(start.x + 0.5f, start.y + 0.5f, 0);
        positions[1] = new Vector3(target.x + 0.5f, target.y + 0.5f, 0);
        line.SetPositions(positions);

        return go.GetComponent<AbstractEdge>();
    }

    private void ShowLevel(int level)
    {
        for(int i = 1; i <= m_maxLevel; i++)
        {
            Transform container = transform.Find(i.ToString());
            if (container != null)
                container.gameObject.SetActive(i == level || level == -1);
        }
    }

    private void OnChooseLevel(int index)
    {
        var option = m_levelChooser.options[index];
        if (option.text == "All")
            ShowLevel(-1);
        else
            ShowLevel(int.Parse(option.text));
    }
}