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
	public Label sectionInfo;

	[Export]
	public string fileName;

	[Export]
	AudioStreamPlayer2D soundPlayer;


	double cmToPix = 0.0f;

	PlayList playList = new PlayList();
	PlayListTracker plrTrack;
	List<PlayItem> drawItems = new List<PlayItem>();
	PlayItem currentSection;
	double currentSectionEndMs = -1.0;
	Node2D drawRoot;
	readonly List<Node> drawNodes = new List<Node>();
	Dictionary<string, Texture2D> texs = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
	Dictionary<string, Font> fonts = new Dictionary<string, Font>(StringComparer.OrdinalIgnoreCase);
	Dictionary<string, AudioStream> sounds = new Dictionary<string, AudioStream>(StringComparer.OrdinalIgnoreCase);


	public bool isWaitingResponse = false;


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
		if (sectionInfo != null)
			sectionInfo.ZIndex = 1000;

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
				//GD.Print($"cmToPix: {cmToPix}; pixels:{m.PixelWidth}; cm: {(m.PhysWidthMM / 10.0)}");
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
		log($"trials:  {list.Count}");

		drawItems.Clear();
		plrTrack = new PlayListTracker(playList);
		plrTrack.Track(0, drawItems, null, null, null);

		string imgDir = Path.GetDirectoryName(inp);
		imgDir = Path.GetDirectoryName(imgDir);
		Preload(exam, imgDir, list);

		RebuildDrawNodes();
		UpdateSectionInfo();
		PlaySoundIfAny(drawItems);
	}

	private void Preload(Configuration exam, string expDir, List<TrialOrder> list)
	{
		string imgDir = Path.Combine(expDir, "Stimulus images");
		
		List<string> resNames = new List<string>();
		
		exam.GetPreloadImages(list, resNames);
		
		List<string> tryExt = new List<string>();
		tryExt.Add(".png");
		tryExt.Add(".bmp");
		foreach (var nm in resNames)
		{
			string ext = Path.GetExtension(nm);
			bool doTryExt = string.IsNullOrEmpty(ext);

			string bmpFn = Path.Combine(imgDir, nm);
			if (!File.Exists(bmpFn)&&doTryExt)
			{
				foreach (var x in tryExt)
				{
					string newfn = Path.ChangeExtension(bmpFn, x);
					if (File.Exists(newfn))
					{
						bmpFn = newfn;
						break;
					}
				}
			}


			Image img = new Image();
			var err = img.Load(bmpFn);
			if (err != 0)
			{
				log($"loading image from {bmpFn} failed: {err}");
				continue;
			}
			var _tex = ImageTexture.CreateFromImage(img);
			//GD.Print($"loaded: {nm} {_tex != null}");
			texs[nm] = _tex;
		}

		// loading fonts
		resNames.Clear();
		exam.GetPreloadFonts(resNames);
		foreach(var fn in resNames)
		{
			if (string.IsNullOrWhiteSpace(fn))
				continue;
			if (fonts.ContainsKey(fn))
				continue;
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
		List<PlayItem> trigAndOff = new List<PlayItem>();
		List<PlayItem> offList = new List<PlayItem>();
		int cnt = plrTrack.Track(delta*1000.0, eff, null, offList, trigAndOff);
		if (cnt != 0)
		{
			drawItems = eff;
			//GD.Print($"cnt: {cnt}; {plrTrack.lastMs}; drawItems: {eff.Count}");
			RebuildDrawNodes();
		}
		UpdateSectionInfo();
		PlaySoundIfAny(trigAndOff);
		StopReadResponse(offList);
		StopReadResponse(trigAndOff);
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
				res.X -= (float)((double)distance * Math.Cos(Math.PI / 4.0 + diff));
				res.Y -= (float)((double)distance * Math.Sin(Math.PI / 4.0 + diff));
				break;
			case PlayItemPos.NW:
				res.X -= cospi;
				res.Y -= sinpi;
				break;
			case PlayItemPos.NE:
				res.X += cospi;
				res.Y -= sinpi;
				break;
			case PlayItemPos.SE:
				res.X += cospi;
				res.Y += sinpi;
				break;
			case PlayItemPos.SW:
				res.X -= cospi;
				res.Y += sinpi;
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
		log($"playing sound: {snd}");
		if (!sounds.TryGetValue(snd, out var strm))
		{
			sounds.TryGetValue($"{snd}.wav", out strm);
		}
		if (strm == null) 
			return;

		if (soundPlayer.Playing)
			soundPlayer.Stop();
		soundPlayer.Stream = strm;
		soundPlayer.Play();
	}

	// if any of the items is "ReadResponse", then we mark read response as false
	private void StopReadResponse(IEnumerable<PlayItem> items)
	{
		foreach(var itm in items)
		{
			if (itm == null) 
				continue;
			if (itm.itemType != PlayItemType.ReadResponse)
				continue;
			if (isWaitingResponse)
				log("Stop waiting for the response. Timeout");
			isWaitingResponse = false;
		}
	}


	private void PlaySoundIfAny(List<PlayItem> items)
	{
		if (soundPlayer == null) return;

		if (items == null) return;
		foreach(var itm in items)
		{
			if (itm.itemType == PlayItemType.Sound)
			{
				PlaySound(itm);
			}
		}
	}

	private void SetCurrentSection(PlayItem itm)
	{
		currentSection = itm;
		currentSectionEndMs = itm.durationMs < 0.0
			? double.MaxValue
			: itm.startMs + itm.durationMs;
		UpdateSectionInfo();
	}

	private void UpdateSectionInfo()
	{
		if (sectionInfo == null)
			return;

		if (currentSection == null)
		{
			sectionInfo.Text = "";
			return;
		}

		if (currentSectionEndMs == double.MaxValue)
		{
			sectionInfo.Text = $"Section: {currentSection.text}\nLeft: unlimited";
			return;
		}

		double remainingMs = Math.Max(0.0, currentSectionEndMs - plrTrack.lastMs);
		int seconds = (int)(remainingMs / 1000.0);
		int milliseconds = (int)(remainingMs % 1000.0);
		sectionInfo.Text = $"Section: {currentSection.text}\nLeft: {seconds}.{milliseconds:000} s";

		if (remainingMs <= 0.0)
			currentSection = null;
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
				case PlayItemType.TrialStart:
					log($"Trial start: {itm.text}");
					break;
				case PlayItemType.TrialEnd:
					log($"Trial end: {itm.text}");
					break;
				case PlayItemType.SectionStart:
					log($"start: {itm.text}");
					SetCurrentSection(itm);
					break;
				case PlayItemType.ReadResponse:
					if (!isWaitingResponse)
						log("waiting for response");
					isWaitingResponse = true;
					break;

				case PlayItemType.Text:
					node = CreateTextNode(itm, pos);
					break;

				case PlayItemType.ImageById:
					string n;
					n = itm.imageId.ToString();
					if (texs.TryGetValue(n, out var tt))
					{
						float w = (float)(itm.sizeCm * cmToPix);
						node = CreateImageNode(tt, pos, w);
					}
					else
						log($"image not found: {n}; {itm.imageId}");
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

				case PlayItemType.Bar:
					node = CreateBarNode(
						pos,
						(float)(itm.barLengthCm * cmToPix),
						(float)(itm.barWidthCm * cmToPix),
						(float)itm.barTheta,
						GDColor(itm.color));
					break;

				case PlayItemType.Plus:
					node = CreatePlusNode(
						pos,
						(float)(itm.barLengthCm * cmToPix),
						(float)(itm.barWidthCm * cmToPix),
						GDColor(itm.color));
					break;

				case PlayItemType.Star:
					node = CreateStarNode(
						pos,
						(float)(itm.radiusCm * cmToPix),
						GDColor(itm.color));
					break;

				case PlayItemType.RegularShape:
					node = CreateRegularShapeNode(
						pos,
						(float)(itm.radiusCm * cmToPix),
						(float)(itm.lineWidthCm * cmToPix),
						itm.regularPoints,
						itm.regularFilled,
						(float)itm.regularRotation,
						GDColor(itm.color));
					break;
			}
			if (node != null)
			{
				node.ZIndex = itm.drawOrder;
				TrackDrawNode(node);
			}
		}
	}






	
}
