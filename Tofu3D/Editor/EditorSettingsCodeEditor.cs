using System.ComponentModel;

namespace Tofu3D;

public class EditorSettingsCodeEditor
{
    [PathString]
    [SplitWords]
    public string CodeEditorPath = "";
}