using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPADemo : MonoBehaviour
{
    public GameObject m_concreteNodePrefab;
    public GameObject m_clusterPrefab;

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

    private Transform m_concreteNodeContainer;

    void Start()
    {
        m_concreteNodeContainer = transform.Find("ConcreteNodeContainer");

        ConcreteMap concreteMap = new ConcreteMap(m_width, m_height);
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
                concreteMap.AddNode(x, y, node);
            }
        }
    }

    void Update()
    {
        
    }
}
