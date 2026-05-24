using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using WMAData;

namespace godot
{
	public static class WMAUtils
	{
		public static Godot.Color GDColor(WMAData.ColorFloat clr)
		{
			return new Color((float)clr.r, (float)clr.g, (float)clr.b);
		}

		public static Vector2[] CreateCirclePoints(float radius)
		{
			const int MinSegments = 48;
			const float PixelsPerSegment = 4.0f;
			int segments = Math.Max(MinSegments, (int)Math.Ceiling((2.0f * Math.PI * radius) / PixelsPerSegment));
			var points = new Vector2[segments];
			for (int i = 0; i < segments; i++)
			{
				double angle = 2.0 * Math.PI * i / segments;
				points[i] = new Vector2(
					(float)(Math.Cos(angle) * radius),
					(float)(Math.Sin(angle) * radius));
			}
			return points;
		}

		public static Line2D CreateHollowCircleNode(Vector2 pos, float radius, float lineWidth, Color color)
		{
			return new Line2D
			{
				Name = "CircleHollowStimulus",
				Position = pos,
				Closed = true,
				DefaultColor = color,
				Width = lineWidth,
				Antialiased = true,
				Points = CreateCirclePoints(radius)
			};
		}
	}
}
