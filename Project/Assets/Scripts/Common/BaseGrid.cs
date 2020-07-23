using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 操作：
/// 左键点击：添加障碍
/// 右键点击：移除障碍
/// 空格键：开始寻路
/// </summary>
public abstract class BaseGrid<T> : MonoBehaviour where T : BaseNode
{
    public GameObject m_nodePrefab;
    
    protected int m_row = 8;
    protected int m_col = 15;

    protected T[,] m_nodes;

    protected virtual void Awake()
    {
        m_nodes = new T[m_row, m_col];
        for(int y = 0; y < m_row; y++)
        {
            for(int x = 0; x < m_col; x++)
            {
                GameObject go = GameObject.Instantiate(m_nodePrefab);
                go.name = $"node({x}, {y})";
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(x, y, 0);

                m_nodes[y, x] = go.GetComponent<T>();
                m_nodes[y, x].Init(x, y, Define.c_costRoad);
            }
        }
    }

    protected virtual void Update()
    {
        if (Input.GetMouseButton(0))
            AddObstacle();
        else if (Input.GetMouseButton(1))
            RemoveObstacle();
        else if (Input.GetKeyDown(KeyCode.Space))
            Generate();
    }

    protected abstract void Generate();

    protected virtual bool AddObstacle()
    {
        BaseNode node = GetMouseOverNode();
        if(node != null)
        {
            byte last = node.Cost;
            node.SetCost(Define.c_costObstacle);
            return last != node.Cost;
        }

        return false;
    }

    protected virtual bool RemoveObstacle()
    {
        BaseNode node = GetMouseOverNode();
        if(node != null)
        {
            byte last = node.Cost;
            node.SetCost(Define.c_costRoad);
            return last != node.Cost;
        }

        return false;
    }

    protected T GetMouseOverNode()
    {
        T result = null;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out RaycastHit hit))
            result = hit.collider.GetComponent<T>();

        return result;
    }

    protected List<BaseNode> GetNeighbors(BaseNode node, bool useDiagonal)
    {
        List<BaseNode> result = new List<BaseNode>();

        if (node == null)
            return result;

        int x = node.X;
        int y = node.Y;

        //left
        CheckAdd(x - 1, y, result);
        //right
        CheckAdd(x + 1, y, result);
        //Bottom
        CheckAdd(x, y - 1, result);
        //top
        CheckAdd(x, y + 1, result);

        //是否考虑对角线
        if(useDiagonal)
        {
            //Top Left
            CheckAdd(x - 1, y + 1, result);
            //Bottom Left
            CheckAdd(x - 1, y - 1, result);
            //Top Right
            CheckAdd(x + 1, y + 1, result);
            //Bottom Right
            CheckAdd(x + 1, y - 1, result);
        }

        return result;
    }

    void CheckAdd(int x, int y, List<BaseNode> list)
    {
        if (x >= 0 && x < m_col && y >= 0 && y < m_row)
            list.Add(GetNode(x, y));
    }

    protected virtual T GetNode(int x, int y)
    {
        return m_nodes[y, x];
    }
}