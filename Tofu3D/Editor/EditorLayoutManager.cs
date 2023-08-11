using System.IO;
using System.Threading.Tasks;
using ImGuiNET;

namespace Tofu3D;

public class EditorLayoutManager
{
	// string _lastUsedLayoutName => PersistentData.;

    float _autoSaveTimer = 3;
	readonly string DefaultSettingsName = "defaultSettings.ini";
	public string LastUsedLayoutName
	{
		get { return PersistentData.GetString(nameof(LastUsedLayoutName), String.Empty); }
		private set { PersistentData.Set(nameof(LastUsedLayoutName), value); }
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
		if (LastUsedLayoutName != String.Empty)
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