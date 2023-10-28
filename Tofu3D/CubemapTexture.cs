namespace Tofu3D;

[Serializable]
public class CubemapTexture : Asset<CubemapTexture>
{
    public int TextureId => Handle.Id;
    public bool Loaded;
    public Vector2 Size;

    [XmlIgnore] // ignore for now
    public CubemapTextureLoadSettings LoadSettings;

    public void Delete()
    {
        Tofu.AssetManager.Unload(this, LoadSettings);
    }
}