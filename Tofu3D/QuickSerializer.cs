using System.IO;
using System.Xml.Serialization;

public static class QuickSerializer
{
    public static void SaveFile<T>(string path, object content)
    {
        XmlSerializer xmlSerializer = new XmlSerializer( typeof(T), new[]{typeof(AssetHandle), typeof(Asset_Texture), typeof(AssetImportParameters_Texture)});
        
        StreamWriter sw = new(path);

        xmlSerializer.Serialize(sw, content);

        sw.Close();
    }
    public static T? ReadFile<T>(string path)
    {
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(T),new[]{typeof(AssetHandle)});
        
        StreamReader sr = new(path);

        object? content =xmlSerializer.Deserialize(sr);

        sr.Close();

        return (T)content;
    }
}