public class BestFirstSearch : AStar
{
    public BestFirstSearch(SearchNode start, SearchNode goal, SearchNode[,] nodes, float weight, float showTime)
        : base(start, goal, nodes, 100000, showTime)
    { }
}