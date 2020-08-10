using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class HierarchicalMapFactory
{
    /// <summary>
    /// Entrance小于该值则取中间，否则两边各取一个
    /// </summary>
    public const int c_maxEntranceSize = 6;

    private ConcreteMap m_concreteMap;
    private HierarchicalMap m_hierarchicalMap;
    private int m_clusterSize;
    private int m_maxLevel;

    private readonly Dictionary<int, NodeBackup> m_backupDict = new Dictionary<int, NodeBackup>();
    
    public HierarchicalMap CreateHierarchicalMap(ConcreteMap concreteMap, int clusterSize, int maxLevel)
    {
        m_concreteMap = concreteMap;
        m_clusterSize = clusterSize;
        m_maxLevel = maxLevel;
        m_hierarchicalMap = new HierarchicalMap(concreteMap, clusterSize, maxLevel);

        List<Cluster> clusters = new List<Cluster>();
        List<Entrance> entrances = new List<Entrance>();
        CreateClustersAndEntrances(clusters, entrances);
        m_hierarchicalMap.InitClusters(clusters);

        CreateAbstractNodes(entrances);
        CreateEdges(clusters, entrances);
        
        return m_hierarchicalMap;
    }

    #region Cluster
    private void CreateClustersAndEntrances(List<Cluster> clusterResult, List<Entrance> entranceResult)
    {
        int clusterId = 0;
        for(int y = 0, clusterY = 0; y < m_concreteMap.Height; y+=m_clusterSize, clusterY++)
        {
            for(int x = 0, clusterX = 0; x < m_concreteMap.Width; x+=m_clusterSize, clusterX++)
            {
                //创建Cluster
                int width = Mathf.Min(m_clusterSize, m_concreteMap.Width - x);
                int height = Mathf.Min(m_clusterSize, m_concreteMap.Height - y);
                Cluster cluster = new Cluster(clusterId++, new Vector2Int(clusterX, clusterY), new Vector2Int(x, y), new Vector2Int(width, height), m_concreteMap);
                clusterResult.Add(cluster);
                //创建Entrance
                Cluster leftCluster = clusterX > 0 ? GetCluster(clusterResult, clusterX - 1, clusterY) : null;
                Cluster underCluster = clusterY > 0 ? GetCluster(clusterResult, clusterX, clusterY - 1) : null;
                CreateEntrances(cluster, leftCluster, underCluster, entranceResult);
            }
        }
    }

    private Cluster GetCluster(List<Cluster> clusters, int x, int y)
    {
        int xCount = m_concreteMap.Width / m_clusterSize;
        if (m_concreteMap.Width % m_clusterSize > 0)
            xCount++;

        return clusters[y * xCount + x];
    }
    #endregion

    #region Entrance
    private void CreateEntrances(Cluster cluster, Cluster leftCluster, Cluster underCluster, List<Entrance> result)
    {
        List<ConcreteNode> prevNodes = new List<ConcreteNode>();
        List<ConcreteNode> curtNodes = new List<ConcreteNode>();
        
        if(leftCluster != null)
        {
            Vector2Int btPos = cluster.Area.position;
            int leftX = btPos.x - 1;
            int rightX = btPos.x;
            int minY = btPos.y;
            int maxY = btPos.y + cluster.Area.height;
            for(int y = minY; y < maxY; y++)
            {
                prevNodes.Add(m_concreteMap.Get(leftX, y));
                curtNodes.Add(m_concreteMap.Get(rightX, y));
            }

            CreateEntranceImpl(prevNodes, curtNodes, leftCluster, cluster, Orientation.Horizontal, result);
        }

        if(underCluster != null)
        {
            prevNodes.Clear();
            curtNodes.Clear();

            Vector2Int btPos = cluster.Area.position;
            int minX = btPos.x;
            int maxX = minX + cluster.Area.width;
            int underY = btPos.y - 1;
            int aboveY = btPos.y;
            for(int x = minX; x < maxX; x++)
            {
                prevNodes.Add(m_concreteMap.Get(x, underY));
                curtNodes.Add(m_concreteMap.Get(x, aboveY));
            }

            CreateEntranceImpl(prevNodes, curtNodes, underCluster, cluster, Orientation.Vertical, result);
        }
    }

    private void CreateEntranceImpl(List<ConcreteNode> prevNodes, List<ConcreteNode> curtNodes, Cluster prevCluster, Cluster curtCluster, Orientation orientation, List<Entrance> result)
    {
        int size = 0;
        ConcreteNode prevBegin = null, curtBegin = null;
        for(int i = 0; i < prevNodes.Count; i++)
        {
            ConcreteNode prev = prevNodes[i];
            ConcreteNode curt = curtNodes[i];

            if(prevBegin == null)
            {
                prevBegin = prev;
                curtBegin = curt;
            }

            if((prev.IsObstacle || curt.IsObstacle) && size > 0)
            {
                if(size >= c_maxEntranceSize)
                {
                    //取两边
                    result.Add(new Entrance(prevCluster, curtCluster, prevBegin, curtBegin, orientation));
                    result.Add(new Entrance(prevCluster, curtCluster, prev, curt, orientation));
                }
                else
                {
                    //取中间
                    int centerX = (prevBegin.Pos.x + prev.Pos.x) / 2;
                    int centerY = (prevBegin.Pos.y + prev.Pos.y) / 2;
                    ConcreteNode prevCenter = m_concreteMap.Get(centerX, centerY);

                    centerX = (curtBegin.Pos.x + curt.Pos.x) / 2;
                    centerY = (curtBegin.Pos.y + curt.Pos.y) / 2;
                    ConcreteNode curtCenter = m_concreteMap.Get(centerX, centerY);

                    result.Add(new Entrance(prevCluster, curtCluster, prevCenter, curtCenter, orientation));
                }

                //清理
                prevBegin = curtBegin = null;
                size = 0;
            }
            else
            {
                size++;
            }
        }
    }
    #endregion

    #region AbstractNode
    private void CreateAbstractNodes(List<Entrance> entrances)
    {
        int abstractId = 1;
        Dictionary<Vector2Int, AbstractNode> abstractDict = new Dictionary<Vector2Int, AbstractNode>();
        for(int i = 0; i < entrances.Count; i++)
        {
            Entrance e = entrances[i];
            int level = e.MaxBelongLevel(m_clusterSize, m_maxLevel);
            CreateOrUpdateAbstractNode(e.Node1, e.Cluster1, level, abstractDict, ref abstractId);
            CreateOrUpdateAbstractNode(e.Node2, e.Cluster2, level, abstractDict, ref abstractId);
        }

        foreach(var node in abstractDict.Values)
            m_hierarchicalMap.AddAbstractNode(node);
    }

    private static void CreateOrUpdateAbstractNode(ConcreteNode node, Cluster cluster, int level, Dictionary<Vector2Int, AbstractNode> abstractDict, ref int abstractId)
    {
        AbstractNode abstractNode;
        if(abstractDict.TryGetValue(node.Pos, out abstractNode))
        {
            abstractNode.Level = level;
        }
        else
        {
            cluster.AddEntrancePoint(abstractId, node);
            abstractDict[node.Pos] = new AbstractNode(abstractId, level, cluster.Id, node.Pos);
            abstractId++;
        }
    }
    #endregion

    #region Edge
    private void CreateEdges(List<Cluster> clusters, List<Entrance> entrances)
    {
        for (int i = 0; i < entrances.Count; i++)
            CreateInterEdges(entrances[i]);

        for(int i = 0; i < clusters.Count; i++)
        {
            clusters[i].CalcIntraEdgesData();
            CreateIntraEdges(clusters[i]);
        }

        m_hierarchicalMap.CreateHierarchicalEdges();
    }

    private void CreateInterEdges(Entrance entrance)
    {
        int level = entrance.MaxBelongLevel(m_clusterSize, m_maxLevel);
        var abstNode1 = m_hierarchicalMap.GetAbstractNode(entrance.Node1.Pos);
        var abstNode2 = m_hierarchicalMap.GetAbstractNode(entrance.Node2.Pos);

        m_hierarchicalMap.AbstractGraph.AddEdge(abstNode1.Id, abstNode2.Id, 1, level, true);
        m_hierarchicalMap.AbstractGraph.AddEdge(abstNode2.Id, abstNode1.Id, 1, level, true);
    }

    private void CreateIntraEdges(Cluster cluster)
    {
        foreach(var point1 in cluster.EntrancePoints)
        {
            foreach(var point2 in cluster.EntrancePoints)
            {
                if (point1 == point2 || !cluster.IsConnected(point1.AbstractId, point2.AbstractId))
                    continue;

                float distance = cluster.Distance(point1.AbstractId, point2.AbstractId);
                m_hierarchicalMap.AbstractGraph.AddEdge(point1.AbstractId, point2.AbstractId, distance, 1, false);
            }
        }
    }
    #endregion

    #region Search Update
    class NodeBackup
    {
        public int Level { get; }
        public List<AbstractEdge> Edges { get; }

        public NodeBackup(int level, List<AbstractEdge> edges)
        {
            Level = level;
            Edges = edges;
        }
    }

    public int InsertAbstractNode(HierarchicalMap map, Vector2Int concretePos)
    {
        int abstractId = InsertAbstractNodeInClusterLevel(map, concretePos);
        map.AddHierarchicalEdgesForAbstractNode(abstractId);
        return abstractId;
    }

    /// <summary>
    /// 插入节点到Cluster内，并构建相关节点和边到图中
    /// </summary>
    private int InsertAbstractNodeInClusterLevel(HierarchicalMap map, Vector2Int concretePos)
    {
        AbstractNode abstractNode = map.GetAbstractNode(concretePos);
        //如果要插入的位置已经存在节点，则保存相关信息，以便之后删除时恢复
        if(abstractNode != null)
        {
            NodeBackup backup = new NodeBackup(abstractNode.Level, abstractNode.Edges.Values.ToList());

            if (m_backupDict.ContainsKey(abstractNode.Id))
                Debug.LogError("已经存在一个NodeBackup，逻辑上出错了");

            m_backupDict[abstractNode.Id] = backup;
            return abstractNode.Id;
        }

        //把节点插入到Cluster内
        int abstractId = map.NodeCount();
        var cluster = map.FindClusterForPosition(concretePos);
        var entrance = cluster.AddEntrancePoint(abstractId, m_concreteMap.Get(concretePos));
        cluster.AddIntraEdgesData(entrance);
        //把节点插入到图中
        abstractNode = new AbstractNode(abstractId, 1, cluster.Id, concretePos);
        map.AddAbstractNode(abstractNode);
        map.AbstractGraph.AddNode(abstractNode);
        //把该节点相关的边插入到图中
        foreach(var otherEntrance in cluster.EntrancePoints)
        {
            if(cluster.IsConnected(abstractId, otherEntrance.AbstractId))
            {
                float distance = cluster.Distance(abstractId, otherEntrance.AbstractId);
                AbstractEdge edge = new AbstractEdge(otherEntrance.AbstractId, distance, 1, false);
                map.AbstractGraph.AddEdge(abstractId, edge);

                edge = new AbstractEdge(abstractId, distance, 1, false);
                map.AbstractGraph.AddEdge(otherEntrance.AbstractId, edge);
            }
        }

        return abstractId;
    }

    /// <summary>
    /// 恢复寻路新增节点导致的变化
    /// </summary>
    private void RestoreNodeBackup(HierarchicalMap map, int nodeId, NodeBackup backup)
    {
        AbstractGraph graph = map.AbstractGraph;
        AbstractNode node = graph.GetNode(nodeId);

        //恢复节点的级别
        node.Level = backup.Level; 

        //恢复相关的边
        graph.RemoveEdgeFromAndToNode(nodeId);
        foreach (var edge in backup.Edges)
        {
            int targetNodeId = edge.TargetNodeId;

            AbstractEdge abstractEdge = new AbstractEdge(targetNodeId, edge.Cost, edge.Level, edge.IsInterEdge);
            abstractEdge.SetInnerLowerLevelPath(edge.InnerLowerLevelPath);
            graph.AddEdge(nodeId, abstractEdge);

            edge.InnerLowerLevelPath?.Reverse();

            abstractEdge = new AbstractEdge(nodeId, edge.Cost, edge.Level, edge.IsInterEdge);
            abstractEdge.SetInnerLowerLevelPath(edge.InnerLowerLevelPath);
            graph.AddEdge(targetNodeId, abstractEdge);
        }

        m_backupDict.Remove(nodeId);
    }

    /// <summary>
    /// 移除寻路时新增的抽象节点
    /// </summary>
    public void RemoveAbstractNode(HierarchicalMap map, int nodeId)
    {
        NodeBackup backup;
        if (m_backupDict.TryGetValue(nodeId, out backup)) 
            RestoreNodeBackup(map, nodeId, backup); //如果寻路时新增的节点在图中原本就存在，则重置相关数据
        else
            map.RemoveAbstractNode(nodeId); //如果的确新增了节点，则直接移除该节点
    }
    #endregion
}