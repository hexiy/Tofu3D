using System.Diagnostics;
using System.IO;
using System.Linq;
using StackFrame = TofuEngine.StackFrame;

public class UserCodeEditorOpener
{
    private CodeEditorInfo? EditorToUse = null;

    public void SetEditorToUse(CodeEditorInfo editorInfo)
    {
        EditorToUse = editorInfo;
        // Debug.Log($"Set editor to use:{editorInfo.Name}");
    }

    public CodeEditorInfo? GetEditorInfoByName(string editorName)
    {
        return _editors.FirstOrDefault(editor => editor.Name.Equals(editorName, StringComparison.OrdinalIgnoreCase),
            null);
    }

    public CodeEditorInfo? AddEditor(string editorExecutablePath)
    {
        if (File.Exists(editorExecutablePath) == false)
        {
            Debug.LogError($"Couldn't add editor {editorExecutablePath}");
            return null;
        }

        CodeEditorInfo foundEditor =
            _editors.FirstOrDefault(editor => editor.ExecutableNames.Contains(editorExecutablePath), null);
        if (foundEditor != null)
        {
            return foundEditor;
        }

        CodeEditorInfo editorInfo = new CodeEditorInfo()
        {
            Name = Path.GetFileNameWithoutExtension(editorExecutablePath),
            ExecutableNames = [editorExecutablePath],
            IsAddedByUser = true,
            IsValid = true
        };


        _editors.Add(editorInfo);


        return editorInfo;
    }

    private List<CodeEditorInfo> _editors = new List<CodeEditorInfo>
    {
        new CodeEditorInfo
        {
            Name = "JetBrains Rider",
            ExecutableNames = ["rider"],
            ArgsTemplate = "--line {line} --column {column} \"{file}\"",
            IsAddedByUser = false,
        },
        new CodeEditorInfo
        {
            Name = "Visual Studio Code",
            ExecutableNames = ["code"],
            ArgsTemplate = "--goto \"{file}:{line}:{column}\"",
            IsAddedByUser = false,
        },
        new CodeEditorInfo
        {
            Name = "Zed",
            ExecutableNames = ["zed"],
            ArgsTemplate = "\"{file}:{line}:{column}\"",
            IsAddedByUser = false,
        },
        new CodeEditorInfo
        {
            Name = "Visual Studio",
            ExecutableNames = ["devenv"],
            ArgsTemplate = "/Edit \"{file}\" /Command \"Edit.GoTo {line}\"",
            IsAddedByUser = false,
        },
        new CodeEditorInfo
        {
            Name = "Sublime Text",
            ExecutableNames = ["subl"],
            ArgsTemplate = "\"{file}:{line}:{column}\"",
            IsAddedByUser = false,
        },
        new CodeEditorInfo
        {
            Name = "TextEdit",
            ExecutableNames = ["open"],
            ArgsTemplate = "-a TextEdit \"{file}\"",
            IsAddedByUser = false,
        },
        // new EditorInfo
        // {
        //     Name = "Vim",
        //     ExecutableNames = new[] { "vim" },
        //     ArgsTemplate = "\"+call cursor({line}, {column})\" \"{file}\""
        // },
    };

    public UserCodeEditorOpener()
    {
        CheckAvailableEditors();

        // EditorToUse =
        // GetEditorInfoByName(Tofu.EditorSettingsAll.EditorSettingsCodeEditor.CodeEditorPaths.GetFirstSelectedItem());
    }

    private void CheckAvailableEditors()
    {
        foreach (CodeEditorInfo editor in _editors)
        {
            editor.IsValid = false;
            foreach (string exe in editor.ExecutableNames)
            {
                if (CanExecute(exe))
                {
                    editor.IsValid = true;
                }
            }
        }
    }

    public string[] GetValidEditorNames()
    {
        return _editors
            .Where(editor => editor.IsValid)
            .Select(editor => editor.Name).ToArray();
    }


    public void OpenFileFromStackFrame(StackFrame stackFrame)
    {
        OpenFile(stackFrame.FileFullPath, stackFrame.Line, stackFrame.Column);
    }

    public void OpenFile(string filePath, int line, int column)
    {
        if (EditorToUse == null)
        {
            CodeEditorInfo codeEditorInfo = GetEditorInfoByName(Tofu.EditorSettingsAll
                .EditorSettingsCodeEditor.CodeEditorPaths.GetFirstSelectedItem());
            SetEditorToUse(codeEditorInfo);
        }

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            Console.WriteLine("Invalid file path.");
            return;
        }

        if (EditorToUse == null)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
            return;
        }

        string exeName = EditorToUse.ExecutableNames.First();
        string args = EditorToUse.ArgsTemplate
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
            Debug.Log($"Failed to open file with {EditorToUse.Name}: {ex.Message}");
            // Fallback
            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
        }
    }

    // private CodeEditorInfo? DetectKnownEditor()
    // {
    //     if (FavouriteEditor?.Length > 0)
    //     {
    //         CodeEditorInfo favouriteCodeEditorInfo = _knownEditors.Find(editor =>
    //             editor.Name.Equals(FavouriteEditor, StringComparison.OrdinalIgnoreCase));
    //         if (favouriteCodeEditorInfo != null)
    //         {
    //             foreach (string exe in favouriteCodeEditorInfo.ExecutableNames)
    //             {
    //                 if (CanExecute(exe))
    //                 {
    //                     return favouriteCodeEditorInfo;
    //                 }
    //             }
    //         }
    //     }
    //
    //     foreach (CodeEditorInfo editor in _knownEditors)
    //     {
    //         foreach (string exe in editor.ExecutableNames)
    //         {
    //             if (CanExecute(exe))
    //             {
    //                 return editor;
    //             }
    //         }
    //     }
    //
    //     return null;
    // }

    private bool CanExecute(string fileName)
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
}