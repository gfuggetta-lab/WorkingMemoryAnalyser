using Godot;
using System;

public partial class BarStimulusNode : Node2D
{
    public float LengthPx { get; set; }
    public float WidthPx { get; set; }
    public float Theta { get; set; }
    public Color FillColor { get; set; }

    public override void _Draw()
    {
        DrawColoredPolygon(CreateBarPoints(LengthPx, WidthPx, Theta), FillColor);
    }

    private static Vector2[] CreateBarPoints(float length, float width, float theta)
    {
        float halfLength = length / 2.0f;
        float halfWidth = width / 2.0f;

        return new[]
        {
            RotateZ(new Vector2(-halfLength, halfWidth), theta),
            RotateZ(new Vector2(-halfLength, -halfWidth), theta),
            RotateZ(new Vector2(halfLength, -halfWidth), theta),
            RotateZ(new Vector2(halfLength, halfWidth), theta)
        };
    }

    private static Vector2 RotateZ(Vector2 point, float theta)
    {
        float cos = MathF.Cos(theta);
        float sin = MathF.Sin(theta);
        return new Vector2(
            point.X * cos - point.Y * sin,
            point.X * sin + point.Y * cos);
    }
}
