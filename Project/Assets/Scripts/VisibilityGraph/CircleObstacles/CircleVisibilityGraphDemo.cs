using UnityEngine;
using Pathfinding.VisibilityGraph;
using System.Collections.Generic;

/// <summary>
/// 障碍为圆的可视图
/// </summary>
public class CircleVisibilityGraphDemo : MonoBehaviour
{
    private Material m_mat;

    private List<Circle> m_circles = new List<Circle>()
    {
        new Circle(new Vector2(100, 100), 60),
        new Circle(new Vector2(362, 334), 62),
        new Circle(new Vector2(218, 183), 52),
        new Circle(new Vector2(236, 305), 29),
        new Circle(new Vector2(354, 165), 32),
    };
    private Circle m_selectedCircle = null;

    private Vector2 m_start = new Vector2(50, 50);
    private Vector2 m_end = new Vector2(400, 400);
    
    void Update()
    {
        //左键选中圆来拖动
        if (Input.GetMouseButtonDown(0))
        {
            m_selectedCircle = null;
            for (int i = 0; i < m_circles.Count; i++)
            {
                if (m_circles[i].Contains(Input.mousePosition))
                {
                    m_selectedCircle = m_circles[i];
                    break;
                }
            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (m_selectedCircle != null)
            {
                var oldPos = m_selectedCircle.center;
                m_selectedCircle.center = Input.mousePosition;
                if (!IsAllCircleSeperate(m_selectedCircle))
                    m_selectedCircle.center = oldPos;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            m_selectedCircle = null;
        }

        //滚轮缩放选中圆
        if(m_selectedCircle != null)
        {
            float oldRadius = m_selectedCircle.radius;

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            float radius = m_selectedCircle.radius + (int)(scroll * 10);
            m_selectedCircle.radius = Mathf.Clamp(radius, 5, 100);
            if (!IsAllCircleSeperate(m_selectedCircle))
                m_selectedCircle.radius = oldRadius;
        }
        else
        {
            //中间添加圆
            if(Input.GetMouseButtonDown(2))
            {
                var c = new Circle(Input.mousePosition, 10);
                if(IsAllCircleSeperate(c))
                    m_circles.Add(c);
            }
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            string msg = string.Empty;
            for (int i = 0; i < m_circles.Count; i++)
                msg += "[" + m_circles[i].center + ", " + m_circles[i].radius + "] ";
            Debug.Log(msg);
        }
    }

    bool IsAllCircleSeperate(Circle c)
    {
        for(int i = 0; i < m_circles.Count; i++)
        {
            if (ReferenceEquals(m_circles[i], c)) continue;

            float d = Vector2.Distance(m_circles[i].center, c.center);
            if (d <= m_circles[i].radius + c.radius)
                return false;
        }

        return true;
    }

    void OnDrawGizmos()
    {
        if (m_mat == null)
            m_mat = new Material(Shader.Find("Unlit/Color"));

        //画点和边
        m_mat.SetColor("_Color", new Color(0.5f, 0.5f, 0.5f));
        List<CircleGraphNode> nodes = CircleVisibilityGraphGenerator.Generate(m_start, m_end, m_circles);
        for(int i = 0; i < nodes.Count; i++)
        {
            var curt = nodes[i];
            GraphicsTool.DrawPoint(curt.Center, 2, m_mat);
            for (int j = 0; j < curt.Neighbors.Count; j++)
            {
                var next = curt.Neighbors[j] as CircleGraphNode;
                if(curt.BelongCircle == null || curt.BelongCircle != next.BelongCircle)
                    GraphicsTool.DrawLine(curt.Center, next.Center, m_mat);
            }
        }

        //画圆
        m_mat.SetColor("_Color", new Color(1, 0.7f, 0));
        for (int i = 0; i < m_circles.Count; i++)
            DrawCircle(m_circles[i]);
        if(m_selectedCircle != null)
        {
            m_mat.SetColor("_Color", new Color(1, 1, 0));
            DrawCircle(m_selectedCircle);
        }

        //寻路
        CircleGraphNode startNode = nodes[0];
        CircleGraphNode endNode = nodes[1];
        var astar = new CircleGraphAStar(startNode, endNode);
        astar.Process();

        //画路径
        m_mat.SetColor("_Color", Color.red);
        GraphicsTool.DrawPoint(m_start, 4, m_mat);
        GraphicsTool.DrawPoint(m_end, 4, m_mat);
        CircleGraphNode lastNode = null;
        while (endNode != null)
        {
            if (lastNode != null)
            {
                if(lastNode.BelongCircle != null && lastNode.BelongCircle == endNode.BelongCircle)
                {
                    var c = lastNode.BelongCircle;
                    float radian1 = Utils.Facing(c.center, lastNode.Center);
                    float radian2 = Utils.Facing(c.center, endNode.Center);
                    GraphicsTool.DrawArc(c.center, c.radius, radian1, radian2, m_mat, 10);
                }
                else
                {
                    GraphicsTool.DrawLine(lastNode.Center, endNode.Center, m_mat);
                }
            }

            lastNode = endNode;
            endNode = endNode.Parent as CircleGraphNode;
        }
    }

    void DrawCircle(Circle c)
    {
        GraphicsTool.DrawCircle(c.center, c.radius, m_mat, 60);
    }
}