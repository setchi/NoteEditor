using UnityEngine;

public class ColoringRect
{
    public Color color;
    public Vector2 max;
    public Vector2 min;

    public ColoringRect(Vector2 min, Vector2 max, Color color)
    {
        this.color = color;
        this.max = max;
        this.min = min;
    }
}
