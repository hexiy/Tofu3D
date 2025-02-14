namespace TofuEngine;

public struct LogEntry
{
    public string Time;
    public string Message;
    public StackTrace StackTrace;

    public LogCategory LogCategory;
}