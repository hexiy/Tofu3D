namespace Tofu3D;

public static class DebugStackTracer
{
	public static string GetStackTrace()
	{
		string stackTrace = Environment.StackTrace;
		
		// remove first line
		stackTrace = stackTrace.Remove(0, stackTrace.IndexOf(Environment.NewLine, StringComparison.Ordinal)+1);
		stackTrace = stackTrace.Remove(0, stackTrace.IndexOf(Environment.NewLine, StringComparison.Ordinal)+1);

		return stackTrace;
	}
}