namespace Tofu3D;

[Serializable]
public class Texture : Asset<Texture>
{
    public int TextureId => Handle.Id;

    [XmlIgnore]
    public bool Loaded;

    public Vector2 Size;

    [XmlIgnore] // ignore for now
    public TextureLoadSettings LoadSettings;

    public void Delete()
    {
        Tofu.AssetManager.Unload(this, LoadSettings);
    }
}