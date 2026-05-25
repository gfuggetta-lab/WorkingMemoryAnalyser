using Godot;

public partial class PlusStimulusNode : Node2D
{
    public float LengthPx { get; set; }
    public float WidthPx { get; set; }
    public Color FillColor { get; set; }

    public override void _Draw()
    {
        DrawColoredPolygon(CreateBarPoints(LengthPx, WidthPx), FillColor);
        DrawColoredPolygon(CreateBarPoints(WidthPx, LengthPx), FillColor);
    }

    private static Vector2[] CreateBarPoints(float length, float width)
    {
        float halfLength = length / 2.0f;
        float halfWidth = width / 2.0f;

        return new[]
        {
            new Vector2(-halfLength, halfWidth),
            new Vector2(-halfLength, -halfWidth),
            new Vector2(halfLength, -halfWidth),
            new Vector2(halfLength, halfWidth)
        };
    }
}
