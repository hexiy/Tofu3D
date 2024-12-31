using System.IO;
using Newtonsoft.Json;

namespace Tofu3D;

public static class PersistentData
{
    private static bool _inited = false;
    private static Dictionary<string, string> _data = new();

    private static void LoadAllData()
    {
        string persistentDataPath = Path.Combine(Folders.Data, "persistentData.json");
        if (File.Exists(persistentDataPath) == false)
        {
            return;
        }

        var jsonFileContent = File.ReadAllText(persistentDataPath);
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

    private static void Save()
    {
        var json = JsonConvert.SerializeObject(_data);
        string persistentDataPath = Path.Combine(Folders.Data, "persistentData.json");

        if (File.Exists(persistentDataPath) == false)
        {
            var fs = File.Create(persistentDataPath);
            fs.Close();
        }


        using (StreamWriter sw = new(persistentDataPath))
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

        if (_data.TryGetValue(key, out string value))
        {
            var deserializedObject =
                JsonConvert.DeserializeObject<T>(_data[key]); // needs this for serialized classes
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
        else
        {
            if (defaultValue != null)
            {
                return defaultValue;
            }

            return null;
        }
    }

    public static object Get(string key, object? defaultValue = null)
    {
        if (_data.Count == 0)
        {
            LoadAllData();
        }

        if (_data.TryGetValue(key, out string value))
        {
            return value;
        }
        else
        {
            if (defaultValue != null)
            {
                return defaultValue;
            }

            return null;
        }
    }

    public static string GetString(string key, string? defaultValue = null) => Get(key, defaultValue).ToString();

    public static int GetInt(string key, int? defaultValue = null) => int.Parse(Get(key, defaultValue)?.ToString());

    public static bool GetBool(string key, bool? defaultValue = null) => bool.Parse(Get(key, defaultValue)?.ToString());

    public static void Set(string key, object value)
    {
        if (_data.Count == 0)
        {
            LoadAllData();
        }

        var json = JsonConvert.SerializeObject(value); // needs this for serialized classes

        _data[key] = json;

        Save();
    }
}