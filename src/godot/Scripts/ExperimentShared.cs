using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMAData;

namespace godot.Scripts
{
    public static class ExperimentShared
    {
        public static ExperimentData Experiment = ExperimentData.Start();
        
        // the flag is set to true, if we pass all the screens as expected
        // the flag would be false, if launched some scene via Godot editor
        public static bool IsInitialized = false;
    }
}
