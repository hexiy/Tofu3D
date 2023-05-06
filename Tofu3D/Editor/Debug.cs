using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Tofu3D;

public static class Debug
{
	static List<LogEntry> _logs = new();

	public static readonly int Limit = 3000;

	public static Dictionary<string, DebugGraphTimer> GraphTimers = new();
	public static Dictionary<string, Stopwatch> SimpleTimers = new();
	public static Dictionary<string, string> Stats = new();
	public static Dictionary<string, float> AdditiveStats = new();

	public static bool Paused = false;

	private static void Log(string message, LogCategory logCategory = LogCategory.Info)
	{
		if (Paused)
		{
			return;
		}

		if (Global.EditorAttached == false)
		{
			return;
		}

		StackTrace stackTrace = StackTraceFactory.GetStackTrace();
		LogEntry logEntry = new LogEntry() {Message = message, StackTrace = stackTrace, Time = $"[{DateTime.Now:HH:mm:ss}:{DateTime.Now.Millisecond:000}]", LogCategory = logCategory};
		lock (_logs)
		{
			_logs.Add(logEntry);

			//Tofu.I.Window.Title = logs.Last();

			if (_logs.Count > Limit + 1)
			{
				_logs.RemoveAt(0);
			}
		}
	}

	public static void LogError(object message)
	{
		Log(message, LogCategory.Error);
	}

	public static void Log(object message, LogCategory logCategory = LogCategory.Info)
	{
		Log(message.ToString(), logCategory);
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

		lock (SimpleTimers)
		{
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
	}

	public static void StatAddValue(string statName, float value)
	{
		if (Global.EditorAttached == false)
		{
			return;
		}

		if (AdditiveStats.ContainsKey(statName) == false)
		{
			AdditiveStats[statName] = value;
		}
		else
		{
			AdditiveStats[statName] += value;
		}
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

	public static float EndTimer(string timerName)
	{
		if (Global.EditorAttached == false)
		{
			return -1;
		}

		SimpleTimers[timerName].Stop();
		float msDuration = (float) Math.Round(SimpleTimers[timerName].Elapsed.TotalMilliseconds, 2);
		return msDuration;
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

	public static float EndAndLogTimer(string timerName)
	{
		if (Global.EditorAttached == false)
		{
			return -1;
		}

		EndTimer(timerName);
		float msDuration = (float) Math.Round(SimpleTimers[timerName].Elapsed.TotalMilliseconds, 2);

		Log($"{timerName} : {msDuration} ms", LogCategory.Timer);
		return msDuration;
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

	public static ref List<LogEntry> GetLogsRef()
	{
		return ref _logs;
	}
}