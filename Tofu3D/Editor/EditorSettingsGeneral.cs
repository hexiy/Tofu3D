using System.ComponentModel;

namespace Tofu3D;

public class EditorSettingsGeneral
{
    [PathString]
    public string CodeEditorPath = "";
    [Slider(3,25)]
    public int FontSize = 12;

    public EditorThemeEnum EditorTheme;
}