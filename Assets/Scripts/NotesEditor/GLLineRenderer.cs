using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GLLineRenderer : SingletonGameObject<GLLineRenderer>
{
    [SerializeField]
    Material lineMaterial;
    Dictionary<string, Line[]> drawLines = new Dictionary<string, Line[]>();

    IEnumerator Start()
    {
        lineMaterial.hideFlags = HideFlags.HideAndDontSave;
        lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;

        while (true)
        {
            yield return new WaitForEndOfFrame();

            lineMaterial.SetPass(0);
            GL.Begin(GL.LINES);

            foreach (var lines in drawLines.Values)
            {
                foreach (var l in lines)
                {
                    GL.Color(l.color);
                    GL.Vertex3(l.start.x, l.start.y, l.start.z);
                    GL.Vertex3(l.end.x, l.end.y, l.end.z);
                }
            }

            GL.End();
        }
    }

    public static void RenderLines(string key, Line[] lines)
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
