using UnityEngine;

public class EntrancePoint
{
    public int AbstractId { get; private set; }
    public ConcreteNode ConcreteNode { get; private set; }

    public EntrancePoint(int abstractId, ConcreteNode concreteNode)
    {
        AbstractId = abstractId;
        ConcreteNode = concreteNode;
    }
}