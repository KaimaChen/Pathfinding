using System.Collections.Generic;

public interface IMap
{
    int NodeCount();
    List<INode> GetSuccessors(INode source);
    float CalcHeuristic(INode a, INode b);
}