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
	public const string CloseTrial = "CloseTrial";
	public const string LeftResponse = "LeftResponse";
	public const string RightResponse = "RightResponse";

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
	public ResponseButton trialResponse = ResponseButton.NotGiven;
	// the condition evaluated based on the response.
	// it's populated at CheckResponse, based on the actual response given
	public PlayItemCond currentCond = PlayItemCond.None;

	public Configuration exam;
	public List<TrialOrder> trials = new List<TrialOrder>();
	private int curTrialIdx = -1;
	public TrialOrder curTrial = null;
	public TrialResults result = new TrialResults();
	public ResultReport report = new ResultReport();
	
	private ulong timeOfExperimentStart;
	private ulong s4start;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		exam = new Configuration();
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
		AssignKeyboardEvents(exam.keyboards);

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
		trials = InputDataHelper.LoadTrials(inp);
		
		

		exam.Schedule(tm, trials, playList);
		log($"trials:  {trials.Count}");
		curTrial = trials[0];
		curTrialIdx = -1; // needed to handle TrialStart properly

		timeOfExperimentStart = Time.GetTicksMsec();
		report.SetConfig(exam);

		drawItems.Clear();
		plrTrack = new PlayListTracker(playList);
		plrTrack.Track(0, drawItems, null, null, null);

		string imgDir = Path.GetDirectoryName(inp);
		imgDir = Path.GetDirectoryName(imgDir);
		Preload(exam, imgDir, trials);

		ControlEvents(drawItems);
		RebuildDrawNodes();
		PlaySoundIfAny(drawItems);
	
		UpdateSectionInfo();
	}

	private void Preload(Configuration exam, string expDir, List<TrialOrder> list)
	{
		string imgDir = Path.Combine(expDir, "Stimulus images");
		
		List<string> resNames = new List<string>();
		
		exam.GetPreloadImages(list, resNames);
		resNames.Add("correct");
		resNames.Add("incorrect");

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
		// for compatibility with the "integer" based images
		// the response images are reported as "int" with 100 for correct 
		// and 101 for incorrect image
		if (texs.TryGetValue("incorrect", out var inci))
		{
			texs[Consts.IMAGEID_INCORRECT.ToString()] = inci;
		}
		if (texs.TryGetValue("correct", out var ci))
		{
			texs[Consts.IMAGEID_CORRECT.ToString()] = ci;
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
		List<PlayItem> trig = new List<PlayItem>();
		int cnt = plrTrack.Track(delta*1000.0, eff, trig, offList, trigAndOff);
		
		// this must run before RebuildDrawNodes
		// because BuildDraw nodes would verify the condition
		// controlevents actually update the currentCondition
		ControlEvents(trig);

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
			if (!IsCondMet(itm))
				continue;

			if (itm.itemType == PlayItemType.Sound)
			{
				PlaySound(itm);
			}
		}
	}

	private void ControlEvents(List<PlayItem> items)
	{
		if (items == null) return;

		foreach (var itm in items)
		{
			if (itm == null) continue;
			if (!IsCondMet(itm)) continue;

			switch (itm.itemType)
			{
				case PlayItemType.TrialStart:
					log($"Trial start: {itm.text}");
					currentCond = PlayItemCond.None;
					curTrialIdx++;
					if ((curTrialIdx>= 0) && (curTrialIdx < trials.Count))
						curTrial = trials[curTrialIdx];

					// resetting the measurement time
					result.Reset();

					break;

				case PlayItemType.TrialEnd:
					log($"Trial end: {itm.text}; {(curTrialIdx + 1)}/{trials.Count}");
					result.blank_onsetTime = (int)(Time.GetTicksMsec() - timeOfExperimentStart);

					report.WriteTrial(curTrial, result);
					
					if ((curTrialIdx+1) >= trials.Count)
					{
						log("ending trials");
						EndTrial();
					}
					break;
				
				case PlayItemType.SectionStart:
					log($"start: {itm.text}");
					SetCurrentSection(itm);
					break;

				case PlayItemType.CheckResponse:
					log("checking response");
					currentCond = PlayItemCond.Incorrect;
					bool isCorr = false;
					
					result.observedDataResponseRecord = -1;
					result.observedDataCorrectResponseRecord = -1;

					if (trialResponse != ResponseButton.NotGiven)
					{
						exam.ProcessResponse(trialResponse, curTrial, out result.observedDataResponseRecord, out isCorr);
						result.observedDataCorrectResponseRecord = isCorr ? 1 : 0;
						if (isCorr)
							currentCond = PlayItemCond.Correct;
					}
					log($"is correct response: {currentCond}");
					break;
			}
		}
	}

	private void SetCurrentSection(PlayItem itm)
	{

		if (string.Compare(itm.text, "S1", true) == 0)
		{
			result.s1_onsetTime = (int)(Time.GetTicksMsec() - timeOfExperimentStart);
		}
		else if (string.Compare(itm.text, "S2", true) == 0)
		{
			result.s2_onsetTime = (int)(Time.GetTicksMsec() - timeOfExperimentStart);
		}
		else if (string.Compare(itm.text, "S3", true) == 0)
		{
			result.s3_onsetTime = (int)(Time.GetTicksMsec() - timeOfExperimentStart);
		}
		else if (string.Compare(itm.text, "S4", true)==0)
		{
			s4start = Time.GetTicksMsec();
			result.s4_onsetTime = (int)(s4start - timeOfExperimentStart);
		}
		else if (string.Compare(itm.text, "Aft", true) == 0)
		{
			result.feedback_onsetTime = (int)(Time.GetTicksMsec() - timeOfExperimentStart);
		}


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

	public bool IsCondMet(PlayItem itm)
	{
		return (itm != null) 
			&& ((itm.cond == PlayItemCond.None) || (itm.cond == currentCond));
	}

	private void RebuildDrawNodes()
	{
		if (drawRoot == null)
			return;

		ClearDrawNodes();
		var cpos = GetViewportRect().Size / 2.0f;
		foreach (var itm in drawItems)
		{
			if (itm == null) continue;
			if (itm.cond != PlayItemCond.None)
			{
				log($"condition check: {itm.cond}; needed {currentCond}");
				if (!IsCondMet(itm))
				{
					log("failed");
					continue;
				}
			}

			var pos = GetPos(itm.pos, cpos, itm.posDistanceCm * cmToPix, itm.posOther, itm.posCount);
			Node2D node = null;
			switch (itm.itemType)
			{
				case PlayItemType.ReadResponse:
					if (!isWaitingResponse)
						log("waiting for response");
					isWaitingResponse = true;
					trialResponse = ResponseButton.NotGiven;
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

	private void EndTrial()
	{
		var rr = report.text.ToString();
		File.WriteAllText(@"C:\FPC_Laz\WorkingMemoryAnalyser_pas\Experiments library\TestExp\Input data\InputData_1.report", rr);


		// we're done with al all the trials
		GetTree().Quit();
	}

	private void CancelTrial()
	{
		// todo: maybe add a signal to verify if we actually
		//       want to cancel the trial?
		GetTree().Quit();
	}

	private void AssignKeyboardEvents(string keysCsv)
	{
		GD.Print($"keys: '{keysCsv}'");
		string[] parts = keysCsv.Split(',');
		for (int i = 0; i < parts.Length; i++)
			parts[i] = parts[i].Trim();

		if (parts.Length > 0)
		{
			if (!AddPhysicalKeyToAction(LeftResponse, parts[0]))
				GD.Print($"failed to map: '{parts[0]}'");
		}

		if (parts.Length > 1)
		{
			if (!AddPhysicalKeyToAction(RightResponse, parts[1]))
				GD.Print($"failed to map: '{parts[1]}'");
		}
	}


	private void SetResponse(ResponseButton resp)
	{
		if (!isWaitingResponse) return;

		log($"response: {resp}");
		trialResponse = resp;
		isWaitingResponse = false;


		// record Reaction Time  between response event and time t1 taken immediately after s4 render command is sent;
		result.responseTimeMs = (int)(Time.GetTicksMsec() - s4start); // Time.curr
		result.response_onsetTime = (int)(Time.GetTicksMsec() - timeOfExperimentStart);
	}


	public override void _Input(InputEvent ev)
	{
		if (ev.IsActionPressed(CloseTrial))
		{
			CancelTrial();
			return;
		}

		if (!isWaitingResponse)
			return;

		if (ev.IsActionPressed(RightResponse))
		{
			SetResponse(ResponseButton.RightButton);
		}
		else if (ev.IsActionPressed(LeftResponse))
		{
			SetResponse(ResponseButton.LeftButton);
		}
	}
}
