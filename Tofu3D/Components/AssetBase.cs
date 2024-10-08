namespace Tofu3D;

public abstract class AssetBase
{
    [Hide] [XmlElement("AssetPath")] public string Path = "";

    [XmlIgnore] public AssetHandle Handle { get; set; }

    // private Asset()
    // {
    // }

    public static implicit operator bool(AssetBase instance)
    {
        if (instance == null)
        {
            return false;
        }

        return true;
    }
}