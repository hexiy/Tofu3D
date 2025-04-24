namespace TofuEngine;

/// <summary>
/// Used in editor settings window
/// </summary>
public class EditorSettingsAll
{
    public EditorSettingsGeneral EditorSettingsGeneral;
    public EditorSettingsCodeEditor EditorSettingsCodeEditor;
    public EditorSettingsSceneView EditorSettingsSceneView;

    public void SaveData()
    {
        PersistentData.Set("EditorSettingsGeneral", EditorSettingsGeneral);
        PersistentData.Set("EditorSettingsCodeEditor", EditorSettingsCodeEditor);
        PersistentData.Set("EditorSettingsSceneView", EditorSettingsSceneView);
    }

    public void LoadSavedData()
    {
        EditorSettingsGeneral =
            PersistentData.Get<EditorSettingsGeneral>("EditorSettingsGeneral", () => new EditorSettingsGeneral());

        EditorSettingsGeneral.Init();

        EditorSettingsCodeEditor =
            PersistentData.Get<EditorSettingsCodeEditor>("EditorSettingsCodeEditor",
                () => new EditorSettingsCodeEditor());

        EditorSettingsCodeEditor.Init();


        EditorSettingsSceneView =
            PersistentData.Get<EditorSettingsSceneView>("EditorSettingsSceneView",
                () => new EditorSettingsSceneView());
    }
}