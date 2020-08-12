using UnityEngine;

public class ConcreteNode : MonoBehaviour, INode
{
    [SerializeField] private int m_id;
    [SerializeField] private int m_cost;
    [SerializeField] private Vector2Int m_pos;
    private SearchType m_searchType;

    public int Id { get { return m_id; } }
    public int Cost { get { return m_cost; } }
    public Vector2Int Pos { get { return m_pos; } }
    public SearchType SearchType { get { return m_searchType; } }
    public bool IsObstacle { get { return Cost == Define.c_costObstacle; } }

    public void Init(int id, int x, int y, int cost = 1)
    {
        m_id = id;
        SetCost(cost);
        SetPos(x, y);
    }

    private void SetPos(int x, int y)
    {
        m_pos = new Vector2Int(x, y);
        transform.position = new Vector3(x + 0.5f, y + 0.5f, 0);
    }

    public void SetCost(int cost)
    {
        m_cost = cost;
        GetComponent<Renderer>().material.color = Define.Cost2Color(cost);
    }

    public void SetSearchType(SearchType type, bool excludeStartEnd = true)
    {
        if (excludeStartEnd && (m_searchType == SearchType.Start || m_searchType == SearchType.Goal))
            return;

        m_searchType = type;
        GetComponent<Renderer>().material.color = Define.SearchType2Color(type);
    }
}