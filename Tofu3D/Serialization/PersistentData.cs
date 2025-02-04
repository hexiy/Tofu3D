using Newtonsoft.Json;

namespace Tofu3D;

public static class PersistentData
{
    private static bool _inited = false;
    private static Dictionary<string, string> _data = new Dictionary<string, string>();

    private static void LoadAllData()
    {
        string persistentDataPath = TofuPath.Combine(Folders.Data, "persistentData.json");

        // var x =Serializer.ReadFileJSON<object>(persistentDataPath);
        // var b = x as Dictionary<string, string>;

        _data = Serializer.ReadFileJSON<Dictionary<string, string>>(persistentDataPath) ?? _data;
    }

    private static void Save()
    {
        string persistentDataPath = TofuPath.Combine(Folders.Data, "persistentData.json");

        Serializer.SaveFileJSON<Dictionary<string, string>>(persistentDataPath, _data);
    }

    public static void DeleteAll()
    {
        _data = new Dictionary<string, string>();
        Save();
    }

    public static T Get<T>(string key, Func<T>? defaultValueFunc) where T : class
    {
        if (_data.Count == 0)
        {
            LoadAllData();
        }

        if (_data.TryGetValue(key, out string value))
        {
            T? deserializedObject =
                JsonConvert.DeserializeObject<T>(_data[key]); // needs this for serialized classes
            if (deserializedObject == null) //_data[key] is not T)
            {
                if (defaultValueFunc != null)
                {
                    return defaultValueFunc.Invoke();
                }

                return null;
            }

            return deserializedObject; //(T) _data[key];
        }
        else
        {
            if (defaultValueFunc != null)
            {
                return defaultValueFunc.Invoke();
            }

            return null;
        }
    }

    public static object Get(string key, Func<object>? defaultValue = null)
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
                return defaultValue.Invoke();
            }

            return null;
        }
    }

    public static string GetString(string key, string? defaultValue = null) => Get(key, ()=>defaultValue).ToString();

    public static int GetInt(string key, int? defaultValue = null) => int.Parse(Get(key, ()=>defaultValue)?.ToString());

    public static bool GetBool(string key, bool? defaultValue = null) => bool.Parse(Get(key, ()=>defaultValue)?.ToString());

    public static void Set(string key, object value)
    {
        if (_data.Count == 0)
        {
            LoadAllData();
        }
        
        string? json = JsonConvert.SerializeObject(value, Formatting.Indented); // needs this for serialized classes

        _data[key] = json;

        Save();
    }
}