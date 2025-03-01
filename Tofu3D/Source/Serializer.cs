using System.IO;
using System.Text;
using Newtonsoft.Json;

public static class Serializer
{
    private static JsonSerializer _jsonSerializer = new JsonSerializer() { Formatting = Formatting.Indented };

    public static void SaveAssetJSON<T>(string path, AssetBase asset)
    {
        if (asset.CanBeSerialized == false)
        {
            return;
        }

        asset.BeforeSerialized();
        SaveFileJSON<T>(path, asset);
    }

    public static T? ReadAssetJSON<T>(string path) where T : AssetBase
    {
        T asset = ReadFileJSON<T>(path);
        asset?.OnDeserialized();

        return asset;
    }

    public static void SaveFileJSON(string path, object content)
    {
        SaveFileJSON<object>(path, content);
    }

    public static void SaveFileJSON<T>(string path, object content)
    {
        // using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate))
        // {
        //     using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, false))
        //     {
        //         writer.Write(JsonConvert.SerializeObject(content, Formatting.Indented));
        //     }
        // }

        using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate))
        using (StreamWriter streamWriter = new StreamWriter(stream, Encoding.UTF8))
        using (JsonTextWriter jsonWriter = new JsonTextWriter(streamWriter))
        {
            _jsonSerializer.Serialize(jsonWriter, content);
        }
    }

    public static void SaveTextFile(string path, string text)
    {
        using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate))
        {
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, false))
            {
                writer.Write(text);
            }
        }
    }

    public static T? ReadFileJSON<T>(string path)
    {
        if (File.Exists(path) == false)
        {
            // Debug.LogError($"ReadFileJSON failed, file doesn't exist: {path}");
            return default;
        }

        // jsonReader streams the data so it doesnt have to allocate huge amounts of data for deserializing big files
        using (FileStream stream = new FileStream(path, FileMode.Open))
        using (StreamReader streamReader = new StreamReader(stream, Encoding.UTF8))
        using (JsonTextReader jsonReader = new JsonTextReader(streamReader))
        {
            // JsonSerializer serializer = new JsonSerializer();
            return _jsonSerializer.Deserialize<T>(jsonReader);
        }


        using (FileStream stream = new FileStream(path, FileMode.Open))
        {
            using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, false))
            {
                // Read the serialized JSON string from the binary file
                string json = reader.ReadString();

                return JsonConvert.DeserializeObject<T>(json);
            }
        }
    }
}