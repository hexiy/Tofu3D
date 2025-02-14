using System.IO;
using System.Linq;

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
        int skipFrames = 3;
        System.Diagnostics.StackTrace? b = new System.Diagnostics.StackTrace(fNeedFileInfo: true, skipFrames: 2);
        List<System.Diagnostics.StackFrame> frames = b.GetFrames().ToList();
        while (frames[0].GetFileName()?.Contains("debug.cs", StringComparison.OrdinalIgnoreCase) ?? false)
        {
            frames.RemoveAt(0);
        }

        StackTrace stackTrace = new StackTrace();
        StackFrame[] stackFrames = new StackFrame[frames.Count];
        for (int i = 0; i < stackFrames.Length; i++)
        {
            stackFrames[i] = new StackFrame();
            string text = frames[i].GetMethod()?.DeclaringType?.Name + "." + frames[i];
            text = text.Substring(0, text.IndexOf(" at"));

            stackFrames[i].Text = text;


            string? fileFullPath = frames[i].GetFileName();
            string fileShort = "";
            int line = 0;
            int column = 0;
            if (fileFullPath != null)
            {
                string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
                fileShort = Path.GetRelativePath(projectDirectory, fileFullPath);
                line = frames[i].GetFileLineNumber();
                column = frames[i].GetFileColumnNumber();
            }
            else
            {
                fileFullPath = "undefined";
            }

            if (fileFullPath.Length == 0)
            {
                fileShort = "external code";
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