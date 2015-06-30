using System.Collections;
using UnityEngine;


public class DrawLineTest : MonoBehaviour
{
    Line[] lines;

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

            foreach (var l in lines)
            {
                GL.Color(l.color);
                GL.Vertex3(l.start.x, l.start.y, l.start.z);
                GL.Vertex3(l.end.x, l.end.y, l.end.z);
            }

            GL.End();
        }
    }

    public void DrawLines(Line[] lines)
    {
        this.lines = lines;
    }
}
