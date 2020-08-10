using UnityEngine;

public class ConcreteNode : MonoBehaviour, INode
{
    public int Id { get; private set; }
    public int Cost { get; private set; }
    public Vector2Int Pos
    {
        get { return new Vector2Int((int)transform.position.x, (int)transform.position.y); }
        set { transform.position = new Vector3(value.x, value.y, 0); }
    }

    public bool IsObstacle { get { return Cost == Define.c_costObstacle; } }

    public void Init(int id, int x, int y, int cost = 1)
    {
        Id = id;
        Pos = new Vector2Int(x, y);
        Cost = cost;
    }
}