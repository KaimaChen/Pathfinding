using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// JPS+
/// </summary>
public class JPSPlus : JumpPointSearch
{
    private const int c_east = 0;
    private const int c_west = 1;
    private const int c_north = 2;
    private const int c_south = 3;
    private const int c_northEast = 4;
    private const int c_northWest = 5;
    private const int c_southEast = 6;
    private const int c_southWest = 7;
    private const int c_dirCount = 8;

    private readonly int[,,] m_distanceData;

    public JPSPlus(SearchNode start, SearchNode goal, SearchNode[,] nodes, float weight, float showTime)
        : base(start, goal, nodes, weight, showTime)
    {
        m_distanceData = new int[nodes.GetLength(0), nodes.GetLength(1), c_dirCount];
    }

    public override IEnumerator Process()
    {
        //预处理是离线了，放到这里只是为了方便看结果
        OfflinePreprocess();

        //运行时
        int[][] dirLookUpTable = new int[c_dirCount][];
        dirLookUpTable[c_east] = new int[3] { c_east, c_northEast, c_southEast };
        dirLookUpTable[c_west] = new int[3] { c_west, c_northWest, c_southWest };
        dirLookUpTable[c_north] = new int[3] { c_north, c_northEast, c_northWest };
        dirLookUpTable[c_south] = new int[3] { c_south, c_southEast, c_southWest };
        dirLookUpTable[c_northEast] = new int[3] { c_northEast, c_north, c_east };
        dirLookUpTable[c_northWest] = new int[3] { c_northWest, c_north, c_west };
        dirLookUpTable[c_southEast] = new int[3] { c_southEast, c_south, c_east };
        dirLookUpTable[c_southWest] = new int[3] { c_southWest, c_south, c_west };

        m_mapStart.G = 0;
        AddToOpenList(m_mapStart);
        while(OpenListSize() > 0)
        {
            Vector2Int curtPos = PopOpenList();
            SearchNode curtNode = GetNode(curtPos);
            curtNode.Closed = true;

            if(curtPos == m_mapGoal.Pos)
                break;

            #region 展示
            Debug.Log("Open List Count = " + OpenListSize());
            yield return new WaitForSeconds(m_showTime); //等待一点时间，以便观察
            curtNode.SetSearchType(SearchType.Expanded, true);
            #endregion

            SearchNode parent = curtNode.Parent;
            if(parent != null)
            {
                int[] dirArr = dirLookUpTable[CalcDir(parent, curtNode)];
                for(int i = 0; i < dirArr.Length; i++)
                {
                    int dir = dirArr[i];
                    SearchNode newSuccessor = null;
                    float givenCost = float.MaxValue;

                    if(IsCardinal(dir) && IsTargetInExactDir(curtNode, dir) &&
                        DiffNodes(curtNode, m_mapGoal) <= Mathf.Abs(Distance(curtNode, dir)))
                    {
                        //目标比障碍或跳点更近
                        newSuccessor = m_mapGoal;
                        givenCost = curtNode.G + DiffNodes(curtNode, m_mapGoal);
                    }
                    else if(IsDiagonal(dir) && IsTargetInGeneralDir(curtNode, dir) &&
                                (DiffNodesRow(curtNode, m_mapGoal) <= Mathf.Abs(Distance(curtNode, dir)) ||
                                DiffNodesCol(curtNode, m_mapGoal) <= Mathf.Abs(Distance(curtNode, dir))))
                    {
                        //目标在水平或竖直方向上比障碍或跳点更近
                        int minDiff = Mathf.Min(RowDiff(curtNode, m_mapGoal), ColDiff(curtNode, m_mapGoal));
                        newSuccessor = GetNodeInDir(curtNode, minDiff, dir);
                        givenCost = curtNode.G + (Define.c_sqrt2 * minDiff);
                    }
                    else if(Distance(curtNode, dir) > 0)
                    {
                        newSuccessor = GetNodeInDir(curtNode, Distance(curtNode, dir), dir);
                        givenCost = DiffNodes(curtNode, newSuccessor);
                        if (IsDiagonal(dir))
                            givenCost *= Define.c_sqrt2;
                        givenCost += curtNode.G;
                    }

                    if(newSuccessor != null)
                    {
                        if(!newSuccessor.Opened && !newSuccessor.Closed)
                        {
                            newSuccessor.SetParent(curtNode, givenCost);
                            AddToOpenList(newSuccessor);
                        }
                        else if(givenCost < newSuccessor.G)
                        {
                            newSuccessor.SetParent(curtNode, givenCost);
                        }
                    }
                }
            }
            else
            {
                List<SearchNode> neighbors = GetNeighbors(curtNode);
                for(int i = 0; i < neighbors.Count; i++)
                {
                    var neighbor = neighbors[i];
                    if (neighbor.Closed == false)
                        UpdateVertex(curtNode, neighbor);
                }
            }
        }

        GeneratePath();

        yield break;
    }

    private int CalcDir(SearchNode prev, SearchNode curt)
    {
        int dx = curt.X - prev.X;
        int dy = curt.Y - prev.Y;
        if(dy == 0)
        {
            return dx > 0 ? c_east : c_west;
        }
        else if(dx == 0)
        {
            return dy > 0 ? c_north : c_south;
        }
        else
        {
            if (dx > 0 && dy > 0)
                return c_northEast;
            else if (dx < 0 && dy > 0)
                return c_northWest;
            else if (dx > 0 && dy < 0)
                return c_southEast;
            else
                return c_southWest;
        }
    }

    private bool IsCardinal(int dir)
    {
        return dir == c_east || dir == c_west || dir == c_south || dir == c_north;
    }

    private bool IsDiagonal(int dir)
    {
        return dir == c_northEast || dir == c_northWest || dir == c_southEast || dir == c_southWest;
    }

    private bool IsTargetInExactDir(SearchNode curtNode, int dir)
    {
        int targetDir = CalcDir(curtNode, m_mapGoal);
        return targetDir == dir;
    }

    private bool IsTargetInGeneralDir(SearchNode curtNode, int dir)
    {
        int dx = m_mapGoal.X - curtNode.X;
        int dy = m_mapGoal.Y - curtNode.Y;

        if (dir == c_northEast)
            return (dx >= 0 && dy >= 0);
        else if (dir == c_northWest)
            return (dx <= 0 && dy >= 0);
        else if (dir == c_southEast)
            return (dx >= 0 && dy <= 0);
        else if (dir == c_southWest)
            return (dx <= 0 && dy <= 0);
        else
            return false;
    }

    private int DiffNodes(SearchNode n1, SearchNode n2)
    {
        int dx = Mathf.Abs(n1.X - n2.X);
        int dy = Mathf.Abs(n1.Y - n2.Y);
        return Mathf.Max(dx, dy);
    }

    private int DiffNodesRow(SearchNode n1, SearchNode n2)
    {
        return Mathf.Abs(n1.Y - n2.Y);
    }

    private int DiffNodesCol(SearchNode n1, SearchNode n2)
    {
        return Mathf.Abs(n1.X - n2.X);
    }

    private int RowDiff(SearchNode prev, SearchNode curt)
    {
        return curt.Y - prev.Y;
    }

    private int ColDiff(SearchNode prev, SearchNode curt)
    {
        return curt.X - prev.X;
    }

    private SearchNode GetNodeInDir(SearchNode node, int distance, int dir)
    {
        int x = node.X;
        int y = node.Y;

        if(dir == c_east)
        {
            x += distance;
        }
        else if(dir == c_west)
        {
            x -= distance;
        }
        else if(dir == c_north)
        {
            y += distance;
        }
        else if(dir == c_south)
        {
            y -= distance;
        }
        else if(dir == c_northEast)
        {
            x += distance;
            y += distance;
        }
        else if(dir == c_northWest)
        {
            x -= distance;
            y += distance;
        }
        else if(dir == c_southEast)
        {
            x += distance;
            y -= distance;
        }
        else if(dir == c_southWest)
        {
            x -= distance;
            y -= distance;
        }

        return GetNode(x, y);
    }

    private int Distance(SearchNode node, int dir)
    {
        return m_distanceData[node.Y, node.X, dir];
    }

    #region 预处理
    /// <summary>
    /// 离线进行的地图预处理操作
    /// </summary>
    private void OfflinePreprocess()
    {
        ClearDistanceData();

        //找到Primary Jump Points
        bool[,,] isJumpPoints = FindPrimaryJumpPoints();

        //处理Straight Jump Points
        InitEastStraightJumpPoints(isJumpPoints);
        InitWestStraightJumpPoints(isJumpPoints);
        InitNorthStraightJumpPoints(isJumpPoints);
        InitSouthStraightJumpPoints(isJumpPoints);

        //处理Diagonal Jump Points
        InitSouthWestJumpPoints();
        InitSouthEastJumpPoints();
        InitNorthWestJumpPoints();
        InitNorthEastJumpPoints();

        #region 展示部分
        for (int y = 0; y < m_distanceData.GetLength(0); y++)
            for (int x = 0; x < m_distanceData.GetLength(1); x++)
                m_nodes[y, x].ShowDistance(m_distanceData[y, x, c_east], m_distanceData[y, x, c_west], 
                                                                m_distanceData[y, x, c_north], m_distanceData[y, x, c_south],
                                                                m_distanceData[y, x, c_northEast], m_distanceData[y, x, c_northWest],
                                                                m_distanceData[y, x, c_southEast], m_distanceData[y, x, c_southWest]);
        #endregion
    }

    private void ClearDistanceData()
    {
        for(int r = 0; r < m_distanceData.GetLength(0); r++)
            for(int c = 0; c < m_distanceData.GetLength(1); c++)
                for(int d = 0; d < c_dirCount; d++)
                    m_distanceData[r, c, d] = 0;
    }

    private bool[,,] FindPrimaryJumpPoints()
    {
        bool[,,] isJumpPoints = new bool[m_mapHeight, m_mapWidth, c_dirCount];

        for(int y = 0; y < m_mapHeight; y++)
        {
            //朝东
            for (int x = 0; x < m_mapWidth; x++)
                isJumpPoints[y, x, c_east] = CheckHorJumpPoint(x, y, 1);

            //朝西
            for (int x = m_mapWidth - 1; x >= 0; x--)
                isJumpPoints[y, x, c_west] = CheckHorJumpPoint(x, y, -1);
        }

        for(int x = 0; x < m_mapWidth; x++)
        {
            //朝北
            for (int y = 0; y < m_mapHeight; y++)
                isJumpPoints[y, x, c_north] = CheckVerJumpPoints(x, y, 1);

            //朝南
            for (int y = m_mapHeight - 1; y >= 0; y--)
                isJumpPoints[y, x, c_south] = CheckVerJumpPoints(x, y, -1);
        }

        return isJumpPoints;
    }

    private void InitHorStraightJumpPoints(bool[,,] isJumpPoints, bool isWest)
    {
        int rowCount = m_distanceData.GetLength(0);
        int colCount = m_distanceData.GetLength(1);

        int dir = isWest ? c_west : c_east;
        int startX = isWest ? 0 : colCount - 1;
        int dx = isWest ? 1 : -1;
        bool judge(int x) { return isWest ? (x < colCount) : (x >= 0); }

        for (int y = 0; y < rowCount; y++)
        {
            int count = -1;
            bool isJumpPointLastSeen = false;

            for (int x = startX; judge(x); x += dx)
            {
                if (IsWalkableAt(x, y) == false)
                {
                    count = -1;
                    isJumpPointLastSeen = false;
                    m_distanceData[y, x, dir] = 0;
                    continue;
                }

                count++;

                if (isJumpPointLastSeen)
                    m_distanceData[y, x, dir] = count;
                else
                    m_distanceData[y, x, dir] = -count;

                if (isJumpPoints[y, x, dir])
                {
                    count = 0;
                    isJumpPointLastSeen = true;
                }
            }
        }
    }

    private void InitVerStraightJumpPoints(bool[,,] isJumpPoints, bool isSouth)
    {
        int rowCount = m_distanceData.GetLength(0);
        int colCount = m_distanceData.GetLength(1);

        int dir = isSouth ? c_south : c_north;
        int startY = isSouth ? 0 : rowCount - 1;
        int dy = isSouth ? 1 : -1;
        bool judge(int y) { return isSouth ? (y < rowCount) : (y >= 0); }

        for(int x = 0; x < colCount; x++)
        {
            int count = -1;
            bool isJumpPointLastSeen = false;

            for(int y = startY; judge(y); y += dy)
            {
                if(IsWalkableAt(x, y) == false)
                {
                    count = -1;
                    isJumpPointLastSeen = false;
                    m_distanceData[y, x, dir] = 0;
                    continue;
                }

                count++;

                if (isJumpPointLastSeen)
                    m_distanceData[y, x, dir] = count;
                else
                    m_distanceData[y, x, dir] = -count;

                if(isJumpPoints[y, x, dir])
                {
                    count = 0;
                    isJumpPointLastSeen = true;
                }
            }
        }
    }

    private void InitEastStraightJumpPoints(bool[,,] isJumpPoints)
    {
        InitHorStraightJumpPoints(isJumpPoints, false);
    }

    private void InitWestStraightJumpPoints(bool[,,] isJumpPoints)
    {
        InitHorStraightJumpPoints(isJumpPoints, true);
    }

    private void InitNorthStraightJumpPoints(bool[,,] isJumpPoints)
    {
        InitVerStraightJumpPoints(isJumpPoints, false);
    }

    private void InitSouthStraightJumpPoints(bool[,,] isJumpPoints)
    {
        InitVerStraightJumpPoints(isJumpPoints, true);
    }

    #region Diagonal Jump Points
    private void InitSouthWestJumpPoints()
    {
        for(int y = 0; y < m_mapHeight; y++)
        {
            for(int x = 0; x < m_mapWidth; x++)
            {
                if (!IsWalkableAt(x, y))
                    continue;

                if(x == 0 || y == 0 || !IsWalkableAt(x - 1, y - 1) || (!IsWalkableAt(x - 1, y) && !IsWalkableAt(x, y - 1)))
                {
                    m_distanceData[y, x, c_southWest] = 0;
                }
                else if((IsWalkableAt(x - 1, y) || IsWalkableAt(x, y - 1)) && 
                            (m_distanceData[y - 1, x - 1, c_south] > 0 || m_distanceData[y - 1, x - 1, c_west] > 0))
                {
                    //Straight jump point one away
                    m_distanceData[y, x, c_southWest] = 1;
                }
                else
                {
                    //Increment from last
                    int jumpDistance = m_distanceData[y - 1, x - 1, c_southWest];
                    if (jumpDistance > 0)
                        m_distanceData[y, x, c_southWest] = 1 + jumpDistance;
                    else
                        m_distanceData[y, x, c_southWest] = -1 + jumpDistance;
                }
            }
        }
    }

    private void InitNorthWestJumpPoints()
    {
        for (int y = m_mapHeight - 1; y >= 0; y--)
        {
            for (int x = 0; x < m_mapWidth; x++)
            {
                if (!IsWalkableAt(x, y))
                    continue;

                if (x == 0 || y == m_mapHeight - 1 || !IsWalkableAt(x - 1, y + 1) || (!IsWalkableAt(x - 1, y) && !IsWalkableAt(x, y + 1)))
                {
                    m_distanceData[y, x, c_northWest] = 0;
                }
                else if ((IsWalkableAt(x - 1, y) || IsWalkableAt(x, y + 1)) &&
                            (m_distanceData[y + 1, x - 1, c_north] > 0 || m_distanceData[y + 1, x - 1, c_west] > 0))
                {
                    //Straight jump point one away
                    m_distanceData[y, x, c_northWest] = 1;
                }
                else
                {
                    //Increment from last
                    int jumpDistance = m_distanceData[y + 1, x - 1, c_northWest];
                    if (jumpDistance > 0)
                        m_distanceData[y, x, c_northWest] = 1 + jumpDistance;
                    else
                        m_distanceData[y, x, c_northWest] = -1 + jumpDistance;
                }
            }
        }
    }

    private void InitSouthEastJumpPoints()
    {
        for (int y = 0; y < m_mapHeight; y++)
        {
            for (int x = m_mapWidth - 1; x >= 0; x--)
            {
                if (!IsWalkableAt(x, y))
                    continue;

                if (x == m_mapWidth - 1 || y == 0 || !IsWalkableAt(x + 1, y - 1) || (!IsWalkableAt(x + 1, y) && !IsWalkableAt(x, y - 1)))
                {
                    m_distanceData[y, x, c_southEast] = 0;
                }
                else if ((IsWalkableAt(x + 1, y) || IsWalkableAt(x, y - 1)) &&
                            (m_distanceData[y - 1, x + 1, c_south] > 0 || m_distanceData[y - 1, x + 1, c_east] > 0))
                {
                    //Straight jump point one away
                    m_distanceData[y, x, c_southEast] = 1;
                }
                else
                {
                    //Increment from last
                    int jumpDistance = m_distanceData[y - 1, x + 1, c_southEast];
                    if (jumpDistance > 0)
                        m_distanceData[y, x, c_southEast] = 1 + jumpDistance;
                    else
                        m_distanceData[y, x, c_southEast] = -1 + jumpDistance;
                }
            }
        }
    }

    private void InitNorthEastJumpPoints()
    {
        for (int y = m_mapHeight - 1; y >= 0; y--)
        {
            for (int x = m_mapWidth - 1; x >= 0; x--)
            {
                if (!IsWalkableAt(x, y))
                    continue;

                if (x == m_mapWidth - 1 || y == m_mapHeight - 1 || !IsWalkableAt(x + 1, y + 1) || (!IsWalkableAt(x + 1, y) && !IsWalkableAt(x, y + 1)))
                {
                    m_distanceData[y, x, c_northEast] = 0;
                }
                else if ((IsWalkableAt(x + 1, y) || IsWalkableAt(x, y + 1)) &&
                            (m_distanceData[y + 1, x + 1, c_north] > 0 || m_distanceData[y + 1, x + 1, c_east] > 0))
                {
                    //Straight jump point one away
                    m_distanceData[y, x, c_northEast] = 1;
                }
                else
                {
                    //Increment from last
                    int jumpDistance = m_distanceData[y + 1, x + 1, c_northEast];
                    if (jumpDistance > 0)
                        m_distanceData[y, x, c_northEast] = 1 + jumpDistance;
                    else
                        m_distanceData[y, x, c_northEast] = -1 + jumpDistance;
                }
            }
        }
    }
    #endregion
    #endregion
}
