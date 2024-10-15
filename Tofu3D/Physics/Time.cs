namespace Tofu3D;

public static class Time
{
    public static float DeltaTime;
    // public static float DeltaTimeRender;

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

    // static Stopwatch _stopwatchUpdate = new Stopwatch();
    // static Stopwatch _stopwatchUpdate = new Stopwatch();
    // static Stopwatch _stopwatch = new Stopwatch();

    public static void Update()
    {
        // _deltaTimeTotal = (float) Tofu.Window.RenderTime + (float) Tofu.Window.UpdateTime; //_stopwatch.ElapsedMilliseconds / 1000f;
        // _stopwatch.Restart();
        // _deltaTimeTotal = (float) (Tofu.Window.RenderTime + Tofu.Window.UpdateTime);

        // var minDeltaTime = 0.0001f; // to avoid division by zero or very small values
        // if (EditorDeltaTime < minDeltaTime)
        // {
        //     EditorDeltaTime = minDeltaTime;
        // }
        
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

        var updateSlowerDebugStats = EditorElapsedTicks % 30 == 0;
        if (updateSlowerDebugStats)
        {
            Debug.StatSetValue("FPS ", $"FPS[LIMITER {(Tofu.Window.FrameLimiterEnabled ? "ON" : "OFF ")}]:{fps}");
        }

        Debug.StatSetValue("FPS Range", $"FPS Range(3s)              < {MinFpsDisplay} -- {MaxFpsDisplay} >");
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

    // public static void StartDeltaTimeUpdateStopWatch()
    // {
    // 	// _stopwatchUpdate.Restart();
    // }
    //
    // public static void EndDeltaTimeUpdateStopWatch()
    // {
    // 	// _stopwatchUpdate.Stop();
    // 	// _deltaTimeUpdate = _stopwatchUpdate.ElapsedMilliseconds / 1000f;
    // }
    //
    // public static void StartDeltaTimeRenderStopWatch()
    // {
    // 	// _stopwatchRender.Restart();
    // }
    //
    // public static void EndDeltaTimeRenderStopWatch()
    // {
    // 	// _stopwatchRender.Stop();
    // 	// _deltaTimeRender = _stopwatchRender.ElapsedMilliseconds / 1000f;
    // }
}