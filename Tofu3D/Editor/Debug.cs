using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Tofu3D;

public static class Debug
{
	static List<string> _logs = new();

	public static readonly int LogLimit = 1000;

	public static Dictionary<string, DebugTimer> Timers = new();
	public static Dictionary<string, float> Stats = new();

	private static void Log(string message)
	{
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

	public static void StartTimer(string timerName, DebugTimer.SourceGroup group = DebugTimer.SourceGroup.None, TimeSpan? redline = null, int drawOrder = 0)
	{
		if (Global.EditorAttached == false)
		{
			return;
		}

		if (Timers.ContainsKey(timerName))
		{
			Timers[timerName].Stopwatch.Restart();
		}
		else
		{
			DebugTimer debugTimer = new DebugTimer(timerName, group, redline, drawOrder);

			Timers.Add(timerName, debugTimer);
			Timers = new Dictionary<string, DebugTimer>(Timers.OrderBy(x => x.Value));


			debugTimer.Stopwatch.Start();
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

		Timers[timerName].Stopwatch.Stop();
	}

	public static void EndAndLogTimer(string timerName)
	{
		if (Global.EditorAttached == false)
		{
			return;
		}

		EndTimer(timerName);
		float msDuration = (float) Math.Round(Timers[timerName].Stopwatch.Elapsed.TotalMilliseconds, 2);

		Stat(timerName, msDuration);
	}

	public static void ResetTimers()
	{
		//Timers.Clear();
		foreach (KeyValuePair<string, DebugTimer> timerPair in Timers)
		{
			timerPair.Value.Stopwatch.Reset();
		}
	}

	public static void ClearStats()
	{
		Stats.Clear();
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