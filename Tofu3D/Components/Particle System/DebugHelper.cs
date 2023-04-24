using System.IO;
using System.Runtime.CompilerServices;

namespace Tofu3D;

public static class DebugHelper
{
	public static void LogDrawCall([CallerFilePath] string filePath = "")
	{
		if (Global.Debug)
		{
			// var b = new System.Diagnostics.StackTrace(fNeedFileInfo: false, skipFrames: 1);
			// System.Diagnostics.StackFrame[] frames = b.GetFrames();
			// string callerName = frames[0].GetMethod().DeclaringType.Name;

			Debug.StatAddValue("  Draw Calls", 1);
			string className = Path.GetFileNameWithoutExtension(filePath);
			Debug.StatAddValue($"  Draw Calls [{className}]", 1);
		}
		else
		{
			Debug.StatAddValue("  Draw Calls", 1);
		}
	}
}