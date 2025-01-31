namespace Tofu3D;

/// <summary>
/// Used in editor settings window
/// </summary>
public class EditorSettingsAll
{
    public EditorSettingsGeneral EditorSettingsGeneral;

    public void SaveData()
    {
        PersistentData.Set("EditorSettingsGeneral", EditorSettingsGeneral);
    }

    public void LoadSavedData()
    {
        EditorSettingsGeneral =
            PersistentData.Get<EditorSettingsGeneral>("EditorSettingsGeneral", new EditorSettingsGeneral());
    }
}