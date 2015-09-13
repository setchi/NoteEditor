using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GLRectRenderer : SingletonGameObject<GLRectRenderer>
{
    [SerializeField]
    Material mat;
    Dictionary<string, ColoringRect[]> drawRects = new Dictionary<string, ColoringRect[]>();

    IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();

            GL.PushMatrix();
            mat.SetPass(0);
            GL.LoadPixelMatrix();
            GL.Begin(GL.QUADS);

            foreach (var rects in drawRects.Values)
            {
                foreach (var r in rects)
                {
                    GL.Color(r.color);
                    GL.Vertex3(r.min.x, r.max.y, 0);
                    GL.Vertex3(r.max.x, r.max.y, 0);
                    GL.Vertex3(r.max.x, r.min.y, 0);
                    GL.Vertex3(r.min.x, r.min.y, 0);
                }
            }

            GL.End();
            GL.PopMatrix();

        }
    }

    public static void Render(string key, ColoringRect[] rect)
    {
        if (Instance.drawRects.ContainsKey(key))
        {
            Instance.drawRects[key] = rect;
        }
        else
        {
            Instance.drawRects.Add(key, rect);
        }
    }
}
