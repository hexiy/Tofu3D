using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TofuEngine;

public class Debug
{
    private static string _logFilePath;

    private static List<LogEntry> _logs = new List<LogEntry>();
    private static readonly Channel<LogEntry> _logChannel;
    private static readonly ChannelWriter<LogEntry> _logWriter;
    private static readonly Task _logProcessorTask;

    public static readonly int Limit = 3000;

    public static Dictionary<string, DebugGraphTimer> GraphTimers = new Dictionary<string, DebugGraphTimer>();
    public static Dictionary<string, Stopwatch> SimpleTimers = new Dictionary<string, Stopwatch>();
    public static Dictionary<string, string> Stats = new Dictionary<string, string>();
    public static Dictionary<string, float> AdditiveStats = new Dictionary<string, float>();

    public static bool Paused = false;

    static Debug()
    {
        _logChannel = Channel.CreateUnbounded<LogEntry>();
        _logWriter = _logChannel.Writer;
        _logProcessorTask = Task.Run(ProcessLogEntries);
    }

    public static void Init()
    {
        CreateLogFile();
    }

    private static void CreateLogFile()
    {
        _logFilePath = Path.Combine(path1: Folders.Data, path2: "logs.txt");
        try
        {
            string? dir = Path.GetDirectoryName(path: _logFilePath);
            if (!string.IsNullOrEmpty(value: dir))
            {
                Directory.CreateDirectory(path: dir);
            }
        }
        catch
        {
        }
    }

    private static async Task ProcessLogEntries()
    {
        await foreach (var logEntry in _logChannel.Reader.ReadAllAsync())
        {
            lock (_logs)
            {
                _logs.Add(logEntry);

                if (_logs.Count > Limit + 1)
                {
                    _logs.RemoveAt(0);
                }
            }


            string fullMessage = $"[{logEntry.Time}] : {logEntry.LogCategory} | {logEntry.Message}";

            Console.WriteLine(value: fullMessage);

            try
            {
                await File.AppendAllTextAsync(path: _logFilePath,
                    contents: $"{fullMessage}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                // ignored
            }
        }
    }

    [Conditional("TRACE")]
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
        LogEntry logEntry = new LogEntry
        {
            Message = message, StackTrace = stackTrace,
            Time = $"[{DateTime.Now:HH:mm:ss}:{DateTime.Now.Millisecond:000}]", LogCategory = logCategory
        };

        _logWriter.TryWrite(logEntry);
    }

    [Conditional("TRACE")]
    public static void LogWarning(object message)
    {
        Log(message, LogCategory.Warning);
    }

    [Conditional("TRACE")]
    public static void LogError(object message)
    {
        Log(message, LogCategory.Error);
    }

    [Conditional("TRACE")]
    public static void Log(object message, LogCategory logCategory = LogCategory.Info)
    {
        Log(message.ToString(), logCategory);
    }

    [Conditional("TRACE")]
    public static void LogVariable(string variableName, object variable)
    {
        Log($"{variableName}:{variable.ToString()}", LogCategory.Info);
    }

    [Conditional("TRACE")]
    public static void LogDebug(object message, LogCategory logCategory = LogCategory.Info)
    {
        if (Global.Debug)
        {
            Log(message.ToString(), logCategory);
        }
    }

    [Conditional("TRACE")]
    public static void StartGraphTimer(string timerName,
        DebugGraphTimer.SourceGroup group = DebugGraphTimer.SourceGroup.None, TimeSpan? redline = null,
        int drawOrder = 0)
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

    [Conditional("TRACE")]
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

    [Conditional("TRACE")]
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

    [Conditional("TRACE")]
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

    [Conditional("TRACE")]
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

    [Conditional("TRACE")]
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
        float msDuration = (float)Math.Round(SimpleTimers[timerName].Elapsed.TotalMilliseconds, 2);
        return msDuration;
    }

    [Conditional("TRACE")]
    public static void EndAndLogGraphTimer(string timerName)
    {
        if (Global.EditorAttached == false)
        {
            return;
        }

        EndGraphTimer(timerName);
        float msDuration = (float)Math.Round(GraphTimers[timerName].Stopwatch.Elapsed.TotalMilliseconds, 2);

        StatSetValue(timerName, msDuration);
    }

    [Conditional("TRACE")]
    public static void EndAndStatTimer(string timerName, bool additiveStat = false)
    {
        if (Global.EditorAttached == false)
        {
            return;
        }

        EndTimer(timerName);
        float msDuration = (float)Math.Round(SimpleTimers[timerName].Elapsed.TotalMilliseconds, 2);


        if (additiveStat)
        {
            StatAddValue(timerName, msDuration);
        }
        else
        {
            StatSetValue(timerName, $"{timerName} : {msDuration}ms");
        }
    }

    public static float EndAndLogTimer(string timerName)
    {
        if (Global.EditorAttached == false)
        {
            return -1;
        }

        EndTimer(timerName);
        float msDuration = (float)Math.Round(SimpleTimers[timerName].Elapsed.TotalMilliseconds, 2);

        Log($"{timerName} : {msDuration} ms", LogCategory.Timer);
        return msDuration;
    }

    [Conditional("TRACE")]
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

    [Conditional("TRACE")]
    public static void ClearAdditiveStats()
    {
        AdditiveStats.Clear();
        //Stats.Clear();
    }

    [Conditional("TRACE")]
    public static void ClearLogs()
    {
        _logs.Clear();
    }

    public static ref List<LogEntry> GetLogsRef() => ref _logs;

    [Conditional("TOFU_ASSERTIONS")]
    [Conditional("TRACE")]
    public static void Assert(bool condition, object? message = null)
    {
        if (condition == false)
        {
            LogError($"Assertion failed. {message ?? string.Empty}");
        }
    }
}