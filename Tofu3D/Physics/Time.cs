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

	// static Stopwatch _stopwatchUpdate = new Stopwatch();
	// static Stopwatch _stopwatchUpdate = new Stopwatch();
	// static Stopwatch _stopwatch = new Stopwatch();

	static int _slowDeltaTimeUpdateCounter = 5;
	static float _slowDeltaTimeUpdateForDebug=0.0166f;
	public static void Update()
	{
		// _deltaTimeTotal = (float) Window.I.RenderTime + (float) Window.I.UpdateTime; //_stopwatch.ElapsedMilliseconds / 1000f;
		// _stopwatch.Restart();
		// _deltaTimeTotal = (float) (Window.I.RenderTime + Window.I.UpdateTime);
		_slowDeltaTimeUpdateCounter--;
		if (_slowDeltaTimeUpdateCounter == 0)
		{
			_slowDeltaTimeUpdateCounter = 5;
			_slowDeltaTimeUpdateForDebug = EditorDeltaTime;
		}
		Debug.StatSetValue("FPS", (int) (1f / _slowDeltaTimeUpdateForDebug));
		Debug.StatSetValue("DeltaTime(ms)", _slowDeltaTimeUpdateForDebug * 1000);
		

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