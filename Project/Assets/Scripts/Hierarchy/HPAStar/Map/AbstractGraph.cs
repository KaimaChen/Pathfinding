using System.Collections.Generic;
using UnityEngine;

public class AbstractGraph
{
    /// <summary>
    /// 索引值就是节点的id
    /// </summary>
    public List<AbstractNode> Nodes { get; set; }

    public AbstractGraph()
    {
        Nodes = new List<AbstractNode>();
    }

    public void AddNode(AbstractNode node)
    {
        int size = node.Id + 1;
        if (Nodes.Count >= size)
            Nodes[node.Id] = node;
        else
            Nodes.Add(node);
    }

    public bool RemoveLastNode(int abstractId)
    {
        //TODO：因为以index作为id，因此只能移除最后一个，想想其它快速的数据组织方式
        if (abstractId != Nodes.Count - 1)
        {
            Debug.LogError($"移除的不是最后一个({abstractId}, {Nodes.Count-1})，有地方出问题了");
            return false;
        }

        Nodes.RemoveAt(abstractId);
        return true;
    }

    public void AddEdge(int sourceId, int targetId, float cost, int level, bool isInterEdge)
    {
        var sourceNode = GetNode(sourceId);
        var targetNode = GetNode(targetId);
        var edge = HPADemo.Instance.CreateEdge(sourceNode.Pos, targetNode.Pos, level, isInterEdge);
        edge.Init(targetId, cost, level, isInterEdge);
        Nodes[sourceId].AddEdge(edge);
    }

    public void AddEdge(int srcId, AbstractEdge edge)
    {
        Nodes[srcId].AddEdge(edge);
    }

    public bool IsContainsEdge(int srcId, int targetId)
    {
        return Nodes[srcId].IsContainsEdge(targetId);
    }

    /// <summary>
    /// 移除所有起点或终点是该点的边
    /// </summary>
    public void RemoveEdgeFromAndToNode(int nodeId)
    {
        for(int i = 0; i < Nodes.Count; i++)
        {
            if(nodeId == i)
                Nodes[i].ClearEdges();
            else
                Nodes[i].RemoveEdge(nodeId);
        }
    }

    public AbstractNode GetNode(int id)
    {
        return Nodes[id];
    }
}