using godot.Scripts;
using Godot;
using MonitorInfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WMAData;
using WMAExcel;
using WMAFiles;
//using ConfigFile = WMAFiles.ConfigFile;
using static godot.WMAUtils;


public partial class BootScript : Node2D
{
	public const string CloseTrial = "CloseTrial";
	public const string LeftResponse = "LeftResponse";
	public const string RightResponse = "RightResponse";
	public const string Pause = "Pause";

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
	int trialCount; 
	PlayListTracker plrTrack;
	List<PlayItem> drawItems = new List<PlayItem>();
	List<PlayItem> pauseList = new List<PlayItem>();
	List<PlayItem> postPauseList = new List<PlayItem>();
	PlayItem currentSection;
	double currentSectionEndMs = -1.0;
	Node2D drawRoot;
	readonly List<Node> drawNodes = new List<Node>();
	Dictionary<string, Texture2D> texs = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
	Dictionary<string, VideoStream> videos = new Dictionary<string, VideoStream>(StringComparer.OrdinalIgnoreCase);
	Dictionary<string, Font> fonts = new Dictionary<string, Font>(StringComparer.OrdinalIgnoreCase);
	Dictionary<string, AudioStream> sounds = new Dictionary<string, AudioStream>(StringComparer.OrdinalIgnoreCase);


	private bool examHasGlobalKeys = false;
	private WMAResponseKeys pendingKeys = null;
	public List<WMAResponseResults> responseResults = new List<WMAResponseResults>();
	public bool isWaitingResponse = false;
	public bool isDrawPause = false;
	public bool isDrawPostPause = false;
	public double postPauseTime = 0.0;
	public ResponseButton trialResponse = ResponseButton.NotGiven;
	// the condition evaluated based on the response.
	// it's populated at CheckResponse, based on the actual response given
	public PlayItemCond currentCond = PlayItemCond.None;

	public IExperimentData examData;
	//public Configuration exam;
	//public List<TrialOrder> trials = new List<TrialOrder>();
	//public List<PauseData> pauses = new List<PauseData>();
	//public TrialOrder curTrial = null;
	private int curTrialIdx = -1;
	public TrialResults result = new TrialResults();
	public ResultReport report = new ResultReport();
	public IAsyncExperimentNotifier notifier = null;

	private ulong timeOfExperimentStart;
	private ulong s4start;

	private int isWaitInput = 0;
	private bool isWaitMouseOnly;

	private static Shader videoCircleMaskShader;


	protected string GetConfigFileName()
	{
		return ExperimentShared.SourceFileName;
	}

	// returns the exact TrialOrderNum
	protected int GetWantedTrial()
	{
		var result = ExperimentShared.data.TrialOrderNum;
		if (result > 0)
			return result;

		var trials = examData.GetInputDataListSync();
		if ((trials == null)||(trials.Length == 0))
			return 1;

		List<int> vals = new List<int>();
		vals.AddRange(trials);
		var rng = new RandomNumberGenerator();
		rng.Randomize();
		var idx = rng.RandiRange(0, vals.Count-1);
		result = vals[idx];
		return result;
	}

	private IAsyncExperimentNotifier PrepareNotifier()
	{
		return new NothingNotifier();
	}

	public static IExperimentReader[] readers = new IExperimentReader[]
	{
		new TextExperimentReader(),
		new ExcelFileProvider()
	};

	public static async Task<IExperimentData> GetExpirmentData(string cfgFileName)
	{
		if (!File.Exists(cfgFileName))
			return null;

		foreach (var rdr in readers)
		{
			if (rdr is IExperimentDataSetLog sl)
				sl.SetLog(new GodotMSLogger());

			bool isReader = await rdr.IsExperimentFile(cfgFileName, CancellationToken.None);
			if (!isReader)
				continue;

			var res = await rdr.ReadExperiment(cfgFileName, CancellationToken.None);
			return res;
		}
		return null;
	}

	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		notifier = PrepareNotifier();


		//exam = new Configuration(new GodotMSLogger());

		// there's no configuration for the clear color
		RenderingServer.SetDefaultClearColor(new Color(0f, 0f, 0f));
		drawRoot = new Node2D
		{
			Name = "StimulusNodes"
		};
		AddChild(drawRoot);
		if (sectionInfo != null)
			sectionInfo.ZIndex = 1000;

		string cfgFileName = GetConfigFileName();
		GD.Print($"Loading config file: {cfgFileName}");
		examData = await GetExpirmentData(cfgFileName);
		if (examData == null)
		{
			GD.Print($"Failed to load an experiment from: {cfgFileName}");
			return;
		}

		if (examData is IExperimentDataSetLog sl)
		{
			sl.SetLog(new GodotMSLogger());
		}

		string globalKeys = examData.GetKeyboardCsv();
		AssignKeyboardEvents(globalKeys);

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
		var dir = Path.GetDirectoryName(cfgFileName);
		int inpNum = GetWantedTrial();

		examData.SelectInputdata(inpNum);

		bool schResult = examData.SchedulePlaylist(tm, playList, out trialCount);
		log($"loaded: {inpNum}; schedule: {schResult}; trials: {trialCount}");

		//exam.Schedule(tm, trials, pauses, playList);
		//curTrial = trials[0];
		curTrialIdx = -1; // needed to handle TrialStart properly

		timeOfExperimentStart = Time.GetTicksMsec();
		if (ExperimentShared.data != null)
			report.SetExperiment(ExperimentShared.data);

		// todo: implement report writing!
		// report.SetConfig(exam);

		drawItems.Clear();
		plrTrack = new PlayListTracker(playList);
		plrTrack.Track(0, drawItems, null, null, null);

		Preload(examData, dir);

		ControlEvents(drawItems);
		RebuildDrawNodes();
		PlaySoundIfAny(drawItems);

		pauseList.Clear();
		GatherByCond(drawItems, pauseList, PlayItemCond.Paused);
		postPauseList.Clear();
		GatherByCond(drawItems, postPauseList, PlayItemCond.PostPause);

		UpdateSectionInfo();

		// Notify Async should be the last step
		await NotifyAsync(drawItems);
	}

	private void PreloadTextures(IEnumerable<string> resNames, string imgDir)
	{
		List<string> tryExt = new List<string>();
		tryExt.Add(".png");
		tryExt.Add(".bmp");
		tryExt.Add(".ogv");
		foreach (var nm in resNames)
		{
			string ext = Path.GetExtension(nm);
			bool doTryExt = string.IsNullOrEmpty(ext);

			string bmpFn = Path.Combine(imgDir, nm);
			bool exists = File.Exists(bmpFn);

			if (!exists && doTryExt)
			{
				foreach (var x in tryExt)
				{
					string newfn = Path.ChangeExtension(bmpFn, x);
					if (File.Exists(newfn))
					{
						bmpFn = newfn;
						exists = true;

						break;
					}
				}
			}

			if (!exists)
			{
				log($"the file image from {bmpFn} doesn't exist");
				continue;
			}


			string foundExt = Path.GetExtension(bmpFn);
			if (IsVideoExtension(foundExt))
			{
				var video = PreloadVideo(bmpFn);
				if (video != null)
				{
					videos[nm] = video;

					var fn = Path.GetFileName(bmpFn);
					videos[fn] = video;
					videos[Path.GetFileNameWithoutExtension(fn)] = video;
				}
				continue;
			}

			Image img = new Image();
			try
			{
				var err = img.Load(bmpFn);
				if (err != 0)
				{
					log($"loading image from {bmpFn} failed: {err}");
					continue;
				}
				var _tex = ImageTexture.CreateFromImage(img);
				GD.Print($"loaded: {Path.GetFileName(bmpFn)}");
				texs[nm] = _tex;

				string f = Path.GetFileName(bmpFn);
				texs[f] = _tex;
				texs[Path.GetFileNameWithoutExtension(f)] = _tex;
			}
			catch (Exception x)
			{
				log($"loading image from {bmpFn} failed: {x.Message}");
			}
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
	}

	private static bool IsVideoExtension(string ext)
	{
		return string.Compare(ext, ".ogv", true) == 0;
	}

	private VideoStream PreloadVideo(string fileName)
	{
		try
		{
			var video = new VideoStreamTheora
			{
				File = fileName
			};
			GD.Print($"loaded video: {Path.GetFileName(fileName)}");
			return video;
		}
		catch (Exception x)
		{
			log($"loading video from {fileName} failed: {x.Message}");
			return null;
		}
	}

	private void Preload(IExperimentData exam, string expDir)
	{
		List<string> resNames = new List<string>();
		
		exam.GetPreloadImages(resNames);
		resNames.Add("correct");
		resNames.Add("incorrect");

		string imgDir = Path.Combine(expDir, "Stimulus images");
		PreloadTextures(resNames, imgDir);

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
		exam.GetPreloadSounds(resNames);
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
	public override async void _Process(double delta)
	{
		if (postPauseTime > 0)
		{
			postPauseTime -= delta;
			if (postPauseTime <= 0)
				postPauseTime = 0;
			isDrawPostPause = false;
			RebuildDrawNodes();
			return;
		}

		// don't count, time because we wait for input
		if (isWaitInput > 0) return;

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
		
		// Notify Async should be the last step
		await NotifyAsync(trigAndOff);
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

	private static Shader GetVideoCircleMaskShader()
	{
		if (videoCircleMaskShader != null)
			return videoCircleMaskShader;

		videoCircleMaskShader = new Shader
		{
			Code = @"shader_type canvas_item;
void fragment() {
	vec2 centered_uv = UV - vec2(0.5);
	if (dot(centered_uv, centered_uv) > 0.25) {
		discard;
	}
}"
		};
		return videoCircleMaskShader;
	}

	private Node2D CreateVideoNode(VideoStream video, Vector2 pos, float sizePx)
	{
		var root = new Node2D
		{
			Name = "Video",
			Position = pos
		};

		var player = new VideoStreamPlayer
		{
			Name = "VideoStreamPlayer",
			Stream = video,
			Autoplay = true,
			Expand = true,
			Size = new Vector2(sizePx, sizePx),
			Position = new Vector2(-sizePx / 2.0f, -sizePx / 2.0f),
			Material = new ShaderMaterial
			{
				Shader = GetVideoCircleMaskShader()
			}
		};
		root.AddChild(player);
		return root;
	}
	protected virtual Node2D CreateTextNode(
		// the item of text
		PlayItem itm, 
		// the central position
		Vector2 pos)
	{
		if ((itm.fontName == null) ||!fonts.TryGetValue(itm.fontName, out var fnt))
		{
			GD.Print($"Font not found: {itm.fontName}; '{itm.text}'");
			return null;
		}
		int fontSize = itm.fontSizePx;
		Vector2 size = fnt.GetStringSize(itm.text, HorizontalAlignment.Left, -1, fontSize);
		float ascent = fnt.GetAscent(fontSize);
		float descent = fnt.GetDescent(fontSize);
		Vector2 textPos = new Vector2(-size.X / 2.0f, (ascent - descent) / 2.0f);

		GD.Print($"text color: {itm.color}");
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

	public double GetMaxDuration(List<PlayItem> items)
	{
		double result = 0;
		foreach(var im in items)
		{
			result = Math.Max(result, im.startMs + im.durationMs);
		}
		return result;
	}
	private void GatherByCond(List<PlayItem> items, List<PlayItem> dstList, PlayItemCond cnd)
	{
		foreach(var itm in items)
		{
			if (itm == null) continue;
			if (itm.cond == cnd)
				dstList.Add(itm);
		}
	}
	public async Task NotifyAsync(List<PlayItem> items)
	{
		if (notifier == null)
			return;

		foreach (var itm in items)
		{
			switch (itm.itemType)
			{
				case PlayItemType.NotifyStart:
					await notifier.StartTrial();
					break;
				case PlayItemType.NotifyS1:
					await notifier.MarkerS1(itm.markerValue);
					break;
				case PlayItemType.NotifyS2:
					await notifier.MarkerS2(itm.markerValue);
					break;
				case PlayItemType.NotifyS3:
					await notifier.MarkerS3(itm.markerValue);
					break;
				case PlayItemType.NotifyS4:
					await notifier.MarkerS4(itm.markerValue);
					break;
				case PlayItemType.NotifyFeedbackCorrect:
				case PlayItemType.NotifyFeedbackIncorrect:
					bool isCorr = itm.itemType == PlayItemType.NotifyFeedbackCorrect;
					await notifier.Feedback(isCorr);
					break;
			}
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
					//curTrialIdx++;
					//if ((curTrialIdx>= 0) && (curTrialIdx < trials.Count))
					//	curTrial = trials[curTrialIdx];

					// resetting the measurement time
					result.Reset();

					break;

				case PlayItemType.TrialEnd:
					log($"Trial end: {itm.text}; {(curTrialIdx + 1)}/{trialCount}");
					result.blank_onsetTime = (int)(Time.GetTicksMsec() - timeOfExperimentStart);

					// TODO: implement report writing
					//report.WriteTrial(curTrial, result);
					
					if ((curTrialIdx+1) >= trialCount)
					{
						log("ending trials");
						EndTrial();
					}
					break;
				
				case PlayItemType.SectionStart:
					log($"start: {itm.text}");
					SetCurrentSection(itm);
					break;

				case PlayItemType.CustomEvent:
					log($"event: {itm.text}");
					if (string.Compare(itm.text, "S3TMS", true)==0)
					{
						result.TMS_onsetTime = GetOnSetTime();
					}
					//SetCurrentSection(itm);
					break;

				case PlayItemType.CheckResponse:
					log("checking response");
					currentCond = PlayItemCond.Incorrect;
					bool isCorr = false;
					
					result.observedDataResponseRecord = -1;
					result.observedDataCorrectResponseRecord = -1;

					if (trialResponse != ResponseButton.NotGiven)
					{
						// todo:
						// exam.ProcessResponse(trialResponse, curTrial, out result.observedDataResponseRecord, out isCorr);

						result.observedDataCorrectResponseRecord = isCorr ? 1 : 0;
						if (isCorr)
							currentCond = PlayItemCond.Correct;
					}
					log($"is correct response: {currentCond}");
					break;

				case PlayItemType.WaitForMouse:
				case PlayItemType.WaitForInput:
					log($"waiting for input: {itm.itemType}");
					isWaitInput++;
					isWaitMouseOnly = itm.itemType == PlayItemType.WaitForMouse;
					break;

			}
		}
	}

	public int GetOnSetTime()
	{
		return (int)(Time.GetTicksMsec() - timeOfExperimentStart);
	}

	private void SetCurrentSection(PlayItem itm)
	{

		if (string.Compare(itm.text, "S1", true) == 0)
		{
			result.s1_onsetTime = GetOnSetTime();
		}
		else if (string.Compare(itm.text, "S2", true) == 0)
		{
			result.s2_onsetTime = GetOnSetTime();
		}
		else if (string.Compare(itm.text, "S3", true) == 0)
		{
			result.s3_onsetTime = GetOnSetTime();
		}
		else if (string.Compare(itm.text, "S4", true)==0)
		{
			s4start = Time.GetTicksMsec();
			result.s4_onsetTime = (int)(s4start - timeOfExperimentStart);
		}
		else if (string.Compare(itm.text, "Aft", true) == 0)
		{
			result.feedback_onsetTime = GetOnSetTime();
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
		return IsCondMet(itm, currentCond);
	}
	public bool IsCondMet(PlayItem itm, PlayItemCond condCheck)
	{
		return (itm != null)
			&& ((itm.cond == PlayItemCond.None) || (itm.cond == condCheck));
	}

	private void RebuildDrawNodes()
	{
		if (isDrawPostPause)
			RebuildDrawNodes(postPauseList, PlayItemCond.PostPause);
		else if (isDrawPause)
			RebuildDrawNodes(pauseList, PlayItemCond.Paused);
		else
			RebuildDrawNodes(drawItems, currentCond);
	}


	private void AddWaitKeys(PlayItem itm)
	{
		WMAResponseKeys rk = new WMAResponseKeys();
		rk.responseKeys = itm.responseKeys;
		rk.correctKeys = itm.correctKeys;
		pendingKeys = rk;
	}

	private void RebuildDrawNodes(List<PlayItem> itemsList, PlayItemCond checkCond)
	{
		if (drawRoot == null)
			return;

		ClearDrawNodes();
		var cpos = GetViewportRect().Size / 2.0f;
		foreach (var itm in itemsList)
		{
			if (itm == null) continue;
			if (itm.cond != PlayItemCond.None)
			{
				//log($"condition check: {itm.cond}; needed {currentCond}");
				if (!IsCondMet(itm, checkCond))
				{
					//log("failed");
					continue;
				}
			}

			var pos = GetPos(itm.pos, cpos, itm.posDistanceCm * cmToPix, itm.posOther, itm.posCount);
			Node2D node = null;
			switch (itm.itemType)
			{
				case PlayItemType.ReadResponse:
					if (!isWaitingResponse)
					{
						log("waiting for response");
						pendingKeys = null;
						responseResults.Clear();
					}
					isWaitingResponse = true;
					trialResponse = ResponseButton.NotGiven;
					if ((itm.responseKeys != null) && (itm.responseKeys.Length > 0))
						AddWaitKeys(itm);
					break;

				case PlayItemType.Text:
					node = CreateTextNode(itm, pos);
					break;

				case PlayItemType.ImageById:
				case PlayItemType.ImageByName:
					string n;
					if (itm.itemType == PlayItemType.ImageById)
						n = itm.imageId.ToString();
					else
						n = itm.imageName;

					float w = (float)(itm.sizeCm * cmToPix);
					if (videos.TryGetValue(n, out var video))
					{
						node = CreateVideoNode(video, pos, w);
					}
					else if (texs.TryGetValue(n, out var tt))
					{
						node = CreateImageNode(tt, pos, w);
					}
					else
						log($"image/video not found: {n}; {itm.imageId}");
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

		string resultFileName = ResultReport.GenerateOutputFileName(ExperimentShared.data);
		string fn = Path.Combine(ExperimentShared.SourcePath, "Output Data", resultFileName);

		string d = Path.GetDirectoryName(fn);
		if (!Directory.Exists(d))
			Directory.CreateDirectory(d); ;

		var repText = report.text.ToString();
		File.WriteAllText(fn, repText);

		SuccessResultForm.ShowSuccessForm(this, d, resultFileName);
	}

	private void CancelTrial()
	{
		// todo: maybe add a signal to verify if we actually
		//       want to cancel the trial?
		GetTree().Quit();
	}

	private void AssignKeyboardEvents(string keysCsv)
	{
		if (string.IsNullOrWhiteSpace(keysCsv))
		{
			GD.Print("no configuration level keys. Using per trial keys");
			return;
		}

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

	public void TogglePause()
	{
		isDrawPause = !isDrawPause;
		if (isDrawPause)
		{
			isWaitInput++;
			isDrawPostPause = false;
			// mark the trial as ruined
			if (result.isRuinedTrial == 0)
				result.isRuinedTrial = 1;
		}
		else
		{
			postPauseTime = GetMaxDuration(postPauseList);
			if (postPauseTime > 0)
				isDrawPostPause = true;
			isWaitInput--;
			isWaitInput = Math.Max(isWaitInput, 0);
		}
		RebuildDrawNodes();
	}

	private void CheckByGlobalKeys(InputEvent ev)
	{
		if (ev.IsActionPressed(RightResponse))
		{
			SetResponse(ResponseButton.RightButton);
		}
		else if (ev.IsActionPressed(LeftResponse))
		{
			SetResponse(ResponseButton.LeftButton);
		}
	}

	private void CheckByTrialKeys(InputEvent ev)
	{
		if (pendingKeys == null)
			return;

		if (!TryGetInputName(ev, out var inputName))
			return;

		if (!StringArrayContains(pendingKeys.responseKeys, inputName))
			return;

		AddTrialResponseResult(new WMAResponseResults
		{
			name = inputName,
			isCorrect = StringArrayContains(pendingKeys.correctKeys, inputName)
		});
	}

	private void AddTrialResponseResult(WMAResponseResults responseResult)
	{
		if (!isWaitingResponse)
			return;

		bool isFirstResponse = responseResults.Count == 0;
		responseResults.Add(responseResult);

		if (isFirstResponse)
		{
			result.responseTimeMs = (int)(Time.GetTicksMsec() - s4start);
			result.response_onsetTime = (int)(Time.GetTicksMsec() - timeOfExperimentStart);
		}
	}

	private static bool TryGetInputName(InputEvent ev, out string inputName)
	{
		inputName = string.Empty;

		if (ev is InputEventKey keyEvent)
			return TryGetKeyEventName(keyEvent, out inputName);

		if (ev is InputEventMouseButton mouseEvent)
			return TryGetMouseButtonName(mouseEvent, out inputName);

		return false;
	}

	private static bool TryGetKeyEventName(InputEventKey keyEvent, out string inputName)
	{
		inputName = string.Empty;

		if (keyEvent == null || !keyEvent.Pressed || keyEvent.Echo)
			return false;

		Key key = keyEvent.PhysicalKeycode != Key.None ? keyEvent.PhysicalKeycode : keyEvent.Keycode;
		if (key == Key.None)
			return false;

		inputName = key.ToString();
		return !string.IsNullOrWhiteSpace(inputName);
	}

	private static bool TryGetMouseButtonName(InputEventMouseButton mouseEvent, out string inputName)
	{
		inputName = string.Empty;

		if (mouseEvent == null || !mouseEvent.Pressed)
			return false;

		switch (mouseEvent.ButtonIndex)
		{
			case MouseButton.Left:
				inputName = "left_mouse";
				return true;

			case MouseButton.Right:
				inputName = "right_mouse";
				return true;

			case MouseButton.Middle:
				inputName = "middle_mouse";
				return true;
		}

		return false;
	}

	private static bool StringArrayContains(string[] values, string expected)
	{
		if (values == null || string.IsNullOrWhiteSpace(expected))
			return false;

		foreach (var value in values)
		{
			if (string.Compare(value?.Trim(), expected, true) == 0)
				return true;
		}

		return false;
	}

	public override void _Input(InputEvent ev)
	{
		if (ev.IsActionPressed(CloseTrial))
		{
			CancelTrial();
			return;
		}

		if (ev.IsActionPressed(Pause))
		{
			TogglePause();
			return;
		}

		if (isDrawPause)
		{
			// The pause is in effect. Thus we ignore any input
			return;
		}


		if (isWaitInput > 0)
		{
			if (ev is InputEventMouseButton)
				isWaitInput--;
			else if ((ev is InputEventKey) && (!isWaitMouseOnly))
				isWaitInput--;

		}

		if (!isWaitingResponse)
			return;

		if (examHasGlobalKeys)
		{
			CheckByGlobalKeys(ev);
		}
		else
		{
			CheckByTrialKeys(ev);
		}


	}
}
