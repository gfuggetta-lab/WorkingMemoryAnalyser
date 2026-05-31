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
        }

        public void LoadConfig(string configFileName)
        {
            var exam = new Configuration();
            var cfg = ConfigFile.FromFile(configFileName);
            exam.LoadConfig(cfg);

            ExperimentShared.data.ExperimentName = exam.ExperimentName;

            if (ExperimentShared.WantedTrialNumber == 0)
                ExperimentShared.WantedTrialNumber = 1;

            bool isOdd = (ExperimentShared.WantedTrialNumber & 1) != 0;

            string img;
            string audio;
            if (isOdd) 
            {
                img = exam.Instructions_ODD_participants;
            } 
            else
            {
                img = exam.Instructions_EVEN_participants;
            }
        }
    }
}
