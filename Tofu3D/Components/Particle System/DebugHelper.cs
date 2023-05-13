using System.IO;
using System.Runtime.CompilerServices;

namespace Tofu3D;

public static class DebugHelper
{
	public static void LogDrawCall([CallerFilePath] string filePath = "")
	{
		Debug.StatAddValue("Draw Calls", 1);

		if (Global.Debug)
		{
			// var b = new System.Diagnostics.StackTrace(fNeedFileInfo: false, skipFrames: 1);
			// System.Diagnostics.StackFrame[] frames = b.GetFrames();
			// string callerName = frames[0].GetMethod().DeclaringType.Name;

			string className = Path.GetFileNameWithoutExtension(filePath);
			Debug.StatAddValue($"Draw Calls [{className}]", 1);
		}
	}

	public static void LogVerticesDrawCall([CallerFilePath] string filePath = "", int verticesCount = 0)
	{
		Debug.StatAddValue("Total vertices:", verticesCount);

		if (Global.Debug)
		{
			// var b = new System.Diagnostics.StackTrace(fNeedFileInfo: false, skipFrames: 1);
			// System.Diagnostics.StackFrame[] frames = b.GetFrames();
			// string callerName = frames[0].GetMethod().DeclaringType.Name;

			string className = Path.GetFileNameWithoutExtension(filePath);
			Debug.StatAddValue($"Total vertices [{className}]:", verticesCount);

		}
	}
}