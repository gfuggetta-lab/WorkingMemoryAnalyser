using Godot;
using System;

// Circle based geomtry.
// It's drive by the numer of the sides that needs to be drawn
// i.e. 3 gives triangle, 6 gives hex, etc
public partial class RegularShapeStimulusNode : Node2D
{
    public float OuterRadiusPx { get; set; }
    public float LineWidthPx { get; set; }
    public int PointCount { get; set; }
    public bool Filled { get; set; }
    public float RotationRadians { get; set; }
    public Color FillColor { get; set; }

    public override void _Draw()
    {
        if (PointCount < 3 || OuterRadiusPx <= 0.0f)
            return;

        Vector2[] outer = GetCircleCoords(OuterRadiusPx, PointCount, RotationRadians);
        if (Filled)
        {
            DrawColoredPolygon(outer, FillColor);
            return;
        }

        float innerRadius = Math.Max(0.0f, OuterRadiusPx - LineWidthPx);
        Vector2[] inner = GetCircleCoords(innerRadius, PointCount, RotationRadians);
        for (int i = 0; i < PointCount; i++)
        {
            int next = (i + 1) % PointCount;
            DrawColoredPolygon(
                new[] { outer[i], inner[i], inner[next], outer[next] },
                FillColor);
        }
    }

    private static Vector2[] GetCircleCoords(float radius, int pointCount, float rotation)
    {
        var points = new Vector2[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            float theta = (MathF.PI * 2.0f / pointCount) * i;
            points[i] = RotateZ(
                new Vector2(radius * MathF.Sin(theta), radius * MathF.Cos(theta)),
                rotation);
        }
        return points;
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
