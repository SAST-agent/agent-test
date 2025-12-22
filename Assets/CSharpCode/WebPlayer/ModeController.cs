public static class ModeController
{
    public enum Mode
    {
        Init,
        Play,
        Replay
    }

    private static Mode currentMode = Mode.Init;

    public static bool IsPlay()
    {
        return currentMode == Mode.Play;
    }

    public static bool IsReplay()
    {
        return currentMode == Mode.Replay;
    }

    public static void SwitchToPlay()
    {
        currentMode = Mode.Play;
    }

    public static void SwitchToReplay()
    {
        currentMode = Mode.Replay;
    }
}
