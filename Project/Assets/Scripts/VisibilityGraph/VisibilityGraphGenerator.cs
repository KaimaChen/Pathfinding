using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding.VisibilityGraph
{
    /// <summary>
    /// 生成可见性图
    /// 参考：
    /// * 简单介绍：http://www.cs.kent.edu/~dragan/ST-Spring2016/visibility%20graphs.pdf
    /// * Lee's O(n^2 logn)算法：《计算几何算法与应用》第15章、http://dav.ee/papers/Visibility_Graph_Algorithm.pdf
    /// * Ghosh等人发明了 O(|e| + n logn)的算法
    /// * Reduced Visibility Graph：可以去掉中间一些多余的线段，简化整张寻路图
    /// </summary>
    public class VisibilityGraphGenerator
    {
        public static List<GraphNode> Generate(Vector2 start, Vector2 end, List<Polygon> obstalces)
        {
            return BruteForce(start, end, obstalces);
        }

        #region 蛮力算法
        /// <summary>
        /// 蛮力：直接两两判断
        /// 复杂度：O(n^3)
        /// </summary>
        private static List<GraphNode> BruteForce(Vector2 start, Vector2 end, List<Polygon> obstacles)
        {
            List<GraphNode> result = new List<GraphNode>();

            //生成所有节点
            for (int m = 0; m < obstacles.Count; m++)
            {
                var polygon = obstacles[m];
                for (int i = 0; i < polygon.Points.Length; i++)
                    result.Add(new GraphNode(polygon.Points[i]));
            }
            result.Add(new GraphNode(start));
            result.Add(new GraphNode(end));

            //两两判断顶点是否直接可见
            for (int u = 0; u < result.Count; u++)
            {
                var node1 = result[u];
                for (int v = u + 1; v < result.Count; v++)
                {
                    var node2 = result[v];
                    Line line = new Line(node1.Center, node2.Center);

                    bool isIntersect = false;
                    for (int w = 0; w < obstacles.Count; w++)
                    {
                        if (obstacles[w].IsIntersect(line))
                        {
                            isIntersect = true;
                            break;
                        }
                    }

                    if (!isIntersect)
                    {
                        node1.AddNeighbor(node2);
                        node2.AddNeighbor(node1);
                    }
                }
            }

            return result;
        }
        #endregion
    }

    public struct Polygon
    {
        public readonly Vector2[] Points;
        private readonly Dictionary<Vector2, int> PointDict;

        public Polygon(Vector2[] arr)
        {
            Points = arr;

            PointDict = new Dictionary<Vector2, int>();
            for (int i = 0; i < arr.Length; i++)
                PointDict[arr[i]] = i;
        }

        /// <summary>
        /// 障碍是开放集，即其边缘不视为障碍
        /// </summary>
        public bool IsIntersect(Line other)
        {
            //处理线段在障碍内部的特殊情况
            int aIndex, bIndex;
            if (PointDict.TryGetValue(other.a, out aIndex) && PointDict.TryGetValue(other.b, out bIndex))
                return Mathf.Abs(aIndex - bIndex) != 1;

            //线段在障碍外部的正常情况
            for (int i = 0; i < Points.Length - 1; i++)
            {
                Line line = new Line(Points[i], Points[i + 1]);
                if (line.IsIntersect(other))
                    return true;
            }
            Line last = new Line(Points[0], Points[Points.Length - 1]);
            return last.IsIntersect(other);
        }
    }

    public struct Line
    {
        public Vector2 b;
        public Vector2 a;

        public Line(Vector2 a, Vector2 b)
        {
            this.a = a;
            this.b = b;
        }

        public bool IsIntersect(Line other)
        {
            Vector2 c = other.a, d = other.b;
            
            float u = (c.x - a.x) * (b.y - a.y) - (b.x - a.x) * (c.y - a.y);
            float v = (d.x - a.x) * (b.y - a.y) - (b.x - a.x) * (d.y - a.y);
            float w = (a.x - c.x) * (d.y - c.y) - (d.x - c.x) * (a.y - c.y);
            float z = (b.x - c.x) * (d.y - c.y) - (d.x - c.x) * (b.y - c.y);
            return u * v < 0 && w*z < 0;
        }
    }
}