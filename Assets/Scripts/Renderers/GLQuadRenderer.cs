using System.Collections.Generic;
using UnityEngine;

public class GLQuadRenderer : SingletonGameObject<GLQuadRenderer>
{
    [SerializeField]
    Material mat;
    public Dictionary<string, Geometry[]> drawData = new Dictionary<string, Geometry[]>();

    void OnRenderObject()
    {
        GL.PushMatrix();
        mat.SetPass(0);
        GL.LoadPixelMatrix();
        GL.Begin(GL.QUADS);

        foreach (var quads in drawData.Values)
        {
            foreach (var quad in quads)
            {
                GL.Color(quad.color);

                foreach (var vertex in quad.vertices)
                {
                    GL.Vertex(vertex);
                }
            }
        }

        GL.End();
        GL.PopMatrix();
    }

    public static void Render(string key, Geometry[] drawData)
    {
        Instance.drawData[key] = drawData;
    }
}
