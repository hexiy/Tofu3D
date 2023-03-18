using System.Diagnostics;

namespace Tofu3D;

public static class Time
{
	public static float DeltaTime;
	// public static float DeltaTimeRender;

	public static float EditorDeltaTime = 0.01666666f;
	public static float FixedDeltaTime = 0.01f;
	public static float ElapsedTime;
	public static float EditorElapsedTime;
	public static float EditorElapsedTicks;
	public static float ElapsedSeconds;
	public static ulong ElapsedTicks;
	public static ulong TimeScale = 0;

	public static int MaxFps = 0;
	public static int MinFps = 0;
	public static float MinMaxFpsTimer = 0;

	// static Stopwatch _stopwatchUpdate = new Stopwatch();
	// static Stopwatch _stopwatchUpdate = new Stopwatch();
	// static Stopwatch _stopwatch = new Stopwatch();

	public static void Update()
	{
		// _deltaTimeTotal = (float) Tofu.I.Window.RenderTime + (float) Tofu.I.Window.UpdateTime; //_stopwatch.ElapsedMilliseconds / 1000f;
		// _stopwatch.Restart();
		// _deltaTimeTotal = (float) (Tofu.I.Window.RenderTime + Tofu.I.Window.UpdateTime);

		int fps = (int) (1f / EditorDeltaTime);
		if (fps > MaxFps && Time.EditorElapsedTime > 2)
		{
			MaxFps = fps;
		}

		if (fps < MinFps && Time.EditorElapsedTime > 2)
		{
			MinFps = fps;
		}

		if (Time.EditorElapsedTime < 2)
		{
			MinFps = fps;
			MaxFps = fps;
		}

		MinMaxFpsTimer += EditorDeltaTime;
		if (MinMaxFpsTimer >= 5)
		{
			MaxFps = (MaxFps + fps) / 2;
			MinFps = (MinFps + fps) / 2;
			MinMaxFpsTimer = 0;
		}

		bool updateSlowerDebugStats = Time.EditorElapsedTicks % 30 == 0;
		if (updateSlowerDebugStats)
		{
			Debug.StatSetValue("FPS ", $"FPS:{fps}");
		}

		Debug.StatSetValue("FPS Range", $"FPS Range(5s)     < {MinFps} -- {MaxFps} >");
		// Debug.StatSetValue("Max FPS ", $"Max FPS(5s) {MaxFps}");
		if (updateSlowerDebugStats)
		{
			Debug.StatSetValue("DeltaTime(ms)", $"DeltaTime(ms) {(EditorDeltaTime * 1000).ToString("F2")}");
		}

		// Tofu.I.Window.Title = $"DeltaTime(ms){(EditorDeltaTime * 1000).ToString("F2")}";


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