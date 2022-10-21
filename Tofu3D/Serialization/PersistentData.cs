using System.IO;
using System.Linq;

namespace Tofu3D;

public static class PersistentData
{
	static bool _inited = false;
	static Dictionary<string, object> _data = new();

	static void LoadAllData()
	{
		if (File.Exists("persistentData") == false)
		{
			return;
		}

		_data = new Dictionary<string, object>();
		using (StreamReader sr = new("persistentData"))
		{
			while (sr.Peek() != -1)
			{
				string line = sr.ReadLine();
				if (line?.Length > 0)
				{
					string key = line.Substring(0, line.IndexOf(":"));
					string value = line.Substring(line.IndexOf(":") + 1);
					_data.Add(key, value);
				}
			}
		}
	}

	static void Save()
	{
		if (File.Exists("persistentData") == false)
		{
			File.Delete("persitentData");
		}

		FileStream fs = File.Create("persistentData");
		fs.Close();
		using (StreamWriter sw = new("persistentData"))
		{
			for (int i = 0; i < _data.Count; i++)
			{
				sw.WriteLine(_data.Keys.ElementAt(i) + ":" + _data.Values.ElementAt(i));
			}
		}
	}

	public static void DeleteAll()
	{
		_data = new Dictionary<string, object>();
		Save();
	}

	static object Get(string key, object? defaultValue = null)
	{
		if (_data.Count == 0)
		{
			LoadAllData();
		}

		if (_data.ContainsKey(key) == false && defaultValue != null)
		{
			return defaultValue;
		}

		if (_data.ContainsKey(key) == false)
		{
			return null;
		}

		return _data[key];
	}

	public static string GetString(string key, string? defaultValue = null)
	{
		return Get(key, defaultValue)?.ToString();
	}

	public static int GetInt(string key, int? defaultValue = null)
	{
		return int.Parse(Get(key, defaultValue)?.ToString());
	}

	public static bool GetBool(string key, bool? defaultValue = null)
	{
		return bool.Parse(Get(key, defaultValue)?.ToString());
	}

	public static void Set(string key, object value)
	{
		if (_data.Count == 0)
		{
			LoadAllData();
		}

		_data[key] = value;

		Save();
	}
}