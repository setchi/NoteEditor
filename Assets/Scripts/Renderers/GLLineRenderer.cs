using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GLLineRenderer : SingletonGameObject<GLLineRenderer>
{
    [SerializeField]
    Material mat;
    Dictionary<string, Line[]> drawLines = new Dictionary<string, Line[]>();

    void OnRenderObject()
    {
        GL.PushMatrix();
        mat.SetPass(0);
        GL.LoadPixelMatrix();
        GL.Begin(GL.LINES);

        foreach (var lines in drawLines.Values)
        {
            foreach (var l in lines)
            {
                GL.Color(l.color);
                GL.Vertex3(l.start.x, l.start.y, 0);
                GL.Vertex3(l.end.x, l.end.y, 0);
            }
        }

        GL.End();
        GL.PopMatrix();
        drawLines.Clear();
    }

    public static void Render(string key, Line[] lines)
    {
        if (Instance.drawLines.ContainsKey(key))
        {
            Instance.drawLines[key] = lines;
        }
        else
        {
            Instance.drawLines.Add(key, lines);
        }
    }
}
