using Godot;
using MonitorInfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using WMAData;
using WMAFiles;
using static godot.WMAUtils;
using ConfigFile = WMAFiles.ConfigFile;

public partial class BootScript : Node2D
{
	[Export]
	public Label screenRes;

	[Export]
	public string fileName;

	[Export]
	AudioStreamPlayer2D soundPlayer;


	double cmToPix = 0.0f;

	PlayList playList = new PlayList();
	PlayListTracker plrTrack;
	List<PlayItem> drawItems = new List<PlayItem>();
	Node2D drawRoot;
	readonly List<Node> drawNodes = new List<Node>();
	Dictionary<string, Texture2D> texs = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
	Dictionary<string, Font> fonts = new Dictionary<string, Font>(StringComparer.OrdinalIgnoreCase);
	Dictionary<string, AudioStream> sounds = new Dictionary<string, AudioStream>(StringComparer.OrdinalIgnoreCase);



	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Configuration exam = new Configuration();
		// there's no configuration for the clear color
		RenderingServer.SetDefaultClearColor(new Color(0f, 0f, 0f));
		drawRoot = new Node2D
		{
			Name = "StimulusNodes"
		};
		AddChild(drawRoot);

		if (File.Exists(fileName))
		{
			var cfg = ConfigFile.FromFile(fileName);
			exam.LoadConfig(cfg);
		}

		TrialMonitor tm = new TrialMonitor();
		if (screenRes != null)
		{
			var mons = MonitorEnumerator.GetConnectedMonitors();
			StringBuilder b = new StringBuilder();
			foreach(var m in mons)
			{
				float widthIn = (float)m.PhysWidthMM / 25.4f;
				float physdpi = (float)m.PixelWidth / widthIn;
				b.Append($"physSize: {m.PhysWidthMM}x{m.PhysHeightMM}; Res:{m.PixelWidth}x{m.PixelHeight};  " +
					$"SysDpi: {m.Dpi}; PhysDpi: {physdpi}");
				cmToPix = m.PixelWidth / (m.PhysWidthMM / 10.0);
				GD.Print($"cmToPix: {cmToPix}; pixels:{m.PixelWidth}; cm: {(m.PhysWidthMM / 10.0)}");
				tm.widthPx = m.PixelWidth;
				tm.heightPx = m.PixelHeight;
				tm.widthCm = m.PhysWidthMM / 10.0;
				tm.heightCm = m.PhysHeightMM / 10.0;
			}
			screenRes.Text = b.ToString();
		}
		string inp = @"C:\FPC_Laz\WorkingMemoryAnalyser_pas\Experiments library\TestExp\Input data\InputData_1.txt";
		var list = InputDataHelper.LoadTrials(inp);
		
		

		exam.Schedule(tm, list, playList);
		GD.Print($"trials:  {list.Count}");
		GD.Print($"actions: {playList.items.Count}");

		drawItems.Clear();
		plrTrack = new PlayListTracker(playList);
		plrTrack.Track(0, drawItems, null, null, null);

		GD.Print($"init: {drawItems.Count}");

		string imgDir = Path.GetDirectoryName(inp);
		imgDir = Path.GetDirectoryName(imgDir);
		Preload(exam, imgDir, list);

		RebuildDrawNodes();
		PlaySoundIfAny(drawItems);
	}

	private void Preload(Configuration exam, string expDir, List<TrialOrder> list)
	{
		string imgDir = Path.Combine(expDir, "Stimulus images");
		
		List<string> resNames = new List<string>();
		
		exam.GetPreloadImages(list, resNames);
		GD.Print($"images: {resNames.Count}");
		foreach (var nm in resNames)
		{
			string bmpFn = Path.Combine(imgDir, nm);
			Image img = new Image();
			GD.Print($"loading: {bmpFn}");
			var err = img.Load(bmpFn);
			if (err != 0)
			{
				GD.Print($"loading image from {bmpFn} failed: {err}");
				continue;
			}
			var _tex = ImageTexture.CreateFromImage(img);
			GD.Print($"loaded: {nm} {_tex != null}");
			texs[nm] = _tex;
		}

		// loading fonts
		resNames.Clear();
		exam.GetPreloadFonts(resNames);
		GD.Print($"fonts: {resNames.Count}");
		foreach(var fn in resNames)
		{
			if (string.IsNullOrWhiteSpace(fn))
				continue;
			if (fonts.ContainsKey(fn))
				continue;
			GD.Print($"preloading: {fn}");
			FontFile ff = GD.Load<FontFile>($"res://Fonts/{fn}");
			if (ff != null)
				fonts[fn] = ff;
		}

		resNames.Clear();

		string audDir = Path.Combine(expDir, "Stimulus sounds");
		exam.GetPreloadSounds(list, resNames);
		GD.Print($"sounds: {resNames.Count}");
		foreach (var fn in resNames)
		{
			if (string.IsNullOrWhiteSpace(fn))
				continue;
			
			GD.Print($"preloading: {fn}");
			string audFn = Path.Combine(audDir, fn);
			if (!File.Exists(audFn))
			{
				GD.Print($"file doesn't exist: {audFn}");
				continue;
			}
			var wav = AudioStreamWav.LoadFromFile(audFn);
			sounds[fn] = wav;
		}

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		List<PlayItem> eff = new List<PlayItem>();
		List<PlayItem> fadeTrig = new List<PlayItem>();
		int cnt = plrTrack.Track(delta*1000.0, eff, null, null, fadeTrig);
		if (cnt != 0)
		{
			drawItems = eff;
			GD.Print($"cnt: {cnt}; {plrTrack.lastMs}; drawItems: {eff.Count}");
			RebuildDrawNodes();
		}
		PlaySoundIfAny(fadeTrig);
	}

	public static Vector2 GetPos(PlayItemPos pos, Vector2 center, double distance, int posVal, int posCount)
	{
		Vector2 res = center;
		if ((pos == PlayItemPos.Center) || (pos == PlayItemPos.Other))
			return res;

		float cospi = (float)((double)distance * Math.Cos(Math.PI / 4.0));
		float sinpi = (float)((double)distance * Math.Sin(Math.PI / 4.0));
		switch (pos)
		{
			case PlayItemPos.OneOfCount:
				double diff = (double)posVal * (Math.PI * 2 / (double)posCount);
				res.X += (float)((double)distance * Math.Cos(Math.PI / 4.0 + diff));
				res.Y += (float)((double)distance * Math.Sin(Math.PI / 4.0 + diff));
				break;
			case PlayItemPos.NW:
				res.X -= cospi;
				res.Y += sinpi;
				break;
			case PlayItemPos.NE:
				res.X += cospi;
				res.Y += sinpi;
				break;
			case PlayItemPos.SE:
				res.X += cospi;
				res.Y -= sinpi;
				break;
			case PlayItemPos.SW:
				res.X -= cospi;
				res.Y -= sinpi;
				break;
		}
		return res;

	}

	protected virtual Node2D CreateTextNode(
		// the item of text
		PlayItem itm, 
		// the central position
		Vector2 pos)
	{
		if (!fonts.TryGetValue(itm.fontName, out var fnt))
		{
			GD.Print($"Font not found: {itm.fontName}");
			return null;
		}
		int fontSize = itm.fontSizePx;
		Vector2 size = fnt.GetStringSize(itm.text, HorizontalAlignment.Left, -1, fontSize);
		float ascent = fnt.GetAscent(fontSize);
		float descent = fnt.GetDescent(fontSize);
		Vector2 textPos = new Vector2(-size.X / 2.0f, (ascent - descent) / 2.0f);

		return new TextStimulusNode
		{
			Name = "TextStimulus",
			Position = pos,
			Font = fnt,
			Text = itm.text,
			TextPosition = textPos,
			TextColor = GDColor(itm.color),
			FontSize = fontSize
		};
	}

	protected void PlaySound(PlayItem itm)
	{
		string snd = itm.soundId;
		GD.Print($"playing sound: {snd}");
		if (!sounds.TryGetValue(snd, out var strm))
		{
			sounds.TryGetValue($"{snd}.wav", out strm);
		}
		if (strm == null) 
			return;

		if (soundPlayer.Playing)
			soundPlayer.Stop();
		soundPlayer.Stream = strm;
		GD.Print($"start!");
		soundPlayer.Play();
	}

	private void PlaySoundIfAny(List<PlayItem> items)
	{
		if (soundPlayer == null) return;

		if (items == null) return;
		foreach(var itm in items)
		{
			GD.Print($"{itm.itemType}");
			if (itm.itemType == PlayItemType.Sound)
			{
				GD.Print("sound found!");
				PlaySound(itm);
			}
		}
	}

	private void ClearDrawNodes()
	{
		foreach (var node in drawNodes)
		{
			if (!GodotObject.IsInstanceValid(node))
				continue;
			if (node.GetParent() == drawRoot)
				drawRoot.RemoveChild(node);
			node.QueueFree();
		}
		drawNodes.Clear();
	}

	private void TrackDrawNode(Node node)
	{
		if (node == null)
			return;
		drawRoot.AddChild(node);
		drawNodes.Add(node);
	}

	private void RebuildDrawNodes()
	{
		if (drawRoot == null)
			return;

		ClearDrawNodes();
		var cpos = GetViewportRect().Size / 2.0f;
		foreach (var itm in drawItems)
		{
			var pos = GetPos(itm.pos, cpos, itm.posDistanceCm * cmToPix, itm.posOther, itm.posCount);
			Node2D node = null;
			switch (itm.itemType)
			{
				case PlayItemType.Text:
					node = CreateTextNode(itm, pos);
					break;

				case PlayItemType.ImageById:
					string n;
					n = $"{itm.imageId}.bmp";
					if (texs.TryGetValue(n, out var tt))
					{
						float w = (float)(itm.sizeCm * cmToPix);
						node = CreateImageNode(tt, pos, w);
					}
					else
						GD.Print($"image not found: {n}; {itm.imageId}");
					break;

				case PlayItemType.CircleFilled:
				case PlayItemType.CircleHollow:
					float r = (float)(itm.radiusCm * cmToPix);
					if (itm.itemType == PlayItemType.CircleFilled)
						node = CreateFilledCircleNode(pos, r, GDColor(itm.color));
					else
					{
						var lw = (float)(itm.lineWidthCm * cmToPix);
						node = CreateHollowCircleNode(pos, r, lw, GDColor(itm.color));
					}
					break;
			}
			if (node != null)
			{
				node.ZIndex = itm.drawOrder;
				TrackDrawNode(node);
			}
		}
	}

	private static Sprite2D CreateImageNode(Texture2D texture, Vector2 pos, float sizePx)
	{
		var sprite = new Sprite2D
		{
			Name = "ImageStimulus",
			Texture = texture,
			Centered = true,
			Position = pos
		};
		Vector2 texSize = texture.GetSize();
		if ((texSize.X > 0.0f) && (texSize.Y > 0.0f))
			sprite.Scale = new Vector2(sizePx / texSize.X, sizePx / texSize.Y);
		return sprite;
	}

	private static Polygon2D CreateFilledCircleNode(Vector2 pos, float radius, Color color)
	{
		return new Polygon2D
		{
			Name = "CircleFilledStimulus",
			Position = pos,
			Color = color,
			Polygon = CreateCirclePoints(radius)
		};
	}

	private static Line2D CreateHollowCircleNode(Vector2 pos, float radius, float lineWidth, Color color)
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

	private static Vector2[] CreateCirclePoints(float radius)
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

	private partial class TextStimulusNode : Node2D
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
}
