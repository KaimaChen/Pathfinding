using UnityEngine;
using System.Collections.Generic;
using System;

public class Cluster : MonoBehaviour
{
    [SerializeField] private int m_id;

    /// <summary>
    /// 在Cluster级别的位置
    /// </summary>
    [SerializeField] private Vector2Int m_pos;

    /// <summary>
    /// Cluster覆盖的grid范围
    /// </summary>
    [SerializeField] private RectInt m_area;

    private ConcreteMap m_concreteMap;

    private readonly Dictionary<Tuple<int, int>, float> m_distanceDict = new Dictionary<Tuple<int, int>, float>();
    private readonly Dictionary<Tuple<int, int>, List<INode>> m_pathDict = new Dictionary<Tuple<int, int>, List<INode>>();

    public List<EntrancePoint> EntrancePoints { get; } = new List<EntrancePoint>();

    public int Id { get { return m_id; } }
    public Vector2Int Pos { get { return m_pos; } }
    public RectInt Area { get { return m_area; } }

    public void Init(int id, Vector2Int pos, Vector2Int concretePos, Vector2Int size, ConcreteMap concreteMap)
    {
        m_id = id;
        m_pos = pos;
        m_concreteMap = concreteMap;
        m_area = new RectInt(concretePos, size);
        transform.localPosition = new Vector3(m_area.center.x, m_area.center.y, -0.1f);
        transform.localScale = new Vector3(size.x, size.y, 1);
    }

    public bool IsContainsPoint(Vector2Int concretePos)
    {
        return Area.Contains(concretePos);
    }

    public EntrancePoint AddEntrancePoint(int abstractId, ConcreteNode node)
    {
        EntrancePoint point = new EntrancePoint(abstractId, node);
        EntrancePoints.Add(point);
        return point;
    }

    public void RemoveEntrancePoint(int abstractId)
    {
        for(int i = 0; i < EntrancePoints.Count; i++)
        {
            if(EntrancePoints[i].AbstractId == abstractId)
            {
                EntrancePoints.RemoveAt(i);
                break;
            }
        }

        List<Tuple<int, int>> removeList = new List<Tuple<int, int>>();
        foreach(var key in m_distanceDict.Keys)
        {
            if (key.Item1 == abstractId || key.Item2 == abstractId)
                removeList.Add(key);
        }

        for(int i = 0; i< removeList.Count; i++)
        {
            var key = removeList[i];
            m_distanceDict.Remove(key);
            m_pathDict.Remove(key);
        }
    }

    private void CalcPathBetweenEntrances(EntrancePoint e1, EntrancePoint e2)
    {
        if (e1.AbstractId == e2.AbstractId)
            return;

        var tuple = Tuple.Create(e1.AbstractId, e2.AbstractId);
        var invTuple = Tuple.Create(e2.AbstractId, e1.AbstractId);

        if (m_distanceDict.ContainsKey(tuple))
            return;

        PathPlanner planner = new PathPlanner(m_concreteMap, Area);
        Path path = planner.Search(e1.ConcreteNode, e2.ConcreteNode);
        if(path != null)
        {
            m_distanceDict[tuple] = m_distanceDict[invTuple] = path.Cost;

            m_pathDict[tuple] = new List<INode>();
            m_pathDict[tuple].AddRange(path.Nodes);

            path.Nodes.Reverse();
            m_pathDict[invTuple] = new List<INode>();
            m_pathDict[invTuple].AddRange(path.Nodes);
        }
    }

    public void CalcIntraEdgesData()
    {
        foreach (var e1 in EntrancePoints)
            foreach (var e2 in EntrancePoints)
                CalcPathBetweenEntrances(e1, e2);
    }

    public void AddIntraEdgesData(EntrancePoint entrance)
    {
        foreach (var other in EntrancePoints)
            CalcPathBetweenEntrances(entrance, other);
    }

    public bool IsConnected(int abstractId1, int abstractId2)
    {
        return m_distanceDict.ContainsKey(Tuple.Create(abstractId1, abstractId2));
    }

    public float Distance(int abstractId1, int abstractId2)
    {
        return m_distanceDict[Tuple.Create(abstractId1, abstractId2)];
    }

    public List<INode> GetPath(int abstractId1, int abstractId2)
    {
        List<INode> result;
        m_pathDict.TryGetValue(Tuple.Create(abstractId1, abstractId2), out result);
        return result;
    }
}