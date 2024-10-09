using System.IO;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

public static class QuickSerializer
{
    public static void SaveFileBinary<T>(string path, object content)
    {
        using (var stream = new FileStream(path, FileMode.OpenOrCreate))
        {
            using (var writer = new BinaryWriter(stream))
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

    public static T? ReadFileBinary<T>(string path)
    {
        using (var stream = new FileStream(path, FileMode.Open))
        {
            using (var reader = new BinaryReader(stream))
            {
                // Read the serialized JSON string from the binary file
                string json = reader.ReadString();
                try
                {
                    // Deserialize the JSON string back into the object of type T
                    return JsonConvert.DeserializeObject<T>(json);
                }
                catch (Exception ex)
                {
                    var a = 0;
                }

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
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(T), new[] { typeof(RuntimeAssetHandle) });

        StreamReader sr = new(path);

        object? content = xmlSerializer.Deserialize(sr);

        sr.Close();

        return (T)content;
    }
}