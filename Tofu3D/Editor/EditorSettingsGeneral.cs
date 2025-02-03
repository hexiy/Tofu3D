using System.ComponentModel;
using Newtonsoft.Json;

namespace Tofu3D;

public class EditorSettingsGeneral
{
    [Slider(3, 25)]
    public int FontSize = 12;

    public EditorThemeEnum EditorTheme;

    [JsonIgnore]
    [Header("Delete persistent data header")]
    public Action DeletePersistentData => () => { PersistentData.DeleteAll(); };
}