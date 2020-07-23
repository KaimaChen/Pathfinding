using UnityEngine;

public class BaseNode : MonoBehaviour
{
    protected int m_x;
    protected int m_y;
    protected byte m_cost;

    #region get_set
    public int X { get { return m_x; } }

    public int Y { get { return m_y; } }

    public Vector2Int Pos { get { return new Vector2Int(m_x, m_y); } }

    public byte Cost { get { return m_cost; } }
    #endregion

    protected virtual void Awake()
    {
        if (GetComponent<Collider>() == null)
            Debug.LogError("请在Node的预设上绑定Collider");
    }

    public virtual void Init(int x, int y, byte cost)
    {
        m_x = x;
        m_y = y;
        m_cost = cost;
    }

    public bool IsObstacle()
    {
        return m_cost == Define.c_costObstacle;
    }

    public virtual void SetCost(byte cost)
    {
        m_cost = cost;
    }
}