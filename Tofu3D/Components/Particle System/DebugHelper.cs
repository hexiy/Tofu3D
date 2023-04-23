namespace Tofu3D;

public static class DebugHelper
{
	public static void LogDrawCall()
	{
		if (Global.Debug)
		{
			var b = new System.Diagnostics.StackTrace(fNeedFileInfo: false, skipFrames: 1);
			System.Diagnostics.StackFrame[] frames = b.GetFrames();
			string callerName = frames[0].GetMethod().DeclaringType.Name;

			Debug.StatAddValue("  Draw Calls", 1);
			Debug.StatAddValue($"  Draw Calls [{callerName}]", 1);
		}
		else
		{
			Debug.StatAddValue("  Draw Calls", 1);
		}
	}
}