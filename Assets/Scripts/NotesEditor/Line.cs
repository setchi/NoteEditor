using UnityEngine;

public class Line
{

    public Color color;
    public Vector3 start;
    public Vector3 end;

    public Line() { }
    public Line(Vector3 start, Vector3 end, Color color)
    {
        this.color = color;
        this.start = start;
        this.end = end;
    }
}
