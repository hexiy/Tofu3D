using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

public static class QuickSerializer
{
    public static void SaveFileJSON<T>(string path, object content)
    {
        using (var stream = new FileStream(path, FileMode.OpenOrCreate))
        {
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, false))
            {
                writer.Write(JsonConvert.SerializeObject(content));
            }
        }
        // XmlSerializer xmlSerializer = new XmlSerializer( typeof(T), new[]{typeof(RuntimeAssetHandle), typeof(Asset_Texture), typeof(AssetImportParameters_Texture)});
        //
        // StreamWriter sw = new(path);
        //
        // xmlSerializer.Serialize(sw, content);
        //
        // sw.Close();
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

    public static void SaveFileXML<T>(string path, object content)
    {
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(T),
            new[] { typeof(RuntimeAssetHandle), typeof(RuntimeTexture), typeof(AssetImportParameters_Texture) });

        StreamWriter sw = new(path);

        xmlSerializer.Serialize(sw, content);

        sw.Close();
    }

    public static T? ReadFileXML<T>(string path)
    {
        if (File.Exists(path) == false)
        {
            return default;
        }

        XmlSerializer xmlSerializer = new XmlSerializer(typeof(T), new[] { typeof(RuntimeAssetHandle) });

        StreamReader sr = new(path);

        object? content = xmlSerializer.Deserialize(sr);

        sr.Close();

        return (T)content;
    }
}