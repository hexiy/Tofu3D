using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

public static class Serializer
{
    public static void SaveAssetJSON<T>(string path, AssetBase asset)
    {
        asset.BeforeSerialized();
        SaveFileJSON<T>(path, asset);
    }
    public static T? ReadAssetJSON<T>(string path) where T : AssetBase
    {
        T asset = ReadFileJSON<T>(path);
        asset.OnDeserialized();

        return asset;
    }

    public static void SaveFileJSON<T>(string path, object content)
    {
        using (var stream = new FileStream(path, FileMode.OpenOrCreate))
        {
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, false))
            {
                writer.Write(JsonConvert.SerializeObject(content));
            }
        }
    }

    public static T? ReadFileJSON<T>(string path)
    {
        if (File.Exists(path) == false)
        {
            return default;
        }

        using (var stream = new FileStream(path, FileMode.Open))
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
            {
                // Read the serialized JSON string from the binary file
                string json = reader.ReadString();

                return JsonConvert.DeserializeObject<T>(json);
            }
        }
    }
}