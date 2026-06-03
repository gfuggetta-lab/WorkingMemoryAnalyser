using System;
using System.IO;
using Godot;
using WMAData;
using WMAFiles;
using ConfigFile = WMAFiles.ConfigFile;

namespace godot.Scripts
{
	public partial class InstructionsShow : Node
	{
		[Export]
		public TextureRect instructionsImage;

		// Instructions play button
		[Export]
		public Button playButton;
		[Export]
		public Button stopButton;

		public string audio = "";

		private AudioStreamPlayer audioPlayer;
		private AudioStream audioStream;

		public override void _Ready()
		{
			audioPlayer = new AudioStreamPlayer();
			AddChild(audioPlayer);

			string cfgFn = "";
			string dir = ExperimentShared.SourcePath;
			if (!string.IsNullOrWhiteSpace(dir))
			{
				cfgFn = Path.Combine(dir, "Configuration.txt");
			}
			if (!string.IsNullOrWhiteSpace(cfgFn))
				LoadConfig(cfgFn);
			if (playButton != null)
				playButton.Pressed += PlayAudio;

			if (stopButton != null)
				stopButton.Pressed += StopAudio;
		}

		public void PlayAudio()
		{
			if (string.IsNullOrWhiteSpace(audio))
				return;

			if (audioPlayer == null)
			{
				audioPlayer = new AudioStreamPlayer();
				AddChild(audioPlayer);
			}

			if (audioStream == null)
				audioStream = LoadAudioStream(audio);
			if (audioStream == null)
				return;

			if (audioPlayer.Playing)
				audioPlayer.Stop();

			audioPlayer.Stream = audioStream;
			audioPlayer.Play();
		}

		public void StopAudio()
		{
			if ((audioPlayer != null) && audioPlayer.Playing)
				audioPlayer.Stop();
		}

		private static AudioStream LoadAudioStream(string fileName)
		{
			try
			{
				switch (Path.GetExtension(fileName).ToLowerInvariant())
				{
					case ".wav":
						return AudioStreamWav.LoadFromFile(fileName);
					case ".mp3":
						return AudioStreamMP3.LoadFromFile(fileName);
					case ".ogg":
						return AudioStreamOggVorbis.LoadFromFile(fileName);
					default:
						GD.Print($"unsupported instructions audio file: {fileName}");
						return null;
				}
			}
			catch (Exception x)
			{
				GD.Print($"failed to read audio: {fileName}; {x.Message}");
				return null;
			}
		}

		public void LoadConfig(string configFileName)
		{
			string dir = Path.GetDirectoryName(configFileName);
			var exam = new Configuration();
			var cfg = ConfigFile.FromFile(configFileName);
			exam.LoadConfig(cfg);

			ExperimentShared.data.ExperimentName = exam.ExperimentName;

			if (ExperimentShared.data.TrialOrderNum == 0)
				ExperimentShared.data.TrialOrderNum = 1;

			bool isOdd = (ExperimentShared.data.TrialOrderNum & 1) != 0;

			string imgFn;
			if (isOdd)
			{
				imgFn = exam.Instructions_ODD_participants;
				audio = exam.Audio_Instructions_ODD_participants;
			}
			else
			{
				imgFn = exam.Instructions_EVEN_participants;
				audio = exam.Audio_Instructions_EVEN_participants;
			}

			imgFn = Path.Combine(dir, imgFn);
			if (instructionsImage != null)
			{
				instructionsImage.Texture = null;
				try
				{
					Image m = new Image();
					var err = m.Load(imgFn);
					if (err == 0)
					{
						var _tex = ImageTexture.CreateFromImage(m);
						instructionsImage.Texture = _tex;
					}
				}
				catch (Exception x)
				{
					GD.Print($"failed to read: {imgFn}; {x.Message}");
				}
			}

			StopAudio();
			audioStream = null;
			audio = Path.Combine(dir, audio);
			if (!File.Exists(audio))
			{
				audio = "";
			}
			if (playButton != null)
				playButton.Visible = !string.IsNullOrWhiteSpace(audio);
			if (stopButton != null)
				stopButton.Visible = !string.IsNullOrWhiteSpace(audio);
		}
	}
}