using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Tofu3D;

public static class Debug
{
	static List<string> _logs = new();

	public static readonly int LogLimit = 1000;

	public static Dictionary<string, DebugGraphTimer> GraphTimers = new();
	public static Dictionary<string, Stopwatch> SimpleTimers = new();
	public static Dictionary<string, string> Stats = new();
	public static Dictionary<string, float> AdditiveStats = new();

	public static bool Paused = false;

	private static void Log(string message)
	{
		if (Paused)
		{
			return;
		}

		if (Global.EditorAttached == false)
		{
			return;
		}


		_logs.Add($"[{DateTime.Now:HH:mm:ss}]" + message);

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

	public static void StartGraphTimer(string timerName, DebugGraphTimer.SourceGroup group = DebugGraphTimer.SourceGroup.None, TimeSpan? redline = null, int drawOrder = 0)
	{
		if (Global.EditorAttached == false)
		{
			return;
		}

		if (GraphTimers.ContainsKey(timerName))
		{
			GraphTimers[timerName].Stopwatch.Restart();
		}
		else
		{
			DebugGraphTimer debugGraphTimer = new DebugGraphTimer(timerName, group, redline, drawOrder);

			GraphTimers.Add(timerName, debugGraphTimer);
			GraphTimers = new Dictionary<string, DebugGraphTimer>(GraphTimers.OrderBy(x => x.Value));


			debugGraphTimer.Stopwatch.Start();
		}
	}

	public static void StartTimer(string timerName)
	{
		if (Global.EditorAttached == false)
		{
			return;
		}

		if (SimpleTimers.ContainsKey(timerName))
		{
			SimpleTimers[timerName].Restart();
		}
		else
		{
			Stopwatch sw = new Stopwatch();
			SimpleTimers.Add(timerName, sw);

			sw.Start();
		}
	}

	public static void StatAddValue(string statName, float value)
	{
		if (Global.EditorAttached == false)
		{
			return;
		}

		if (AdditiveStats.ContainsKey(statName) == false)
		{
			AdditiveStats[statName] = 0;
		}

		AdditiveStats[statName] += value;
	}

	public static void StatSetAdditiveValue(string statName, float value)
	{
		if (Global.EditorAttached == false)
		{
			return;
		}


		if (AdditiveStats.ContainsKey(statName) == false)
		{
			AdditiveStats[statName] = 0;
		}

		AdditiveStats[statName] = value;
	}

	public static void StatSetValue(string statName, object value)
	{
		if (Global.EditorAttached == false)
		{
			return;
		}


		if (Stats.ContainsKey(statName) == false)
		{
			Stats[statName] = "";
		}

		Stats[statName] = value.ToString();
	}

	public static void EndGraphTimer(string timerName)
	{
		if (Global.EditorAttached == false)
		{
			return;
		}

		/*
		if (Timers.ContainsKey(timerName) == false)
		{
			return;
		}*/

		GraphTimers[timerName].Stopwatch.Stop();
	}

	public static void EndTimer(string timerName)
	{
		if (Global.EditorAttached == false)
		{
			return;
		}

		SimpleTimers[timerName].Stop();
	}

	public static void EndAndLogGraphTimer(string timerName)
	{
		if (Global.EditorAttached == false)
		{
			return;
		}

		EndGraphTimer(timerName);
		float msDuration = (float) Math.Round(GraphTimers[timerName].Stopwatch.Elapsed.TotalMilliseconds, 2);

		StatSetValue(timerName, msDuration);
	}

	public static void EndAndStatTimer(string timerName, bool additiveStat = false)
	{
		if (Global.EditorAttached == false)
		{
			return;
		}

		EndTimer(timerName);
		float msDuration = (float) Math.Round(SimpleTimers[timerName].Elapsed.TotalMilliseconds, 2);


		if (additiveStat)
		{
			StatAddValue(timerName, msDuration);
		}
		else
		{
			StatSetValue(timerName, msDuration);
		}
	}

	public static void EndAndLogTimer(string timerName)
	{
		if (Global.EditorAttached == false)
		{
			return;
		}

		EndTimer(timerName);
		float msDuration = (float) Math.Round(SimpleTimers[timerName].Elapsed.TotalMilliseconds, 2);

		Log($"{timerName} {msDuration}");
	}

	public static void ResetTimers()
	{
		//Timers.Clear();
		foreach (KeyValuePair<string, DebugGraphTimer> timerPair in GraphTimers)
		{
			if (timerPair.Value.Stopwatch.IsRunning == false)
			{
				timerPair.Value.Stopwatch.Reset();
			}
		}

		foreach (KeyValuePair<string, Stopwatch> timerPair in SimpleTimers)
		{
			if (timerPair.Value.IsRunning == false)
			{
				timerPair.Value.Reset();
			}
		}
	}

	public static void ClearAdditiveStats()
	{
		AdditiveStats.Clear();
		//Stats.Clear();
	}

	public static void ClearLogs()
	{
		_logs.Clear();
	}

	public static ref List<string> GetLogsRef()
	{
		return ref _logs;
	}
}