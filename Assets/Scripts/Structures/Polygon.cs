using UnityEngine;

public class Polygon
{
    public Color color;
    public Vector3[] vertex;

    public Polygon(Vector3[] vertex, Color color)
    {
        this.color = color;
        this.vertex = vertex;
    }
}
