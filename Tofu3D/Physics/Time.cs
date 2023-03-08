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

	static int _slowDeltaTimeUpdateCounter = 5;
	static float _slowDeltaTimeUpdateForDebug = 0.0166f;

	public static void Update()
	{
		// _deltaTimeTotal = (float) Window.I.RenderTime + (float) Window.I.UpdateTime; //_stopwatch.ElapsedMilliseconds / 1000f;
		// _stopwatch.Restart();
		// _deltaTimeTotal = (float) (Window.I.RenderTime + Window.I.UpdateTime);


		// _slowDeltaTimeUpdateCounter--;
		// if (_slowDeltaTimeUpdateCounter == 0)
		// {
		// 	_slowDeltaTimeUpdateCounter = 30;
		// 	_slowDeltaTimeUpdateForDebug = EditorDeltaTime;
		// }
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

		Debug.StatSetValue("FPS ", $"FPS:{fps}");
		Debug.StatSetValue("FPS Range", $"FPS Range(5s)     < {MinFps} -- {MaxFps} >");
		// Debug.StatSetValue("Max FPS ", $"Max FPS(5s) {MaxFps}");
		Debug.StatSetValue("DeltaTime(ms)", $"DeltaTime(ms) {(EditorDeltaTime * 1000).ToString("F2")}");

		Window.I.Title = $"DeltaTime(ms){(EditorDeltaTime * 1000).ToString("F2")}";


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