using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class TextStimulusNode : Node2D
{
    public Font Font { get; set; }
    public string Text { get; set; }
    public Vector2 TextPosition { get; set; }
    public Color TextColor { get; set; }
    public int FontSize { get; set; }

    public override void _Draw()
    {
        if ((Font == null) || string.IsNullOrEmpty(Text))
            return;

        DrawString(
            Font,
            TextPosition,
            Text,
            HorizontalAlignment.Left,
            modulate: TextColor,
            fontSize: FontSize);
    }
}
