using UnityEngine;

public static class Define
{
	public const int c_costGround = 1;
	public const int c_costWater = 2;
	public const int c_costObstacle = 255;
	public static readonly float c_sqrt2 = Mathf.Sqrt(2);

	public static Color Cost2Color(int cost)
	{
		switch(cost)
		{
			case 1:
				return Color.white;
			case 2:
				return Color.blue;
			case 3:
				return Color.green;
			case c_costObstacle:
				return Color.black;
			default:
				return Color.white;
		}
	}

	public static Color SearchType2Color(SearchType type)
	{
		switch(type)
		{
			case SearchType.Start:
				return new Color(0.5f, 0, 0, 1);
			case SearchType.Goal:
				return new Color(1, 0, 0, 1);
			case SearchType.Open:
				return new Color(0, 0.5f, 0.5f, 1);
			case SearchType.Expanded:
				return Color.cyan;
			case SearchType.Path:
				return Color.yellow;
			case SearchType.CurtPos:
				return Color.blue;
			default:
				return Color.white;
		}
	}
}

public enum Dir
{
	None,

	TopLeft,
	Left,
	BottomLeft,
	Bottom,
	BottomRight,
	Right,
	TopRight,
	Top,
}

public enum SearchType
{
	None,
	Start,
	Goal,
	Open,
	Expanded,
	Path,
	CurtPos,
}

public enum SearchAlgo
{
	A_Star,
	BestFirstSearch,
	BreadthFirstSearch,
	DijkstraSearch,
	Theta_Star,
	LazyTheta_Star,
	JPS,
	JPSPlus,
	BiA_Star,
	AnnotatedA_Star,
	//Incremental
	D_Star,
	FocussedD_Star,
	IDA_Star,
	LPA_Star,
	DstarLite,
	Path_AA_Star,
	Tree_AA_Star,
	//Moving Target
	GAA_Star,
	GFRA_Star,
	MT_DstarLite,
}