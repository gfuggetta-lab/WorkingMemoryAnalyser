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
	Dictionary<string, Texture2D> texs = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
	Dictionary<string, Font> fonts = new Dictionary<string, Font>(StringComparer.OrdinalIgnoreCase);
	Dictionary<string, AudioStream> sounds = new Dictionary<string, AudioStream>(StringComparer.OrdinalIgnoreCase);



	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Configuration exam = new Configuration();
		// there's no configuration for the clear color
		RenderingServer.SetDefaultClearColor(new Color(0f, 0f, 0f));
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
			QueueRedraw();
		}
		PlaySoundIfAny(fadeTrig);
	}

	public static Vector2 GetPos(PlayItemPos pos, Vector2 center, double distance)
	{
		Vector2 res = center;
		if ((pos == PlayItemPos.Center) || (pos == PlayItemPos.Other))
			return res;

		float cospi = (float)((double)distance * Math.Cos(Math.PI / 4.0));
		float sinpi = (float)((double)distance * Math.Sin(Math.PI / 4.0));
		switch (pos)
		{
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

	protected virtual void DrawText(
		// the item of text
		PlayItem itm, 
		// the central position
		Vector2 pos)
	{
		if (!fonts.TryGetValue(itm.fontName, out var fnt))
		{
			GD.Print($"Font not found: {itm.fontName}");
			return;
		}
		int fontSize = itm.fontSizePx;
		Vector2 size = fnt.GetStringSize(
			itm.text, HorizontalAlignment.Left, -1, fontSize);
		float ascent = fnt.GetAscent(fontSize);
		float descent = fnt.GetDescent(fontSize);
		pos.X -= size.X / 2.0f;
		pos.Y += (ascent - descent) / 2.0f;

		DrawString(
			fnt, pos, itm.text,
			HorizontalAlignment.Left,
			modulate: GDColor(itm.color),
			fontSize: fontSize
		);
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

	public override void _Draw()
	{
		var cpos = GetViewportRect().Size / 2.0f;
		foreach (var itm in drawItems)
		{
			var pos = GetPos(itm.pos, cpos, itm.posDistanceCm * cmToPix);
			switch (itm.itemType)
			{
				case PlayItemType.Text:
					DrawText(itm, pos);
					break;

				case PlayItemType.ImageById:
					string n;
					n = $"{itm.imageId}.bmp";
					if (texs.TryGetValue(n, out var tt))
					{
						float w = (float)(itm.sizeCm * cmToPix);
						Rect2 rr = new Rect2(pos.X - w / 2.0f, pos.Y - w / 2.0f, w, w);
						DrawTextureRect(tt, rr, false);
					}
					else
						GD.Print($"image not found: {n}; {itm.imageId}");
					break;

				case PlayItemType.CircleFilled:
				case PlayItemType.CircleHollow:
					float r = (float)(itm.radiusCm * cmToPix);
					if (itm.itemType == PlayItemType.CircleFilled)
						DrawCircle(pos, r, GDColor(itm.color), true, -1, true);
					else
					{
						var lw = (float)(itm.lineWidthCm * cmToPix);
						DrawCircle(pos, r, GDColor(itm.color), false, lw, true);
					}
					break;
			}
		}
	}
}
