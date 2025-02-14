using System.IO;
using ImGuiNET;

namespace TofuEngine;

public class EditorLayoutManager
{
    private readonly string DefaultEditorLayoutPath = TofuPath.Combine(Folders.Data, "defaultEditorLayout.ini");
    private readonly string CurrentEditorLayoutPath = TofuPath.Combine(Folders.Data, "currentEditorLayout.ini");
    // string _lastUsedLayoutName => PersistentData.;

    private float _autoSaveTimer = 3;

    public string LastUsedLayoutName
    {
        get => PersistentData.GetString(nameof(LastUsedLayoutName), string.Empty);
        private set => PersistentData.Set(nameof(LastUsedLayoutName), value);
    }

    public void SaveCurrentLayout()
    {
        SaveLayout(CurrentEditorLayoutPath);
    }

    private void SaveLayout(string fileName)
    {
        if (fileName == DefaultEditorLayoutPath)
        {
            return;
        }

        ImGui.SaveIniSettingsToDisk(fileName);
        LastUsedLayoutName = fileName;
    }

    public void LoadDefaultLayout()
    {
        LoadLayout(DefaultEditorLayoutPath);
    }

    private void LoadLayout(string fileName)
    {
        if (File.Exists(fileName) == false)
        {
            if (fileName != DefaultEditorLayoutPath)
            {
                LoadDefaultLayout();
            }

            return;
        }

        ImGui.LoadIniSettingsFromDisk(fileName);
        LastUsedLayoutName = fileName;
    }

    public void LoadLastLayout()
    {
        if (LastUsedLayoutName != string.Empty)
        {
            LoadLayout(LastUsedLayoutName);
        }
        else
        {
            LoadDefaultLayout();
        }
    }

    public void Update()
    {
        // _autoSaveTimer -= Time.EditorDeltaTime;
        // if (_autoSaveTimer <= 0)
        // {
        // 	SaveCurrentLayout();
        // 	_autoSaveTimer = 3;
        // }
    }

    public void SaveDefaultLayout()
    {
        ImGui.SaveIniSettingsToDisk(DefaultEditorLayoutPath);
        LastUsedLayoutName = DefaultEditorLayoutPath;
    }
}