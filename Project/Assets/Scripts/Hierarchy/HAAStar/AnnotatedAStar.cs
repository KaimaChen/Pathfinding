public class AnnotatedAStar : AStar
{
    private readonly int m_unitSize;

    public AnnotatedAStar(SearchNode start, SearchNode goal, SearchNode[,] nodes, float weight, float showTime, int unitSize)
        : base(start, goal, nodes, weight, showTime)
    {
        m_unitSize = unitSize;
    }

    protected override bool IsNeighborValid(SearchNode neighbor)
    {
        return (neighbor.Closed == false && neighbor.TrueClearance >= m_unitSize);
    }

    #region Preprocess
    /// <summary>
    /// 离线预处理：给每个可行走格子设置Clearance
    /// 如果有多种地形，则每种地形及组合都要单独遍历地图处理一遍
    /// </summary>
    protected override void Preprocess()
    {
        ForeachNode((node) =>
        {
            if (node.IsObstacle() == false)
                node.SetTrueClearance(CalcTrueClearance(node));
        });
    }

    private int CalcTrueClearance(SearchNode node)
    {
        int x = node.X;
        int y = node.Y;

        int curtExpand = 2;
        //不断往右下扩展，直到发现不可走的点
        while (true)
        {
            for(int dx = 0; dx < curtExpand; dx++)
            {
                for(int dy = 0; dy < curtExpand; dy++)
                {
                    SearchNode next = GetNode(x + dx, y - dy);
                    if (next == null || next.IsObstacle())
                        return curtExpand - 1;
                }
            }

            curtExpand++;
        }
    }
    #endregion
}