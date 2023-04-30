using System.IO;
using System.Threading.Tasks;
using ImGuiNET;

namespace Tofu3D;

public static class EditorLayoutManager
{
	// string _lastUsedLayoutName => PersistentData.;

	static float _autoSaveTimer = 3;
	static readonly string DefaultSettingsName = "defaultSettings.ini";
	public static string LastUsedLayoutName
	{
		get { return PersistentData.GetString(nameof(LastUsedLayoutName), String.Empty); }
		private set { PersistentData.Set(nameof(LastUsedLayoutName), value); }
	}

	public static void SaveCurrentLayout()
	{
		SaveLayout("editor.ini");
	}

	private static void SaveLayout(string fileName)
	{
		if (fileName == DefaultSettingsName)
		{
			return;
		}

		ImGui.SaveIniSettingsToDisk(fileName);
		LastUsedLayoutName = fileName;
	}

	public static void LoadDefaultLayout()
	{
		LoadLayout(DefaultSettingsName);
	}

	private static void LoadLayout(string fileName)
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

	public static void LoadLastLayout()
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

	public static void Update()
	{
		// _autoSaveTimer -= Time.EditorDeltaTime;
		// if (_autoSaveTimer <= 0)
		// {
		// 	SaveCurrentLayout();
		// 	_autoSaveTimer = 3;
		// }
	}

	public static void SaveDefaultLayout()
	{
		ImGui.SaveIniSettingsToDisk(DefaultSettingsName);
		LastUsedLayoutName = DefaultSettingsName;
	}
}