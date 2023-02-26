using System.Diagnostics;

namespace Tofu3D;

public static class Time
{
	private static float _deltaTimeUpdate = 0.01666666f;
	private static float _deltaTimeRender = 0.01666666f;
	private static float _deltaTimeTotal = 0.01666666f;

	public static float DeltaTime = 0.01666666f;
	public static float EditorDeltaTime = 0.01666666f;
	public static float FixedDeltaTime = 0.01f;
	public static float ElapsedTime;
	public static float EditorElapsedTime;
	public static float EditorElapsedTicks;
	public static float ElapsedSeconds;
	public static ulong ElapsedTicks;
	public static ulong TimeScale = 0;

	static Stopwatch _stopwatchUpdate = new Stopwatch();
	static Stopwatch _stopwatchRender = new Stopwatch();

	public static void Update()
	{
		// _deltaTimeTotal = (float) (Window.I.RenderTime + Window.I.UpdateTime);
		_deltaTimeTotal = (float) (_deltaTimeRender + _deltaTimeUpdate);
		Debug.Stat("FPS", (int)(1f/_deltaTimeTotal));
		EditorDeltaTime = _deltaTimeTotal;
		EditorElapsedTime += EditorDeltaTime;
		EditorElapsedTicks++;

		if (Global.GameRunning)
		{
			DeltaTime = _deltaTimeTotal;

			ElapsedTime += DeltaTime;
			ElapsedSeconds = ElapsedTime;
			ElapsedTicks++;
		}
		else
		{
			DeltaTime = 0;
		}
	}

	public static void StartDeltaTimeUpdateStopWatch()
	{
		_stopwatchUpdate.Restart();
	}

	public static void EndDeltaTimeUpdateStopWatch()
	{
		_stopwatchUpdate.Stop();
		_deltaTimeUpdate = _stopwatchUpdate.ElapsedMilliseconds / 1000f;
	}

	public static void StartDeltaTimeRenderStopWatch()
	{
		_stopwatchRender.Restart();
	}

	public static void EndDeltaTimeRenderStopWatch()
	{
		_stopwatchRender.Stop();
		_deltaTimeRender = _stopwatchRender.ElapsedMilliseconds / 1000f;
	}
}