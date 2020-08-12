using System.Collections.Generic;
using UnityEngine;

public static class HierarchicalSearch
{
    public static List<PathNode> Search(HierarchicalMapFactory factory, HierarchicalMap map, int maxLevel, Vector2Int startPos, Vector2Int goalPos, int maxRefineCount = int.MaxValue)
    {
        //首先插入起点和终点到图中
        var startAbsId = factory.InsertAbstractNode(map, startPos);
        var goalAbsId = factory.InsertAbstractNode(map, goalPos);

        var path =  SearchImpl(map, startAbsId, goalAbsId, maxLevel, maxRefineCount);

        //可以在这里应用路径平滑

        //搜索完毕后就移除起点和终点（如果常用，也可以留着）
        factory.RemoveAbstractNode(map, goalAbsId);
        factory.RemoveAbstractNode(map, startAbsId);

        return path;
    }

    private static List<PathNode> SearchImpl(HierarchicalMap map, int startAbsId, int goalAbsId, int maxSearchLevel, int maxRefineCount = int.MaxValue)
    {
        //找出最高层的抽象路径
        List<PathNode> path = GetPath(map, startAbsId, goalAbsId, maxSearchLevel, true);
        if (path.Count == 0)
            return path;

        //一层层具化路径直到最低层抽象
        for (int level = maxSearchLevel; level > 1; level--)
        {
            if (maxRefineCount <= 0)
                break;

            path = RefineAbstractPath(map, path, level, ref maxRefineCount);
        }

        //找到具体的路径
        path = AbstractPathToConcretePath(map, path, maxRefineCount);

        return path;
    }

    /// <summary>
    /// 获得指定层的路径
    /// </summary>
    private static List<PathNode> GetPath(HierarchicalMap map, int startAbsId, int goalAbsId, int level, bool isMainSearch)
    {
        map.SetCurrentLevelForSearch(level);

        AbstractNode startAbsNode = map.AbstractGraph.GetNode(startAbsId);
        AbstractNode goalAbsNode = map.AbstractGraph.GetNode(goalAbsId);

        Path path;
        if (isMainSearch)
        {
            map.SetAllMapAsCurrentCluster();
            var planner = new PathPlanner(map, null);
            path = planner.Search(startAbsNode, goalAbsNode);
        }
        else
        {
            map.SetCurrentClusterAndLevel(startAbsNode.Pos, level + 1); //因为每一层节点都保存的是下一层的路径，所以要通过上一层来找到当层的路径
            AbstractEdge edge = map.AbstractGraph.GetNode(startAbsNode.Id).Edges[goalAbsNode.Id];
            path = new Path(edge.InnerLowerLevelPath, edge.Cost);
        }

        if (path == null)
            return new List<PathNode>();

        var result = new List<PathNode>(path.Nodes.Count);
        foreach (var node in path.Nodes)
            result.Add(new PathNode(node.Id, node.Pos, level));
        return result;
    }

    private static List<PathNode> RefineAbstractPath(HierarchicalMap map, List<PathNode> path, int level, ref int maxRefineCount)
    {
        var result = new List<PathNode>();
        if (path.Count == 0)
            return result;

        int calculatedPaths = 0;
        result.Add(new PathNode(path[0].Id, path[0].Pos, path[0].Level - 1));
        for(int i = 1; i < path.Count; i++)
        {
            if(path[i].Level == level && path[i].Level == path[i - 1].Level && map.BelongToSameCluster(path[i].Pos, path[i-1].Pos, level) && calculatedPaths < maxRefineCount)
            {
                var interPath = GetPath(map, path[i - 1].Id, path[i].Id, level - 1, false);
                result.AddRange(interPath);
                calculatedPaths++;
            }
            else
            {
                result.Add(new PathNode(path[i].Id, path[i].Pos, path[i].Level - 1));
            }
        }

        maxRefineCount -= calculatedPaths;

        return result;
    }

    /// <summary>
    /// 计算出抽象路径对应的具体路径
    /// </summary>
    private static List<PathNode> AbstractPathToConcretePath(HierarchicalMap map, List<PathNode> abstractPath, int maxRefineCount)
    {
        var result = new List<PathNode>();
        if (abstractPath.Count == 0)
            return result;

        int calculatedPaths = 0;
        var lastAbstractNodeId = abstractPath[0].Id;

        if(abstractPath[0].Level != 1)
        {
            result.Add(abstractPath[0]);
        }
        else
        {
            var abstractNode = map.AbstractGraph.GetNode(lastAbstractNodeId);
            result.Add(new PathNode(abstractNode.Pos));
        }

        for(int curtIndex = 1; curtIndex < abstractPath.Count; curtIndex++)
        {
            var curtPathNode = abstractPath[curtIndex];
            var lastAbsNode = map.AbstractGraph.GetNode(lastAbstractNodeId);
            var curtAbsNode = map.AbstractGraph.GetNode(curtPathNode.Id);

            if (lastAbstractNodeId == curtAbsNode.Id)
                continue;

            //TODO 对于没有Refine的点，需要找个方式处理，直接加进去不好
            if(curtPathNode.Level != 1)
            {
                result.Add(curtPathNode);
                continue;
            }

            int curtClusterId = curtAbsNode.ClusterId;
            int lastClusterId = lastAbsNode.ClusterId;
            if(curtClusterId == lastClusterId && calculatedPaths < maxRefineCount) //查找Cluster内部的路径
            {
                var cluster = map.GetCluster(curtClusterId);
                var localPath = cluster.GetPath(lastAbsNode.Id, curtAbsNode.Id);

                for (int i = 1; i < localPath.Count; i++)
                    result.Add(new PathNode(localPath[i].Pos));

                calculatedPaths++;
            }
            else //查找Cluster之间的路径（直接相连，所以把两个节点加起来即可）
            {
                if (result[result.Count - 1].Pos != lastAbsNode.Pos)
                    result.Add(new PathNode(lastAbsNode.Pos));

                if (result[result.Count - 1].Pos != curtAbsNode.Pos)
                    result.Add(new PathNode(curtAbsNode.Pos));
            }

            lastAbstractNodeId = curtAbsNode.Id;
        }

        return result;
    }
}