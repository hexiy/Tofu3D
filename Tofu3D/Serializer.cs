using System.IO;
using System.Text;
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
        asset?.OnDeserialized();

        return asset;
    }

    public static void SaveFileJSON<T>(string path, object content)
    {
        using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate))
        {
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, false))
            {
                writer.Write(JsonConvert.SerializeObject(content, Formatting.Indented));
            }
        }
    }

    public static T? ReadFileJSON<T>(string path)
    {
        if (File.Exists(path) == false)
        {
            Debug.LogError("ReadFileJSON failed, file doesn't exist");
            return default;
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