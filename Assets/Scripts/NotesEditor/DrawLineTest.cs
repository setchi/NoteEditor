using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DrawLineTest : MonoBehaviour
{
    Dictionary<string, Line[]> drawLines = new Dictionary<string, Line[]>();

    IEnumerator Start()
    {
        var lineMaterial = new Material("Shader \"Lines/Colored Blended\" {SubShader { Pass { BindChannels { Bind \"Color\",color } Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off Fog { Mode Off } } } }");
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

    public void DrawLines(string key, Line[] lines)
    {
        if (drawLines.ContainsKey(key))
        {
            drawLines[key] = lines;
        }
        else
        {
            drawLines.Add(key, lines);
        }
    }
}
