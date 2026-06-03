using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using WMAData;
using WMAFiles;
using static godot.WMAUtils;
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

		public override void _Ready()
		{
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

		}

		public void StopAudio()
		{ 

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
			string audio;
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
				catch(Exception x)
				{
					GD.Print($"failed to read: {imgFn}; {x.Message}");
				}
			}
        
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
