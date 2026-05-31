using Godot;
using WMAData;

public partial class ExperimentShared : Node
{
    public static ExperimentData data = null;

    // the flag is set to true, if we pass all the screens as expected
    // the flag would be false, if launched some scene via Godot editor
    public static bool IsInitialized = false;

    public override void _Ready()
    {
        if (data == null)
            data = ExperimentData.Start();
    }
}
