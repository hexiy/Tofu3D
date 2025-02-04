namespace Tofu3D;

/// <summary>
/// Used in editor settings window
/// </summary>
public class EditorSettingsAll
{
    public EditorSettingsGeneral EditorSettingsGeneral;
    public EditorSettingsCodeEditor EditorSettingsCodeEditor;

    public void SaveData()
    {
        PersistentData.Set("EditorSettingsGeneral", EditorSettingsGeneral);
        PersistentData.Set("EditorSettingsCodeEditor", EditorSettingsCodeEditor);
    }

    public void LoadSavedData()
    {
        EditorSettingsGeneral =
            PersistentData.Get<EditorSettingsGeneral>("EditorSettingsGeneral", () => new EditorSettingsGeneral());

        EditorSettingsGeneral.Init();
        
        EditorSettingsCodeEditor =
            PersistentData.Get<EditorSettingsCodeEditor>("EditorSettingsCodeEditor",
                () => new EditorSettingsCodeEditor());
    }
}