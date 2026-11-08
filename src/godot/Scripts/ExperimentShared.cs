using Godot;
using WMAData;
using godot.Scripts;
using NLog;

public partial class ExperimentShared : Node
{
    public static ExperimentData data = null;


    // The exam source directory where Overview.txt and Configure.txt are stored
    public static string SourcePath = "";
    // the soruce file name
    public static string SourceFileName = "";

    // The monitor selected
    public static string SelectedMonitorID = "";

    // the flag is set to true, if we pass all the screens as expected
    // the flag would be false, if launched some scene via Godot editor
    public static bool IsInitialized = false;

    public override void _EnterTree()
    {
        AppLogging.Configure(ProjectSettings.GlobalizePath("user://logs"));
        LogManager.GetLogger(nameof(ExperimentShared)).Info("Application started.");
    }

    public override void _ExitTree()
    {
        LogManager.GetLogger(nameof(ExperimentShared)).Info("Application stopped.");
        AppLogging.Shutdown();
    }

    public override void _Ready()
    {
        if (data == null)
            data = ExperimentData.Start();
    }
}
