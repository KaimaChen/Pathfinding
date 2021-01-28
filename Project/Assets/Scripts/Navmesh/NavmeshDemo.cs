using Pathfinding.ClipperLib;
using Pathfinding.Navmesh;
using Pathfinding.Poly2Tri;
using System.Collections.Generic;
using UnityEngine;

public class NavmeshDemo : MonoBehaviour 
{
	private Material m_mat;

	void OnDrawGizmos()
    {
		if(m_mat == null)
			m_mat = new Material(Shader.Find("Unlit/Color"));

		List<List<IntPoint>> walkablePolygons = new List<List<IntPoint>>()
		{
			new List<IntPoint>()
			{
				new IntPoint(10, 10),
				new IntPoint(600, 10),
				new IntPoint(600, 400),
				new IntPoint(10, 400),
			}
		};

		List<List<IntPoint>> blockedPolygons = new List<List<IntPoint>>()
		{
			new List<IntPoint>()
			{
				new IntPoint(100, 160),
				new IntPoint(150, 160),
				new IntPoint(200, 200),
				new IntPoint(130, 300),
			},
			new List<IntPoint>()
			{
				new IntPoint(220, 50),
				new IntPoint(260, 50),
				new IntPoint(260, 100),
				new IntPoint(220, 100),
			},
			new List<IntPoint>()
			{
				new IntPoint(300, 150),
				new IntPoint(350, 150),
				new IntPoint(350, 200),
				new IntPoint(300, 200),
			},
			new List<IntPoint>()
			{
				new IntPoint(400, 250),
				new IntPoint(450, 250),
				new IntPoint(450, 300),
				new IntPoint(400, 300),
			},
		};

		var triangles = NavmeshGenerator.Generate(walkablePolygons, blockedPolygons);

		DelaunayTriangle start = null, end = null;
		float minX = float.MaxValue, maxX = 0;
		for(int i = 0;  i < triangles.Count; i++)
        {
			var t = triangles[i];
			var centroid = t.Centroid();
			if(centroid.Xf < minX)
            {
				minX = centroid.Xf;
				start = t;
            }
			if(centroid.Xf > maxX)
            {
				maxX = centroid.Xf;
				end = t;
            }

			//鼠标所在三角形为终点
			if (IsTriangleContains(t, Input.mousePosition))
            {
				maxX = float.MaxValue;
				end = t;
            }
        }

		NavmeshAStar algo = new NavmeshAStar(triangles, start, end);
		algo.Process();

		m_mat.SetColor("_Color", Color.black);
		for (int i = 0; i < blockedPolygons.Count; i++)
			DrawPolygon(blockedPolygons[i], true, true);

		m_mat.SetColor("_Color", Color.white);
        for (int i = 0; i < triangles.Count; i++)
            DrawTriangle(triangles[i], false);

		m_mat.SetColor("_Color", Color.yellow);
		DrawTriangle(start, false);
		DrawTriangle(end, false);

		List<Vector2> path = new List<Vector2>();
		var node = NavmeshAStar.EndNode;
		while (node != null)
        {
			path.Add(node.Center);
			node = node.Parent;
        }

		m_mat.SetColor("_Color", Color.red);
		GraphicsTool.DrawPolygon(path, m_mat, false);
	}

	private static bool IsTriangleContains(DelaunayTriangle triangle, Vector2 pos)
    {
		var p0 = triangle.Points._0;
		var p1 = triangle.Points._1;
		var p2 = triangle.Points._2;

		var cross01 = (p1.Xf - p0.Xf) * (pos.y - p1.Yf) - (pos.x - p1.Xf) * (p1.Yf - p0.Yf);
		var cross12 = (p2.Xf - p1.Xf) * (pos.y - p2.Yf) - (pos.x - p2.Xf) * (p2.Yf - p1.Yf);
		var cross20 = (p0.Xf - p2.Xf) * (pos.y - p0.Yf) - (pos.x - p0.Xf) * (p0.Yf - p2.Yf);
		return (Mathf.Sign(cross01) == Mathf.Sign(cross12)) && (Mathf.Sign(cross01) == Mathf.Sign(cross20));
	}

	void DrawPolygon(List<IntPoint> polygon, bool isCCW, bool isFill)
    {
		List<Vector2> points = new List<Vector2>();

        if (isCCW)
        {
			for(int i = polygon.Count - 1; i >= 0; i--)
				points.Add(new Vector2(polygon[i].X, polygon[i].Y));
		}
		else
        {
			for (int i = 0; i < polygon.Count; i++)
				points.Add(new Vector2(polygon[i].X, polygon[i].Y));
		}
		

		GraphicsTool.DrawPolygon(points, m_mat, true, isFill);
    }

	void DrawTriangle(DelaunayTriangle t, bool isFill = false)
    {
		if (t == null)
			return;

		var v0 = new Vector2(t.Points._0.Xf, t.Points._0.Yf);
		var v1 = new Vector2(t.Points._1.Xf, t.Points._1.Yf);
		var v2 = new Vector2(t.Points._2.Xf, t.Points._2.Yf);
		GraphicsTool.DrawTriangle(v2, v1, v0, m_mat, isFill);

		var centroid = t.Centroid();
		GraphicsTool.DrawPoint(new Vector2(centroid.Xf, centroid.Yf), 3, m_mat);
	}
}