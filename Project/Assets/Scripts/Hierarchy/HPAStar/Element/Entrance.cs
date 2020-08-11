using UnityEngine;

public enum Orientation
{
    Horizontal,
    Vertical,
}

public class Entrance
{
    public Cluster Cluster1 { get; }
    public Cluster Cluster2 { get; }

    public ConcreteNode Node1 { get; }
    public ConcreteNode Node2 { get; }

    public Orientation Orientation { get; }

    public Entrance(Cluster cluster1, Cluster cluster2, ConcreteNode node1, ConcreteNode node2, Orientation orientation)
    {
        Debug.Log($"Create Entrance: {node1.Pos}, {node2.Pos}");

        Cluster1 = cluster1;
        Cluster2 = cluster2;
        Node1 = node1;
        Node2 = node2;
        Orientation = orientation;
    }

    /// <summary>
    /// 获取该Entrance所属的最大Level
    /// </summary>
    public int MaxBelongLevel(int clusterSize, int maxLevel)
    {
        switch(Orientation)
        {
            case Orientation.Horizontal:
                return CalcLevel(clusterSize, maxLevel, Node1.Pos.x);
            case Orientation.Vertical:
                return CalcLevel(clusterSize, maxLevel, Node1.Pos.y);
            default:
                Debug.LogErrorFormat("没有代码处理Orientation={0}", Orientation);
                return -1;
        }
    }

    private int CalcLevel(int clusterSize, int maxLevel, int value)
    {
        if (value <= clusterSize)
            return 1;

        int count = value / clusterSize;
        if (value % clusterSize != 0)
            count++;

        int level = 1;
        while (count % 2 == 0 && level < maxLevel) //这里默认上一级的clusterSize是下一级的2倍
        {
            count /= 2;
            level++;
        }

        return level;
    }
}