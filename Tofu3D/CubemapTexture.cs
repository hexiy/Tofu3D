namespace Tofu3D;

[Serializable]
public class CubemapTexture : Asset<CubemapTexture>
{
    public bool Loaded;

    // [XmlIgnore] // ignore for now
    // public CubemapTextureLoadSettings LoadSettings;

    public Vector2 Size;
    public int TextureId => RuntimeAssetHandle.Id;

    public void Delete()
    {
        // Tofu.AssetManager.Unload(this, LoadSettings);
    }
}