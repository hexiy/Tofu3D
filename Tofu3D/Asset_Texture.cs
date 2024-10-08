namespace Tofu3D;

[Serializable]
public class Asset_Texture : Asset<Asset_Texture>
{
    [XmlIgnore] public bool Loaded;

    // [XmlIgnore] // ignore for now
    // public TextureLoadSettings LoadSettings;

    public Vector2 Size;
    public int TextureId => Handle.Id;

    public void Delete()
    {
        // Tofu.AssetManager.Unload(this, LoadSettings);
    }
}