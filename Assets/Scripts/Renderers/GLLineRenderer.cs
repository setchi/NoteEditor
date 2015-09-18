using System.Collections.Generic;
using UnityEngine;

public class GLLineRenderer : SingletonGameObject<GLLineRenderer>
{
    [SerializeField]
    Material mat;
    Dictionary<string, Line[]> drawData = new Dictionary<string, Line[]>();

    void OnRenderObject()
    {
        GL.PushMatrix();
        mat.SetPass(0);
        GL.LoadPixelMatrix();
        GL.Begin(GL.LINES);

        foreach (var lines in drawData.Values)
        {
            foreach (var line in lines)
            {
                GL.Color(line.color);
                GL.Vertex(line.start);
                GL.Vertex(line.end);
            }
        }

        GL.End();
        GL.PopMatrix();
        drawData.Clear();
    }

    public static void Render(string key, Line[] drawData)
    {
        Instance.drawData[key] = drawData;
    }
}
