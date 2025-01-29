using System.Text;

namespace Tofu3D;

public static class Time
{
    public static float DeltaTime;

    public static float EditorDeltaTime = 0.01666666f;
    public static float EditorDeltaTimeMS = EditorDeltaTime*1000f;
    public static float EditorFPS => 1f / EditorDeltaTime;
    public static float FixedDeltaTime = 0.01f;
    public static float ElapsedTime;
    public static float EditorElapsedTime;
    public static int EditorElapsedTicks;
    public static float ElapsedSeconds;
    public static ulong ElapsedTicks;
    public static ulong TimeScale = 1;

    public static uint MaxFps;
    public static uint MinFps;
    public static uint MaxFpsDisplay;
    public static uint MinFpsDisplay;
    public static float MinMaxFpsTimer;

    static StringBuilder _fpsRangeStringBuilder = new();
    private static float _slowUpdateTimeLeft = 0f;
    public static void Update()
    {
        uint fps = (uint)(1f / EditorDeltaTime);
        if (fps > MaxFps && EditorElapsedTime > 1)
        {
            MaxFps = fps;
        }

        if (fps < MinFps && EditorElapsedTime > 1)
        {
            MinFps = fps;
        }

        if (EditorElapsedTime < 1)
        {
            MinFps = fps;
            MaxFps = fps;

            MinFpsDisplay = MinFps;
            MaxFpsDisplay = MaxFps;
        }

        MinMaxFpsTimer += EditorDeltaTime;
        if (MinMaxFpsTimer >= 3)
        {
            MaxFpsDisplay = MaxFps;
            MinFpsDisplay = MinFps;

            MaxFps = 0;
            MinFps = 99999;
            MinMaxFpsTimer = 0;
        }
        
        bool updateSlowerDebugStats = false;
        _slowUpdateTimeLeft -= EditorDeltaTime;
        if (_slowUpdateTimeLeft <= 0)
        {
            _slowUpdateTimeLeft = 0.3f;
            updateSlowerDebugStats = true;
        }
        if (updateSlowerDebugStats)
        {
            Debug.StatSetValue("FPS ", $"FPS[LIMITER {(Tofu.Window.FrameLimiterEnabled ? "ON" : "OFF ")}]:{fps}");
        }

        _fpsRangeStringBuilder.Clear();
        _fpsRangeStringBuilder.Append("FPS Range(3s)              < ");
        _fpsRangeStringBuilder.Append(MinFpsDisplay);
        _fpsRangeStringBuilder.Append(" -- ");
        _fpsRangeStringBuilder.Append(MaxFpsDisplay);
        _fpsRangeStringBuilder.Append(" >");

        Debug.StatSetValue("FPS Range",_fpsRangeStringBuilder.ToString());
        // Debug.StatSetValue("Max FPS ", $"Max FPS(5s) {MaxFps}");
        if (updateSlowerDebugStats)
        {
            Debug.StatSetValue("DeltaTime(ms)", $"DeltaTime(ms) {(EditorDeltaTime * 1000).ToString("F4")}");
        }

        // Tofu.Window.Title = $"DeltaTime(ms){(EditorDeltaTime * 1000).ToString("F2")}";


        EditorElapsedTime += EditorDeltaTime;
        EditorElapsedTicks++;


        if (Global.GameRunning)
        {
            DeltaTime = EditorDeltaTime;

            ElapsedTime += DeltaTime;
            ElapsedSeconds = ElapsedTime;
            ElapsedTicks++;
        }
        else
        {
            DeltaTime = 0;
        }
    }
}