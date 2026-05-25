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
				Name = "CircleHollow",
				Position = pos,
				Closed = true,
				DefaultColor = color,
				Width = lineWidth,
				Antialiased = true,
				Points = CreateCirclePoints(radius)
			};
		}


		public static Sprite2D CreateImageNode(Texture2D texture, Vector2 pos, float sizePx)
		{
			var sprite = new Sprite2D
			{
				Name = "Image",
				Texture = texture,
				Centered = true,
				Position = pos
			};
			Vector2 texSize = texture.GetSize();
			if ((texSize.X > 0.0f) && (texSize.Y > 0.0f))
				sprite.Scale = new Vector2(sizePx / texSize.X, sizePx / texSize.Y);
			return sprite;
		}

		public static BarStimulusNode CreateBarNode(Vector2 pos, float lengthPx, float widthPx, float theta, Color color)
		{
			return new BarStimulusNode
			{
				Name = "Bar",
				Position = pos,
				LengthPx = lengthPx,
				WidthPx = widthPx,
				Theta = theta,
				FillColor = color
			};
		}

		public static PlusStimulusNode CreatePlusNode(Vector2 pos, float lengthPx, float widthPx, Color color)
		{
			return new PlusStimulusNode
			{
				Name = "Plus",
				Position = pos,
				LengthPx = lengthPx,
				WidthPx = widthPx,
				FillColor = color
			};
		}

		public static StarStimulusNode CreateStarNode(Vector2 pos, float outerRadiusPx, Color color)
		{
			return new StarStimulusNode
			{
				Name = "Star",
				Position = pos,
				OuterRadiusPx = outerRadiusPx,
				FillColor = color
			};
		}

		public static RegularShapeStimulusNode CreateRegularShapeNode(Vector2 pos, float outerRadiusPx, float lineWidthPx, int pointCount, bool filled, float rotation, Color color)
		{
			return new RegularShapeStimulusNode
			{
				Name = "RegularShape",
				Position = pos,
				OuterRadiusPx = outerRadiusPx,
				LineWidthPx = lineWidthPx,
				PointCount = pointCount,
				Filled = filled,
				RotationRadians = rotation,
				FillColor = color
			};
		}

		public static Polygon2D CreateFilledCircleNode(Vector2 pos, float radius, Color color)
		{
			return new Polygon2D
			{
				Name = "CircleFilled",
				Position = pos,
				Color = color,
				Polygon = CreateCirclePoints(radius)
			};
		}

		public static void log(string message)
		{
			var stamp = DateTime.Now.ToString("HH:mm:ss.fff");
			GD.Print($"{stamp}: {message}");
		}

		public static bool AddPhysicalKeyToAction(string actionName, string keyText)
		{
			if (string.IsNullOrWhiteSpace(keyText))
				return false;

			char ch = keyText.Trim()[0];

			if (!TryCharToGodotKey(ch, out Key key))
			{
				GD.Print($"failed to map char '{ch}' to godot");
				return false;
			}

			StringName action = new StringName(actionName);

			if (!InputMap.HasAction(action))
			{
				GD.Print($"there's no action: '{actionName}'");
				return false;
			}

			var ev = new InputEventKey
			{
				PhysicalKeycode = key
			};

			// Do not add duplicate binding.
			if (!InputMap.ActionHasEvent(action, ev))
				InputMap.ActionAddEvent(action, ev);

			return true;
		}

		public static bool TryCharToGodotKey(char ch, out Key key)
		{
			ch = char.ToUpperInvariant(ch);

			// For letters: 'A' -> Key.A, 'D' -> Key.D, etc.
			if (ch >= 'A' && ch <= 'Z')
			{
				key = (Key)Enum.Parse(typeof(Key), ch.ToString());
				return true;
			}

			key = Key.None;
			return false;
		}
	}
}
