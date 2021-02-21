using System.Collections.Generic;
using UnityEngine;
using Pathfinding.ClipperLib;
using Pathfinding.Poly2Tri;

namespace Pathfinding.Navmesh
{
	/// <summary>
	/// 简单的Navmesh生成器玩具，真正应用还得是业界标准RecastNavgation
	/// </summary>
	public class NavmeshGenerator
	{
		public static List<GraphNode> Generate(List<List<IntPoint>> walkablePolygons, List<List<IntPoint>> blockedPolygons,  out List<DelaunayTriangle> triangles)
		{
			var polygons = ClipPolygons(walkablePolygons, blockedPolygons);
			triangles = GenerateTriangles(polygons);

			return TrianglesToNodes(triangles);
		}

		/// <summary>
		/// 将三角形转为图搜索节点
		/// </summary>
		private static List<GraphNode> TrianglesToNodes(List<DelaunayTriangle> triangles)
		{
			List<GraphNode> result = new List<GraphNode>();

			var dict = new Dictionary<DelaunayTriangle, GraphNode>();
			for (int i = 0; i < triangles.Count; i++)
			{
				var centroid = triangles[i].Centroid();
				var center = new Vector2(centroid.Xf, centroid.Yf);
				var node = new GraphNode(center);
				dict[triangles[i]] = node;
				result.Add(node);
			}

			for (int i = 0; i < triangles.Count; i++)
			{
				var t = triangles[i];
				var node = result[i];

				if (t.Neighbors._0 != null)
				{
					if (dict.TryGetValue(t.Neighbors._0, out GraphNode n))
						node.AddNeighbor(n);
				}
				if (t.Neighbors._1 != null)
				{
					if (dict.TryGetValue(t.Neighbors._1, out GraphNode n))
						node.AddNeighbor(n);
				}
				if (t.Neighbors._2 != null)
				{
					if (dict.TryGetValue(t.Neighbors._2, out GraphNode n))
						node.AddNeighbor(n);
				}
			}

			return result;
		}

		/// <summary>
		/// 使用Clipper将阻挡裁剪出来
		/// </summary>
		private static List<List<IntPoint>> ClipPolygons(List<List<IntPoint>> walkablePolygons, List<List<IntPoint>> blockedPolygons)
		{
			//合并可走区域
			List<List<IntPoint>> result = new List<List<IntPoint>>();
			Clipper clipper = new Clipper();
			clipper.AddPolygons(walkablePolygons, PolyType.ptClip);
			clipper.Execute(ClipType.ctUnion, result, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

			clipper.Clear();

			//去掉不可走区域
			clipper.AddPolygons(result, PolyType.ptSubject);
			clipper.AddPolygons(blockedPolygons, PolyType.ptClip);
			clipper.Execute(ClipType.ctDifference, result);

			return result;
		}

		/// <summary>
		/// 使用Poly2Tri进行三角剖分
		/// </summary>
		/// <param name="polygons"></param>
		/// <returns></returns>
		private static List<DelaunayTriangle> GenerateTriangles(List<List<IntPoint>> polygons)
		{
			//根据时针方向判断可走区域和障碍
			var walkables = new List<Polygon>();
			var blockeds = new List<Polygon>();
			for (int i = 0; i < polygons.Count; i++)
			{
				var list = Convert(polygons[i]);
				if (IsCCW(polygons[i]))
					walkables.Add(new Polygon(list));
				else
					blockeds.Add(new Polygon(list));
			}

			//可以考虑添加SteinerPoint来避免生成狭长的三角形

			//三角剖分
			List<DelaunayTriangle> triangles = new List<DelaunayTriangle>();
			for (int index = 0; index < walkables.Count; index++)
			{
				for (int i = 0; i < blockeds.Count; i++)
					walkables[index].AddHole(blockeds[i]);

				P2T.Triangulate(walkables[index]);
				triangles.AddRange(walkables[index].Triangles);
			}

			return triangles;
		}

		private static List<PolygonPoint> Convert(List<IntPoint> list)
		{
			List<PolygonPoint> result = new List<PolygonPoint>();

			for (int i = 0; i < list.Count; i++)
				result.Add(new PolygonPoint(list[i].X, list[i].Y));

			return result;
		}

		private static Vector2 Convert(TriangulationPoint p)
		{
			return new Vector2(p.Xf, p.Yf);
		}

		private static bool IsCCW(List<IntPoint> polygon)
		{
			for (int i = 2; i < polygon.Count; i++)
			{
				var p0 = polygon[i - 2];
				var p1 = polygon[i - 1];
				var p2 = polygon[i];

				var cross = (p1.X - p0.X) * (p2.Y - p1.Y) - (p2.X - p1.X) * (p1.Y - p0.Y);
				if (cross > 0)
					return true;
				else if (cross < 0)
					return false;
			}

			return false; //点都在一条直线上
		}

		struct Edge
		{
			public Vector2 v0;
			public Vector2 v1;

			public Edge(TriangulationPoint p0, TriangulationPoint p1)
			{
				v0 = Convert(p0);
				v1 = Convert(p1);
			}

			public override int GetHashCode()
			{
				return v0.GetHashCode() + v1.GetHashCode();
			}
		}
	}
}