using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding.VisibilityGraph
{
    /// <summary>
    /// 障碍不是多边形而是圆的可见性图
	/// 注意：圆都需要是分离的，因为没有处理圆相交影响HuggingEdge的情况，感觉一般游戏里并不会存在圆相交或包围的情况
    /// 参考：
    /// https://redblobgames.github.io/circular-obstacle-pathfinding/
    /// </summary>
    public class CircleVisibilityGraphGenerator
	{
		public static List<CircleGraphNode> Generate(Vector2 start, Vector2 end, List<Circle> obstacles)
        {
			return BruteForce(start, end, obstacles);
        }

        #region 蛮力算法
		private static List<CircleGraphNode> BruteForce(Vector2 start, Vector2 end, List<Circle> obstacles)
        {
			List<CircleGraphNode> result = new List<CircleGraphNode>();

			for (int i = 0; i < obstacles.Count; i++)
				obstacles[i].surfingPoints.Clear();

			CircleGraphNode startNode = new CircleGraphNode(start, null);
			CircleGraphNode endNode = new CircleGraphNode(end, null);
			result.Add(startNode);
			result.Add(endNode);

			for (int i = 0; i < obstacles.Count; i++)
            {
				var a = obstacles[i];

				//连接起点和终点
				HandlePointCircleEdges(startNode, a, obstacles, result);
				HandlePointCircleEdges(endNode, a, obstacles, result);

				//到别的圆的链接
				for (int j = i + 1; j < obstacles.Count; j++)
                {
					var b = obstacles[j];
					
					var internalBitangents = InternalBitangents(a, b);
					HandleEdges(a, b, obstacles, internalBitangents, result);

					var externalBitangents = ExternalBitangents(a, b);
					HandleEdges(a, b, obstacles, externalBitangents, result);
                }

				//本圆的链接
				for(int j = 0; j < a.surfingPoints.Count - 1; j++)
                {
					for(int k = j + 1; k < a.surfingPoints.Count; k++)
                    {
						var ga = a.surfingPoints[j];
						var gb = a.surfingPoints[k];
						ga.AddNeighbor(gb);
						gb.AddNeighbor(ga);
					}
                }
			}

			return result;
        }
        #endregion

        private static void HandleEdges(Circle a, Circle b, List<Circle> circles, List<Edge> edges, List<CircleGraphNode> nodes)
        {
			if (edges == null)
				return;

			for(int index = 0; index < circles.Count; index++)
            {
				var c = circles[index];
				if (ReferenceEquals(a, c) || ReferenceEquals(b, c))
					continue;

				for(int i = edges.Count - 1; i >= 0; i--)
                {
					var e = edges[i];
					if (!c.CheckLineOfSignt(e))
						edges.RemoveAt(i);
                }

				if (edges.Count <= 0)
					break;
            }

			for(int i = 0; i < edges.Count; i++)
            {
				var ga = new CircleGraphNode(edges[i].a, a);
				a.surfingPoints.Add(ga);
				nodes.Add(ga);

				var gb = new CircleGraphNode(edges[i].b, b);
				b.surfingPoints.Add(gb);
				nodes.Add(gb);

				ga.AddNeighbor(gb);
				gb.AddNeighbor(ga);
            }
        }

		public static List<Edge> InternalBitangents(Circle a, Circle b)
		{
			float d = Vector2.Distance(a.center, b.center);
			if (d <= 0) return null;

			float v = (a.radius + b.radius) / d;
			if (v > 1) return null; //两个圆相交或包围

			float theta = Mathf.Acos(v);

			float abAngle = Utils.Facing(a.center, b.center);
			var a1 = Utils.DirectionStep(a.center, a.radius, abAngle + theta);
			var a2 = Utils.DirectionStep(a.center, a.radius, abAngle - theta);

			float baAngle = Utils.Facing(b.center, a.center);
			var b1 = Utils.DirectionStep(b.center, b.radius, baAngle + theta);
			var b2 = Utils.DirectionStep(b.center, b.radius, baAngle - theta);

			return new List<Edge>()
			{
				new Edge(a1, b1),
				new Edge(a2, b2),
			};
		}

		public static List<Edge> ExternalBitangents(Circle a, Circle b)
		{
			float d = Vector2.Distance(a.center, b.center);
			if (d <= 0) return null;

			float v = Mathf.Abs(a.radius - b.radius) / d;
			if (v > 1) return null; //一个圆包围另一个圆的情况

			float theta = Mathf.Acos(v);

			float abAngle = Utils.Facing(a.center, b.center);
			var a1 = Utils.DirectionStep(a.center, a.radius, abAngle + theta);
			var a2 = Utils.DirectionStep(a.center, a.radius, abAngle - theta);
			var b1 = Utils.DirectionStep(b.center, b.radius, abAngle + theta);
			var b2 = Utils.DirectionStep(b.center, b.radius, abAngle - theta);

			return new List<Edge>()
			{
				new Edge(a1, b1),
				new Edge(a2, b2),
			};
		}

		/// <summary>
		/// 计算点连接圆的两条切线（用来处理起点和终点）
		/// </summary>
		private static void HandlePointCircleEdges(CircleGraphNode p, Circle c, List<Circle> circles, List<CircleGraphNode> nodes)
        {
			float d = Vector2.Distance(c.center, p.Center);
			float theta = Mathf.Acos(c.radius / d);
			Vector2 toP = p.Center - c.center;
			float alpha = Mathf.Atan2(toP.y, toP.x);

			var c1 = c.center + new Vector2(Mathf.Cos(alpha + theta), Mathf.Sin(alpha + theta)) * c.radius;
			var e1 = new Edge(p.Center, c1);
			var c2 = c.center + new Vector2(Mathf.Cos(alpha - theta), Mathf.Sin(alpha - theta)) * c.radius;
			var e2 = new Edge(p.Center, c2);

			bool isValid1 = true, isValid2 = true;
			for(int i = 0; i < circles.Count; i++)
            {
				if (ReferenceEquals(circles[i], c))
					continue;

				if (isValid1 && !circles[i].CheckLineOfSignt(e1))
					isValid1 = false;
				if (isValid2 && !circles[i].CheckLineOfSignt(e2))
					isValid2 = false;
				if (!isValid1 && !isValid2)
					break;
            }

			if(isValid1)
            {
				CircleGraphNode gb = new CircleGraphNode(e1.b, c);
				p.AddNeighbor(gb);
				gb.AddNeighbor(p);
				nodes.Add(gb);
				c.surfingPoints.Add(gb);
            }

            if (isValid2)
            {
				CircleGraphNode gb = new CircleGraphNode(e2.b, c);
				p.AddNeighbor(gb);
				gb.AddNeighbor(p);
				nodes.Add(gb);
				c.surfingPoints.Add(gb);
            }
		}
	}

	public struct Edge
    {
		public Vector2 a;
		public Vector2 b;
		public Edge(Vector2 a, Vector2 b)
        {
			this.a = a;
			this.b = b;
        }
    }

	public class Circle
    {
		public Vector2 center;
		public float radius;
		public readonly List<CircleGraphNode> surfingPoints;

		public Circle(Vector2 c, float r)
        {
			center = c;
			radius = r;
			surfingPoints = new List<CircleGraphNode>();
        }

		public bool Contains(Vector2 p)
        {
			float sqrDist = (center - p).sqrMagnitude;
			return sqrDist <= radius * radius;
        }

		public bool CheckLineOfSignt(Edge edge)
		{
			Vector2 a = edge.a, b = edge.b;
			float u = Vector2.Dot(center - a, b - a) / Vector2.Dot(b - a, b - a);
			Vector2 e = a + Mathf.Clamp01(u) * (b - a);
			float d = Vector2.Distance(e, center);
			return d >= radius;
		}
    }
}