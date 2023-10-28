using System.Diagnostics;

namespace Tofu3D;

// ReSharper disable once InconsistentNaming
public static class RiderIDE
{
    private static string _cachedRiderPath = "";

    public static void OpenStackTrace(StackFrame stackFrame)
    {
        if (stackFrame.FileFullPath?.Length == 0) return;

        try
        {
            // windows solution
            // if (_cachedRiderPath.Length == 0)
            // {
            // 	string riderPath = "";
            // 	var x = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User);
            // 	if (x == null)
            // 	{
            // 		riderPath = "/Users/hexiy/Applications/Rider.app/Contents/MacOS";
            // 	}
            // 	else
            // 	{
            // 		string[] paths = x.Split(";");
            //                  riderPath = paths.FirstOrDefault((s => s.ToLower().Contains("jetbrains rider")), "");
            // 	}
            //
            // 	if (riderPath.Length == 0 || Directory.Exists(riderPath) == false)
            // 	{
            // 		Debug.LogError("Couldn't find Rider in envornment variable 'Path'");
            // 		return;
            // 	}
            //
            // 	_cachedRiderPath = riderPath;
            // }

            Process.Start(new ProcessStartInfo
            {
                WorkingDirectory = "/",
                FileName = "rider",
                Arguments = $"--line {stackFrame.Line} --column {stackFrame.Column + 4} \"{stackFrame.FileFullPath}\"",
                // Arguments = $"--line {stackFrame.Line} --column {stackFrame.Column + 4} {stackFrame.FileFullPath}",
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            });
        }
        catch (Exception ex)
        {
            Debug.Log("Error opening file in Rider IDE : " + ex.Message);
        }
    }
}