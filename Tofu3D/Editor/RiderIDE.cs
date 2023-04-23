using System.Diagnostics;
using System.Linq;

namespace Tofu3D;

// ReSharper disable once InconsistentNaming
public static class RiderIDE
{
	public static void OpenStackTrace(StackFrame stackFrame)
	{
		if (stackFrame.FileFullPath?.Length == 0)
		{
			return;
		}

		try
		{
			var x = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User);
			string[] paths = x.Split(";");
			string riderPath = paths.First((s => s.ToLower().Contains("rider")));
			Process.Start(startInfo: new ProcessStartInfo()
			                         {
				                         WorkingDirectory = riderPath,
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