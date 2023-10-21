using System.IO;
using Newtonsoft.Json;

namespace Tofu3D;

public static class PersistentData
{
	static bool _inited = false;
	static Dictionary<string, string> _data = new();
	static readonly string PersistentDataFileName = "persistentData.json";

	static void LoadAllData()
	{
		if (File.Exists(PersistentDataFileName) == false)
		{
			return;
		}

		string jsonFileContent = File.ReadAllText(PersistentDataFileName);
		var x = JsonConvert.DeserializeObject(jsonFileContent);
		_data = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonFileContent);
		// _data = new Dictionary<string, object>();
		// using (StreamReader sr = new(PersistentDataFileName))
		// {
		// 	while (sr.Peek() != -1)
		// 	{
		// 		string line = sr.ReadLine();
		// 		if (line?.Length > 0)
		// 		{
		// 			string key = line.Substring(0, line.IndexOf(":"));
		// 			object value = line.Substring(line.IndexOf(":") + 1);
		// 			_data.Add(key, value);
		// 		}
		// 	}
		// }
	}

	static void Save()
	{
		string json = JsonConvert.SerializeObject(_data);

		if (File.Exists(PersistentDataFileName) == false)
		{
			FileStream fs = File.Create(PersistentDataFileName);
			fs.Close();
		}


		using (StreamWriter sw = new(PersistentDataFileName))
		{
			sw.Write(json);
		}
	}

	public static void DeleteAll()
	{
		_data = new Dictionary<string, string>();
		Save();
	}

	public static T Get<T>(string key, T? defaultValue) where T : class
	{
		if (_data.Count == 0)
		{
			LoadAllData();
		}

		if (_data.ContainsKey(key) == false)
		{
			if (defaultValue != null)
			{
				return defaultValue;
			}

			return null;
		}

		T deserializedObject = JsonConvert.DeserializeObject<T>(_data[key].ToString()); // needs this for serialized classes
		if (deserializedObject == null) //_data[key] is not T)
		{
			if (defaultValue != null)
			{
				return defaultValue;
			}

			return null;
		}

		return deserializedObject; //(T) _data[key];
	}

	public static object Get(string key, object? defaultValue = null)
	{
		if (_data.Count == 0)
		{
			LoadAllData();
		}

		if (_data.ContainsKey(key) == false)
		{
			if (defaultValue != null)
			{
				return defaultValue;
			}

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

		string json= JsonConvert.SerializeObject(value); // needs this for serialized classes

		_data[key] = json;

		Save();
	}
}