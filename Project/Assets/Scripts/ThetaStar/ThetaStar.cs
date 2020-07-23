using UnityEngine;

public class ThetaStar : AStar
{
    public ThetaStar(SearchNode start, SearchNode goal, SearchNode[,] nodes, float weight, float showTime)
        : base(start, goal, nodes, weight, showTime)
    { }

    protected override void ComputeCost(SearchNode curtNode, SearchNode nextNode)
    {
        if(LineOfSign(curtNode.Parent, nextNode))
        {
            //Path 2
            float newG = curtNode.Parent.G + c(curtNode.Parent, nextNode);
            if (newG < nextNode.G)
                nextNode.SetParent(curtNode.Parent, newG);
        }
        else
        {
            //Path 1
            base.ComputeCost(curtNode, nextNode);
        }
    }

    protected bool LineOfSign(SearchNode startNode, SearchNode endNode)
    {
        if (startNode == null || endNode == null)
            return false;

        if (startNode == endNode)
            return true;

        Vector2Int start = startNode.Pos;
        Vector2Int end = endNode.Pos;
        int dx = end.x - start.x;
        int dy = end.y - start.y;
        int ux = dx > 0 ? 1 : -1;
        int uy = dy > 0 ? 1 : -1;
        int x = start.x;
        int y = start.y;
        int eps = 0;
        dx = Mathf.Abs(dx);
        dy = Mathf.Abs(dy);
        if(dx > dy)
        {
            for(x = start.x; x != end.x; x += ux)
            {
                if (GetNode(x, y).IsObstacle())
                    return false;

                eps += dy;
                if((eps << 1) >= dx)
                {
                    if(x != start.x) //处理斜线移动的可移动性判断
                    {
                        //如果附近两个都是障碍，那么不可以走
                        if (GetNode(x + ux, y).IsObstacle() && GetNode(x - ux, y + uy).IsObstacle())
                            return false;
                    }

                    y += uy;
                    eps -= dx;
                }
            }
        }
        else
        {
            for(y = start.y; y != end.y; y += uy)
            {
                if (GetNode(x, y).IsObstacle())
                    return false;

                eps += dx;
                if((eps << 1) >= dy)
                {
                    if(y != start.y)
                    {
                        if (GetNode(x, y + uy).IsObstacle() && GetNode(x + ux, y - uy).IsObstacle())
                            return false;
                    }

                    x += ux;
                    eps -= dy;
                }
            }
        }

        return true;
    }
}
