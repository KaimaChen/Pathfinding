using UnityEngine;
using System.Collections.Generic;

public static class GraphicsTool {
    //创建正交投影矩阵
    public static void CreateOrthogonalProjectMatrix(ref Matrix4x4 projectMatrix, Vector3 maxInViewSpace, Vector3 minInViewSpace)
    {
        float scaleX, scaleY, scaleZ;
        float offsetX, offsetY, offsetZ;
        scaleX = 2.0f / (maxInViewSpace.x - minInViewSpace.x);
        scaleY = 2.0f / (maxInViewSpace.y - minInViewSpace.y);
        offsetX = -0.5f * (maxInViewSpace.x + minInViewSpace.x) * scaleX;
        offsetY = -0.5f * (maxInViewSpace.y + minInViewSpace.y) * scaleY;
        scaleZ = 1.0f / (maxInViewSpace.z - minInViewSpace.z);
        offsetZ = -minInViewSpace.z * scaleZ;

        //列矩阵
        projectMatrix.m00 = scaleX; projectMatrix.m01 = 0.0f; projectMatrix.m02 = 0.0f; projectMatrix.m03 = offsetX;
        projectMatrix.m10 = 0.0f; projectMatrix.m11 = scaleY; projectMatrix.m12 = 0.0f; projectMatrix.m13 = offsetY;
        projectMatrix.m20 = 0.0f; projectMatrix.m21 = 0.0f; projectMatrix.m22 = scaleZ; projectMatrix.m23 = offsetZ;
        projectMatrix.m30 = 0.0f; projectMatrix.m31 = 0.0f; projectMatrix.m32 = 0.0f; projectMatrix.m33 = 1.0f;
    }

    //创建视图矩阵
    public static void CreateViewMatrix(ref Matrix4x4 viewMatrix, Vector3 look, Vector3 up, Vector3 right, Vector3 pos)
    {
        look.Normalize();
        up.Normalize();
        right.Normalize();

        float x = -Vector3.Dot(right, pos);
        float y = -Vector3.Dot(up, pos);
        float z = -Vector3.Dot(look, pos);

        viewMatrix.m00 = right.x; viewMatrix.m10 = up.x; viewMatrix.m20 = look.x; viewMatrix.m30 = 0.0f;
        viewMatrix.m01 = right.y; viewMatrix.m11 = up.y; viewMatrix.m21 = look.y; viewMatrix.m31 = 0.0f;
        viewMatrix.m02 = right.z; viewMatrix.m12 = up.z; viewMatrix.m22 = look.z; viewMatrix.m32 = 0.0f;
        viewMatrix.m03 = x; viewMatrix.m13 = y; viewMatrix.m23 = z; viewMatrix.m33 = 1.0f;
    }

    public static void DrawPoint(Vector2 center, float size, Material mat)
    {
        mat.SetPass(0);

        GL.PushMatrix();
        GL.LoadPixelMatrix(); //转成屏幕坐标

        GL.Begin(GL.QUADS);
        GL.Vertex3(center.x - size, center.y + size, 0);
        GL.Vertex3(center.x + size, center.y + size, 0);
        GL.Vertex3(center.x + size, center.y - size, 0);
        GL.Vertex3(center.x - size, center.y - size, 0);
        GL.End();

        GL.PopMatrix();
    }

    //画矩形
    public static void DrawRect(Vector2 bottomLeft, Vector2 topRight, bool isFill, Material mat)
    {
        mat.SetPass(0);

        GL.PushMatrix();
        GL.LoadPixelMatrix(); //转成屏幕坐标

        if(isFill)
        {
            GL.Begin(GL.QUADS);
            GL.Vertex3(bottomLeft.x, bottomLeft.y, 0);
            GL.Vertex3(bottomLeft.x, topRight.y, 0);
            GL.Vertex3(topRight.x, topRight.y, 0);
            GL.Vertex3(topRight.x, bottomLeft.y, 0);
        }

        
        GL.Begin(GL.LINES);

        //上边
        GL.Vertex3(bottomLeft.x, topRight.y, 0);
        GL.Vertex3(topRight.x, topRight.y, 0);
        //下边
        GL.Vertex3(bottomLeft.x, bottomLeft.y, 0);
        GL.Vertex3(topRight.x, bottomLeft.y, 0);
        //左边
        GL.Vertex3(bottomLeft.x, topRight.y, 0);
        GL.Vertex3(bottomLeft.x, bottomLeft.y, 0);
        //右边
        GL.Vertex3(topRight.x, topRight.y, 0);
        GL.Vertex3(topRight.x, bottomLeft.y, 0);

        GL.End();

        GL.PopMatrix();
    }

    public static void DrawLine(Vector2 begin, Vector2 end, Material mat, bool drawNormal = false)
    {
        mat.SetPass(0);

        GL.PushMatrix();
        GL.LoadPixelMatrix(); //转成屏幕坐标

        GL.Begin(GL.LINES);
        GL.Vertex3(begin.x, begin.y, 0);
        GL.Vertex3(end.x, end.y, 0);

        if (drawNormal)
        {
            Vector2 mid = (begin + end) / 2;
            Vector2 dir = end - begin;
            Vector2 n = new Vector2(-dir.y, dir.x).normalized;
            end = mid + n * 5;
            GL.Vertex3(mid.x, mid.y, 0);
            GL.Vertex3(end.x, end.y, 0);
        }
        GL.End();

        GL.PopMatrix();
    }

    public static void DrawDotLine(Vector2 begin, Vector2 end, Material mat, float dotLen, float dotGap)
    {
        mat.SetPass(0);

        GL.PushMatrix();
        GL.LoadPixelMatrix(); //转成屏幕坐标

        GL.Begin(GL.LINES);
        while (true)
        {
            Vector2 toEnd = end - begin;
            float sqr = toEnd.sqrMagnitude;
            if(sqr > dotLen * dotLen)
            {
                float len = Mathf.Sqrt(sqr);
                Vector2 dir = toEnd / len;
                Vector2 newEnd = begin + dir * dotLen;
                GL.Vertex3(begin.x, begin.y, 0);
                GL.Vertex3(newEnd.x, newEnd.y, 0);

                if (len > dotLen + dotGap)
                    begin += dir * (dotLen + dotGap);
                else
                    break;
            }
            else
            {
                GL.Vertex3(begin.x, begin.y, 0);
                GL.Vertex3(end.x, end.y, 0);
                break;
            }
        }
        GL.End();

        GL.PopMatrix();
    }

    public static void DrawCircle(Vector2 center, float radius, Material mat, int pointCount = 40)
    {
        mat.SetPass(0);

        GL.PushMatrix();
        GL.LoadPixelMatrix(); //转成屏幕坐标

        GL.Begin(GL.LINES);

        Vector2? last = null, first = null;
        float rad = 2 * Mathf.PI / pointCount;
        for(int i = 0; i < pointCount; i++)
        {
            float x = center.x + Mathf.Cos(rad * i) * radius;
            float y = center.y + Mathf.Sin(rad * i) * radius;

            if (last.HasValue)
            {
                GL.Vertex(last.Value);
                GL.Vertex3(x, y, 0);
            }
            else
            {
                first = new Vector2(x, y);
            }

            last = new Vector2(x, y);
        }

        if (first.HasValue)
        {
            GL.Vertex(last.Value);
            GL.Vertex(first.Value);
        }

        GL.End();

        GL.PopMatrix();
    }

    public static void DrawTriangle(Vector2 v0, Vector2 v1, Vector2 v2, Material mat, bool isFill = false)
    {
        mat.SetPass(0);

        GL.PushMatrix();
        GL.LoadPixelMatrix(); //转成屏幕坐标

        if (isFill)
        {
            GL.Begin(GL.TRIANGLES);
            GL.Vertex(v0);
            GL.Vertex(v1);
            GL.Vertex(v2);
            GL.End();
        }
        else
        {
            GL.Begin(GL.LINES);

            GL.Vertex(v0);
            GL.Vertex(v1);

            GL.Vertex(v1);
            GL.Vertex(v2);

            GL.Vertex(v2);
            GL.Vertex(v0);

            GL.End();
        }
        
        GL.PopMatrix();
    }

    public static void DrawPolygon(List<Vector2> points, Material mat, bool wrap = true, bool isFill = false)
    {
        mat.SetPass(0);

        GL.PushMatrix();
        GL.LoadPixelMatrix(); //转成屏幕坐标

        if (isFill)
        {
            GL.Begin(GL.TRIANGLES);
            for (int i = 2; i < points.Count; i++)
            {
                
                GL.Vertex(points[i - 2]);
                GL.Vertex(points[i - 1]);
                GL.Vertex(points[i]);
            }
            if (wrap)
            {
                GL.Vertex(points[points.Count - 2]);
                GL.Vertex(points[points.Count - 1]);
                GL.Vertex(points[0]);
            }
            GL.End();
        }
        else
        {
            GL.Begin(GL.LINES);

            for (int i = 1; i < points.Count; i++)
            {
                GL.Vertex(points[i - 1]);
                GL.Vertex(points[i]);
            }

            if (wrap)
            {
                GL.Vertex(points[points.Count - 1]);
                GL.Vertex(points[0]);
            }

            GL.End();
        }
        
        GL.PopMatrix();
    }
}