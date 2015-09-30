using UnityEngine;

namespace NoteEditor.GLDrawing
{
    public class Geometry
    {
        public Color color;
        public Vector3[] vertices;

        public Geometry(Vector3[] vertices, Color color)
        {
            this.color = color;
            this.vertices = vertices;
        }
    }
}
