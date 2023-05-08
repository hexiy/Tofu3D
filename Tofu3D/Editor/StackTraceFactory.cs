using System.IO;

namespace Tofu3D;

public static class StackTraceFactory
{
	public static StackTrace GetStackTrace()
	{
		// string stackTraceFullText = Environment.StackTrace;
		//
		// // remove first line
		// stackTraceFullText = stackTraceFullText.Remove(0, stackTraceFullText.IndexOf(Environment.NewLine, StringComparison.Ordinal) + 1);
		// stackTraceFullText = stackTraceFullText.Remove(0, stackTraceFullText.IndexOf(Environment.NewLine, StringComparison.Ordinal) + 1);


		// stackTrace.FullText = stackTraceFullText;
		// var a = new System.Diagnostics.StackFrame(true);
		var b = new System.Diagnostics.StackTrace(fNeedFileInfo: true, skipFrames: 3);
		System.Diagnostics.StackFrame[] frames = b.GetFrames();

		StackTrace stackTrace = new StackTrace();
		StackFrame[] stackFrames = new StackFrame[frames.Length];
		for (int i = 0; i < stackFrames.Length; i++)
		{
			stackFrames[i] = new StackFrame();
			string text = frames[i].GetMethod()?.DeclaringType?.Name + "." + frames[i].ToString();
			text = text.Substring(0, text.IndexOf(" at"));

			stackFrames[i].Text = text;


			string fileFullPath = frames[i].GetFileName();
			string fileShort = "";
			int line = 0;
			int column = 0;
			if (fileFullPath != null)
			{
				string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
				fileShort = Path.GetRelativePath(projectDirectory, fileFullPath);
				line = frames[i].GetFileLineNumber();
				column = frames[i].GetFileColumnNumber();
			}
			else
			{
				fileFullPath = "undefined";
			}

			stackFrames[i].FileFullPath = fileFullPath;
			stackFrames[i].FileShort = fileShort;
			stackFrames[i].Line = line;
			stackFrames[i].Column = column;
		}

		stackTrace.Frames = stackFrames;
		// stackTrace.Lines = stackTraceFullText.Split("at");
		return stackTrace;
	}
}