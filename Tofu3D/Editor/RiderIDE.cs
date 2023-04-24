using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Tofu3D;

// ReSharper disable once InconsistentNaming
public static class RiderIDE
{
	static string _cachedRiderPath = "";

	public static void OpenStackTrace(StackFrame stackFrame)
	{
		if (stackFrame.FileFullPath?.Length == 0)
		{
			return;
		}

		try
		{
			if (_cachedRiderPath.Length == 0)
			{
				var x = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User);
				string[] paths = x.Split(";");
				string riderPath = paths.FirstOrDefault((s => s.ToLower().Contains("jetbrains rider")), "");

				if (riderPath.Length == 0 || Directory.Exists(riderPath) == false)
				{
					Debug.LogError("Couldn't find Rider in envornment variable 'Path'");
					return;
				}

				_cachedRiderPath = riderPath;
			}


			Process.Start(startInfo: new ProcessStartInfo()
			                         {
				                         WorkingDirectory = _cachedRiderPath,
				                         FileName = "rider",
				                         Arguments = $"--line {stackFrame.Line} --column {stackFrame.Column + 4} {stackFrame.FileFullPath}",
				                         CreateNoWindow = true,
				                         UseShellExecute = true,
				                         WindowStyle = ProcessWindowStyle.Hidden,
			                         });
		}
		catch (Exception ex)
		{
			Debug.Log("Error opening file in Rider IDE : " + ex.Message);
		}
	}
}