using NoteEditor.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace NoteEditor.GLDrawing
{
    public class GLLineDrawer : SingletonMonoBehaviour<GLLineDrawer>
    {
        [SerializeField]
        Material mat;
        List<Line> drawData = new List<Line>();

        static int size = 0;
        static int maxSize = 0;

        void OnRenderObject()
        {
            GL.PushMatrix();
            mat.SetPass(0);
            GL.LoadPixelMatrix();
            GL.Begin(GL.LINES);

            if (size * 2 < maxSize)
            {
                drawData.RemoveRange(size, maxSize - size);
                maxSize = size;
            }

            for (int i = 0; i < size; i++)
            {
                var line = drawData[i];
                GL.Color(line.color);
                GL.Vertex(line.start);
                GL.Vertex(line.end);
            }

            GL.End();
            GL.PopMatrix();
            size = 0;
        }

        public static void Draw(Line[] lines)
        {
            foreach (var line in lines)
            {
                Draw(line);
            }
        }

        public static void Draw(Line line)
        {
            if (size < maxSize)
            {
                Instance.drawData[size] = line;
            }
            else
            {
                Instance.drawData.Add(line);
                maxSize++;
            }

            size++;
        }
    }
}
