using System.Collections.Generic;

public class LazyThetaStar : ThetaStar
{
    public LazyThetaStar(SearchNode start, SearchNode goal, SearchNode[,] nodes, float weight, float showTime)
        : base(start, goal, nodes, weight, showTime)
    { }

    protected override void ComputeCost(SearchNode curtNode, SearchNode nextNode)
    {
        //起点没有Parent，所以特殊处理为自己
        //原Paper将起点的父节点设置为起点，但我不喜欢图里有个自环，所以这样处理
        var parent = curtNode == m_mapStart ? m_mapStart : curtNode.Parent;

        //Path 2
        //假设都通过了LOS检查
        float newG = parent.G + c(parent, nextNode);
        if (newG < nextNode.G)
            nextNode.SetParent(parent, newG);
    }

    protected override void SetVertex(SearchNode node)
    {
        //起点没有Parent
        //原Paper将起点的父节点设置为起点，但我不喜欢图里有个自环，所以这样处理
        if (node == m_mapStart)
            return;

        if(LineOfSign(node.Parent, node) == false)
        {
            //Path 1
            //实际并没有通过LOS检查，就找已关闭邻居中最小的
            node.SetParent(null, float.MaxValue);

            List<SearchNode> neighbors = GetNeighbors(node);
            for(int i = 0; i < neighbors.Count; i++)
            {
                SearchNode neighbor = neighbors[i];
                if(neighbor.Closed)
                {
                    float newG = neighbor.G + c(neighbor, node);
                    if (newG < node.G)
                        node.SetParent(neighbor, newG);
                }
            }
        }
    }
}
