using System.Collections.Generic;
using UnityEngine;

public class GLPolygonRenderer : SingletonGameObject<GLPolygonRenderer>
{
    [SerializeField]
    Material mat;
    Dictionary<string, Polygon[]> drawPolygons = new Dictionary<string, Polygon[]>();

    void OnRenderObject()
    {
        GL.PushMatrix();
        mat.SetPass(0);
        GL.LoadPixelMatrix();
        GL.Begin(GL.QUADS);

        foreach (var polygons in drawPolygons.Values)
        {
            foreach (var polygon in polygons)
            {
                GL.Color(polygon.color);

                foreach (var vertex in polygon.vertex)
                {
                    GL.Vertex(vertex);
                }
            }
        }

        GL.End();
        GL.PopMatrix();
    }

    public static void Render(string key, Polygon[] polygons)
    {
        if (Instance.drawPolygons.ContainsKey(key))
        {
            Instance.drawPolygons[key] = polygons;
        }
        else
        {
            Instance.drawPolygons.Add(key, polygons);
        }
    }
}
