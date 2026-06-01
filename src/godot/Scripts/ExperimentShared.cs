using Godot;
using WMAData;

public partial class ExperimentShared : Node
{
    public static ExperimentData data = null;


    // The exam source directory where Overview.txt and Configure.txt are stored
    public static string SourcePath = "";

    // The monitor selected
    public static string SelectedMonitorID = "";

    // the flag is set to true, if we pass all the screens as expected
    // the flag would be false, if launched some scene via Godot editor
    public static bool IsInitialized = false;

    public override void _Ready()
    {
        if (data == null)
            data = ExperimentData.Start();
    }
}
