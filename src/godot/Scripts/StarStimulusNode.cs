using Godot;
using System;

public partial class StarStimulusNode : Node2D
{
    public float OuterRadiusPx { get; set; }
    public Color FillColor { get; set; }

    public override void _Draw()
    {
        Vector2[] outer = GetCircleCoords(OuterRadiusPx, 5);

        DrawTriangle(outer[2], outer[0]);
        DrawTriangle(outer[0], outer[3]);
        DrawTriangle(outer[1], outer[4]);
        DrawTriangle(outer[4], outer[2]);
        DrawTriangle(outer[3], outer[1]);
    }

    private void DrawTriangle(Vector2 p1, Vector2 p2)
    {
        DrawColoredPolygon(new[] { Vector2.Zero, p1, p2 }, FillColor);
    }

    private static Vector2[] GetCircleCoords(float radius, int pointCount)
    {
        var points = new Vector2[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            float theta = (MathF.PI * 2.0f / pointCount) * i;
            points[i] = new Vector2(
                radius * MathF.Sin(theta),
                -radius * MathF.Cos(theta)
                );
        }
        return points;
    }
}
