using System.Diagnostics;
using System.IO;
using System.Linq;
using StackFrame = Tofu3D.StackFrame;

public static class UserCodeEditorOpener
{
    private const string? FavouriteEditor = "JetBrains Rider";

    // TODO open scripts folder first and then the script itself
    private static List<EditorInfo> knownEditors = new List<EditorInfo>
    {
        new EditorInfo
        {
            Name = "JetBrains Rider",
            ExecutableNames = new[] { "rider" },
            ArgsTemplate = "--line {line} --column {column} \"{file}\""
        },
        new EditorInfo
        {
            Name = "Visual Studio Code",
            ExecutableNames = new[] { "code" },
            ArgsTemplate = "--goto \"{file}:{line}:{column}\""
        },
        new EditorInfo
        {
            Name = "Zed",
            ExecutableNames = new[] { "zed" },
            ArgsTemplate = "\"{file}:{line}:{column}\""
        },
        new EditorInfo
        {
            Name = "Visual Studio",
            ExecutableNames = new[] { "devenv" },
            ArgsTemplate = "/Edit \"{file}\" /Command \"Edit.GoTo {line}\""
        },
        new EditorInfo
        {
            Name = "Sublime Text",
            ExecutableNames = new[] { "subl" },
            ArgsTemplate = "\"{file}:{line}:{column}\""
        },
        // new EditorInfo
        // {
        //     Name = "Vim",
        //     ExecutableNames = new[] { "vim" },
        //     ArgsTemplate = "\"+call cursor({line}, {column})\" \"{file}\""
        // },
    };

    public static void OpenFileFromStackFrame(StackFrame stackFrame)
    {
        OpenFile(stackFrame.FileFullPath, stackFrame.Line, stackFrame.Column);
    }

    public static void OpenFile(string filePath, int line, int column)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            Console.WriteLine("Invalid file path.");
            return;
        }

        EditorInfo? editorToUse = DetectKnownEditor();

        if (editorToUse == null)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
            return;
        }

        string exeName = editorToUse.ExecutableNames.First();
        string args = editorToUse.ArgsTemplate
            .Replace("{file}", filePath)
            .Replace("{line}", line.ToString())
            .Replace("{column}", column.ToString());

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = exeName,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            });
        }
        catch (Exception ex)
        {
            Debug.Log($"Failed to open file with {editorToUse.Name}: {ex.Message}");
            // Fallback
            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
        }
    }

    private static EditorInfo? DetectKnownEditor()
    {
        if (FavouriteEditor?.Length > 0)
        {
            EditorInfo favouriteEditorInfo = knownEditors.Find(editor =>
                editor.Name.Equals(FavouriteEditor, StringComparison.OrdinalIgnoreCase));
            if (favouriteEditorInfo != null)
            {
                foreach (string exe in favouriteEditorInfo.ExecutableNames)
                {
                    if (CanExecute(exe))
                    {
                        return favouriteEditorInfo;
                    }
                }
            }
        }

        foreach (EditorInfo editor in knownEditors)
        {
            foreach (string exe in editor.ExecutableNames)
            {
                if (CanExecute(exe))
                {
                    return editor;
                }
            }
        }

        return null;
    }

    private static bool CanExecute(string fileName)
    {
        try
        {
            Process? proc = Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            proc?.Kill();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private class EditorInfo
    {
        public string Name { get; set; } = "";
        public string[] ExecutableNames { get; set; } = Array.Empty<string>();
        public string ArgsTemplate { get; set; } = "";
    }
}