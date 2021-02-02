using Pathfinding.VisibilityGraph;
using System.Collections.Generic;
using UnityEngine;

public class VisibilityGraphDemo : MonoBehaviour 
{
    private Material m_mat;
    private Vector2 m_start = new Vector2(50, 10);
    private Vector2 m_end = new Vector2(300, 300);

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            m_start = Input.mousePosition;
        else if (Input.GetMouseButtonDown(1))
            m_end = Input.mousePosition;
    }

    void OnDrawGizmos()
    {
        if (m_mat == null)
            m_mat = new Material(Shader.Find("Unlit/Color"));

        m_mat.SetColor("_Color", Color.black);
        List<Polygon> obstacles = new List<Polygon>()
        {
            new Polygon(new Vector2[]
            {
                new Vector2(100, 50),
                new Vector2(100, 100),
                new Vector2(180, 100),
                new Vector2(180, 50),
            }),
            new Polygon(new Vector2[]
            {
                new Vector2(250, 120),
                new Vector2(230, 140),
                new Vector2(250, 160),
                new Vector2(290, 150),
            }),
            new Polygon(new Vector2[]
            {
                new Vector2(90, 130),
                new Vector2(70, 150),
                new Vector2(110, 150),
            }),
            new Polygon(new Vector2[]
            {
                new Vector2(150, 200),
                new Vector2(150, 250),
                new Vector2(280, 250),
                new Vector2(280, 200),
            }),
        };
        for(int i = 0; i < obstacles.Count; i++)
            GraphicsTool.DrawPolygon(obstacles[i].Points, m_mat, true, true);

        m_mat.SetColor("_Color", Color.yellow);
        List<GraphNode> nodes = VisibilityGraphGenerator.Generate(m_start, m_end, obstacles);
        for(int a = 0; a < nodes.Count; a++)
        {
            var n = nodes[a];
            GraphicsTool.DrawPoint(nodes[a].Center, 3, m_mat);
            for (int i = 0; i < n.Neighbors.Count; i++)
                GraphicsTool.DrawLine(n.Center, n.Neighbors[i].Center, m_mat);
        }

        m_mat.SetColor("_Color", Color.red);
        GraphicsTool.DrawPoint(m_start, 4, m_mat);
        GraphicsTool.DrawPoint(m_end, 4, m_mat);
        GraphNode startNode = nodes[nodes.Count - 2];
        GraphNode endNode = nodes[nodes.Count - 1];
        GraphAStar astar = new GraphAStar(startNode, endNode);
        astar.Process();
        List<Vector2> path = new List<Vector2>();
        while (endNode != null)
        {
            path.Add(endNode.Center);
            endNode = endNode.Parent;
        }
        GraphicsTool.DrawPolygon(path, m_mat, false);
    }
}
