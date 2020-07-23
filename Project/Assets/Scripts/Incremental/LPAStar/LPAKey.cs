using System;

public struct LPAKey : IComparable<LPAKey>
{
    public float Key1;
    public float Key2;

    public LPAKey(float key1, float key2)
    {
        Key1 = key1;
        Key2 = key2;
    }

    public static bool operator <(LPAKey lhs, LPAKey rhs)
    {
        return lhs.CompareTo(rhs) < 0;
    }

    public static bool operator >(LPAKey lhs, LPAKey rhs)
    {
        return !(lhs < rhs);
    }

    public int CompareTo(LPAKey other)
    {
        if (Key1 == other.Key1)
            return Key2.CompareTo(other.Key2);
        else
            return Key1.CompareTo(other.Key1);
    }
}