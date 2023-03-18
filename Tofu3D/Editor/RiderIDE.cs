using System.Diagnostics;

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
			Process.Start(startInfo: new ProcessStartInfo()
			                         {
				                         WorkingDirectory = "/",
				                         FileName = "rider",
				                         Arguments = $"--line {stackFrame.Line} --column {stackFrame.Column + 4} {stackFrame.FileFullPath}",
				                         CreateNoWindow = true,
				                         UseShellExecute = true,
			                         });
		}
		catch (Exception ex)
		{
			Debug.Log("Error opening file in Rider IDE : " + ex.Message);
		}
	}
}