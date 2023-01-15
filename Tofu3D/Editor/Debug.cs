using System.Diagnostics;

namespace Tofu3D;

public static class Debug
{
	static List<string> _logs = new();

	public static readonly int LogLimit = 1000;

	public static Dictionary<string, Stopwatch> Timers = new();
	public static Dictionary<string, float> Stats = new();

	private static void Log(string message)
	{
		if (Global.EditorAttached == false)
		{
			return;
		}


		_logs.Add($"[{DateTime.Now.ToString("HH:mm:ss")}]" + message);

		//Window.I.Title = logs.Last();

		if (_logs.Count > LogLimit + 1)
		{
			_logs.RemoveAt(0);
		}
	}

	public static void Log(object message)
	{
		Log(message.ToString());
	}

	public static void StartTimer(string timerName)
	{
		if (Global.EditorAttached == false)
		{
			return;
		}

		if (Timers.ContainsKey(timerName))
		{
			Timers[timerName].Restart();
		}
		else
		{
			Stopwatch sw = new();
			sw.Start();
			Timers.Add(timerName, sw);
		}
	}

	public static void CountStat(string statName, float value)
	{
		if (Global.EditorAttached == false)
		{
			return;
		}

		if (Stats.ContainsKey(statName) == false)
		{
			Stats[statName] = 0;
		}

		Stats[statName] += value;
	}

	public static void Stat(string statName, float value)
	{
		if (Global.EditorAttached == false)
		{
			return;
		}

		if (Stats.ContainsKey(statName) == false)
		{
			Stats[statName] = 0;
		}

		Stats[statName] = value;
	}

	public static void EndTimer(string timerName)
	{
		if (Global.EditorAttached == false)
		{
			return;
		}

		Timers[timerName].Stop();
	}

	public static void EndAndLogTimer(string timerName)
	{
		if (Global.EditorAttached == false)
		{
			return;
		}

		EndTimer(timerName);
		float msDuration = (float) Math.Round(Timers[timerName].Elapsed.TotalMilliseconds, 2);

		Debug.Log($"[{timerName}] {msDuration}");
	}

	public static void ClearTimers()
	{
		Timers.Clear();
	}

	public static void ClearStats()
	{
		Stats.Clear();
	}

	public static void ClearLogs()
	{
		_logs.Clear();
	}

	public static ref List<string> GetLogs()
	{
		return ref _logs;
	}
}