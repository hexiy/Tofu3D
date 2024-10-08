using System.IO;
using ImGuiNET;

namespace Tofu3D;

public class EditorLayoutManager
{
    private readonly string DefaultSettingsName = "defaultSettings.ini";
    // string _lastUsedLayoutName => PersistentData.;

    private float _autoSaveTimer = 3;

    public string LastUsedLayoutName
    {
        get => PersistentData.GetString(nameof(LastUsedLayoutName), string.Empty);
        private set => PersistentData.Set(nameof(LastUsedLayoutName), value);
    }

    public void SaveCurrentLayout()
    {
        SaveLayout("editor.ini");
    }

    private void SaveLayout(string fileName)
    {
        if (fileName == DefaultSettingsName)
        {
            return;
        }

        ImGui.SaveIniSettingsToDisk(fileName);
        LastUsedLayoutName = fileName;
    }

    public void LoadDefaultLayout()
    {
        LoadLayout(DefaultSettingsName);
    }

    private void LoadLayout(string fileName)
    {
        if (File.Exists(fileName) == false)
        {
            if (fileName != DefaultSettingsName)
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
        ImGui.SaveIniSettingsToDisk(DefaultSettingsName);
        LastUsedLayoutName = DefaultSettingsName;
    }
}