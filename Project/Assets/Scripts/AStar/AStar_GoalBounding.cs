using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar_GoalBounding : AStar
{
    public AStar_GoalBounding(SearchNode start, SearchNode end, SearchNode[,] nodes, float weight, float showTime)
        : base(start, end, nodes, weight, showTime)
    {

    }

    private void Preprocess()
    {

    }

    public override IEnumerator Process()
    {
        //TODO
        return base.Process();
    }
}
