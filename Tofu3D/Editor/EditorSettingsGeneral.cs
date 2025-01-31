using System.ComponentModel;

namespace Tofu3D;

public class EditorSettingsGeneral
{
    public string CodeEditorPath = "";
    [Slider(3,25)]
    public int FontSize = 12;
}