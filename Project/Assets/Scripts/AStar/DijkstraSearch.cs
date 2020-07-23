/// <summary>
/// Dijkstra's Algorithm
/// 只考虑最接近起点的
/// </summary>
public class DijkstraSearch : AStar
{
    public DijkstraSearch(SearchNode start, SearchNode goal, SearchNode[,] nodes, float weight, float showTime)
        : base(start, goal, nodes, 0, showTime)
    { }
}