using System.Collections.Generic;
using UnityEngine;

public class GLQuadRenderer : SingletonGameObject<GLQuadRenderer>
{
    [SerializeField]
    Material mat;
    List<Geometry> drawData = new List<Geometry>();

    static int size = 0;
    static int maxSize = 0;

    void OnRenderObject()
    {
        GL.PushMatrix();
        mat.SetPass(0);
        GL.LoadPixelMatrix();
        GL.Begin(GL.QUADS);

        if (size * 2 < maxSize)
        {
            drawData.RemoveRange(size, maxSize - size);
            maxSize = size;
        }

        for (int i = 0; i < size; i++)
        {
            GL.Color(drawData[i].color);

            foreach (var vertex in drawData[i].vertices)
            {
                GL.Vertex(vertex);
            }
        }

        GL.End();
        GL.PopMatrix();
        size = 0;
    }

    public static void Render(Geometry[] quads)
    {
        foreach (var quad in quads)
        {
            Render(quad);
        }
    }

    public static void Render(Geometry quad)
    {
        if (size < maxSize)
        {
            Instance.drawData[size] = quad;
        }
        else
        {
            Instance.drawData.Add(quad);
            maxSize++;
        }

        size++;
    }
}
