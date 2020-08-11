using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HierarchicalMap : IMap
{
    public AbstractGraph AbstractGraph { get; }
    public int Width { get; }
    public int Height { get; }
    public int MinClusterSize { get; }
    public int MaxLevel { get; }

    private Cluster[] m_clusters;
    private readonly Dictionary<Vector2Int, AbstractNode> m_concreteToAbstractNode = new Dictionary<Vector2Int, AbstractNode>();

    private int m_curtLevel;
    private RectInt m_curtClusterArea;

    public HierarchicalMap(ConcreteMap concreteMap, int clusterSize, int maxLevel)
    {
        AbstractGraph = new AbstractGraph();
        Width = concreteMap.Width;
        Height = concreteMap.Height;
        MinClusterSize = clusterSize;
        MaxLevel = maxLevel;
    }

    public float CalcHeuristic(INode a, INode b)
    {
        return Heuristic.Octile(a.Pos, b.Pos);
    }

    public void InitClusters(List<Cluster> clusters)
    {
        m_clusters = new Cluster[clusters.Count];
        for (int i = 0; i < clusters.Count; i++)
            m_clusters[clusters[i].Id] = clusters[i];
    }

    public void AddAbstractNode(AbstractNode node)
    {
        m_concreteToAbstractNode[node.Pos] = node;
        AbstractGraph.AddNode(node);
    }

    public void RemoveAbstractNode(int abstractId)
    {
        //移除cluster里的点
        var absNode = AbstractGraph.GetNode(abstractId);
        var cluster = m_clusters[absNode.ClusterId];
        cluster.RemoveEntrancePoint(abstractId);

        //移除graph里的点
        AbstractGraph.RemoveEdgeFromAndToNode(abstractId);
        AbstractGraph.RemoveLastNode(abstractId);

        m_concreteToAbstractNode.Remove(absNode.Pos);
    }

    public AbstractNode GetAbstractNode(Vector2Int concretePos)
    {
        AbstractNode node;
        m_concreteToAbstractNode.TryGetValue(concretePos, out node);
        return node;
    }

    public Cluster FindClusterForPosition(Vector2Int concretePos)
    {
        foreach(var cluster in m_clusters)
        {
            if (cluster.IsContainsPoint(concretePos))
                return cluster;
        }

        return null;
    }

    public int NodeCount()
    {
        return AbstractGraph.Nodes.Count;
    }

    public void SetCurrentLevelForSearch(int level)
    {
        m_curtLevel = level;
    }

    public void SetCurrentClusterAndLevel(Vector2Int pos, int level)
    {
        int clusterSize = GetClusterSize(level);
        int minY = pos.y - (pos.y % clusterSize);
        int maxY = Mathf.Min(Height, minY + clusterSize);
        int height = maxY - minY;
        int minX = pos.x - (pos.x % clusterSize);
        int maxX = Mathf.Min(Width, minX + clusterSize);
        int width = maxX - minX;
        m_curtClusterArea = new RectInt(minX, minY, width, height);
    }

    public void SetAllMapAsCurrentCluster()
    {
        m_curtClusterArea = new RectInt(Vector2Int.zero, new Vector2Int(Width, Height));
    }

    /// <summary>
    /// 获得对应层级的Cluster大小
    /// </summary>
    private int GetClusterSize(int level)
    {
        return MinClusterSize * (1 << (level - 1)); //每提高一级，大小就变两倍
    }

    public bool BelongToSameCluster(Vector2Int a, Vector2Int b, int level)
    {
        int clusterSize = GetClusterSize(level);

        int aMinX = a.x - (a.x % clusterSize);
        int bMinX = b.x - (b.x % clusterSize);
        if (aMinX != bMinX)
            return false;

        int aMinY = a.y - (a.y % clusterSize);
        int bMinY = b.y - (b.y % clusterSize);
        if (aMinY != bMinY)
            return false;

        return true;
    }

    public Cluster GetCluster(int clusterId)
    {
        return m_clusters[clusterId];
    }

    public List<INode> GetSuccessors(INode source)
    {
        List<INode> result = new List<INode>();

        var node = AbstractGraph.GetNode(source.Id);
        var edges = node.Edges;
        foreach(var edge in edges.Values)
        {
            if (!IsValidEdgeForLevel(edge, m_curtLevel))
                continue;

            int targetAbsId = edge.TargetNodeId;
            var targetAbsNode = AbstractGraph.GetNode(targetAbsId);
            if (!IsInCurtCluster(targetAbsNode.Pos))
                continue;

            result.Add(targetAbsNode);
        }

        return result;
    }

    /// <summary>
    /// 判断边是否可用于某层级
    /// </summary>
    private static bool IsValidEdgeForLevel(AbstractEdge edge, int level)
    {
        if (edge.IsInterEdge)
            return edge.Level >= level;
        else
            return edge.Level == level;
    }

    private bool IsValidAbstractNodeForLevel(int abstractId, int level)
    {
        AbstractNode node = AbstractGraph.GetNode(abstractId);
        return (node != null && node.Level >= level);
    }

    /// <summary>
    /// 该位置是否在当前Cluster内
    /// </summary>
    public bool IsInCurtCluster(Vector2Int pos)
    {
        return m_curtClusterArea.Contains(pos);
    }

    private int GetEntrancePointLevel(EntrancePoint point)
    {
        return AbstractGraph.GetNode(point.AbstractId).Level;
    }

    /// <summary>
    /// 构建上层的边
    /// </summary>
    public void CreateHierarchicalEdges()
    {
        for(int level = 2; level <= MaxLevel; level++)
        {
            SetCurrentLevelForSearch(level - 1);

            int n = 1 << (level - 1);
            var clusterGroups = m_clusters.GroupBy(c => $"{c.Pos.x / n}_{c.Pos.y / n}"); //按等级划分

            foreach(var clusterGroup in clusterGroups)
            {
                var entrancesInClusterGroup = clusterGroup
                    .SelectMany(c => c.EntrancePoints)
                    .Where(entrance => GetEntrancePointLevel(entrance) >= level)
                    .ToList();

                var firstEntrance = entrancesInClusterGroup.FirstOrDefault();
                if (firstEntrance == null)
                    continue;

                var entrancePos = AbstractGraph.GetNode(firstEntrance.AbstractId).Pos;
                SetCurrentClusterAndLevel(entrancePos, level);

                foreach(var entrance1 in entrancesInClusterGroup)
                {
                    foreach (var entrance2 in entrancesInClusterGroup)
                    {
                        if (entrance1 == entrance2 || !IsValidAbstractNodeForLevel(entrance1.AbstractId, level) || !IsValidAbstractNodeForLevel(entrance2.AbstractId, level))
                            continue;

                        AddEdgesBetweenAbstractNodes(entrance1.AbstractId, entrance2.AbstractId, level);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 构建指定层级节点间的边
    /// </summary>
    private void AddEdgesBetweenAbstractNodes(int absId1, int absId2, int level)
    {
        var planner = new PathPlanner(this, null);
        var path = planner.Search(AbstractGraph.GetNode(absId1), AbstractGraph.GetNode(absId2));
        if(path != null && path.Nodes.Count > 0)
        {
            AbstractEdge edge = new AbstractEdge(absId2, path.Cost, level, false);
            edge.SetInnerLowerLevelPath(path.Nodes);
            AbstractGraph.AddEdge(absId1, edge);

            path.Nodes.Reverse();

            edge = new AbstractEdge(absId1, path.Cost, level, false);
            edge.SetInnerLowerLevelPath(path.Nodes);
            AbstractGraph.AddEdge(absId2, edge);
        }
    }

    /// <summary>
    /// 在指定层级的Cluster中，添加节点与其它EntrancePoint的边
    /// </summary>
    private void AddEdgesToOtherEntrancesInCluster(AbstractNode absNode, int level)
    {
        SetCurrentLevelForSearch(level - 1);
        SetCurrentClusterAndLevel(absNode.Pos, level);

        foreach(var cluster in m_clusters)
        {
            if(m_curtClusterArea.Contains(cluster.Area.position))
            {
                foreach(var entrance in cluster.EntrancePoints)
                {
                    if (absNode.Id == entrance.AbstractId || !IsValidAbstractNodeForLevel(entrance.AbstractId, level))
                        continue;

                    AddEdgesBetweenAbstractNodes(absNode.Id, entrance.AbstractId, level);
                }
            }
        }
    }

    /// <summary>
    /// 把指定节点涉及的边加到所有层级中
    /// </summary>
    public void AddHierarchicalEdgesForAbstractNode(int abstractId)
    {
        var absNode = AbstractGraph.GetNode(abstractId);
        int oldLevel = absNode.Level;
        absNode.Level = MaxLevel;
        for (int level = oldLevel + 1; level <= MaxLevel; level++)
            AddEdgesToOtherEntrancesInCluster(absNode, level);
    }
}