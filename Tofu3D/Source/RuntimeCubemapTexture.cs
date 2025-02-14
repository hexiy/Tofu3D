namespace TofuEngine;

public class RuntimeCubemapTexture : Asset<RuntimeCubemapTexture>
{
    public bool Loaded;

    // [XmlIgnore] // ignore for now
    // public CubemapTextureLoadSettings LoadSettings;

    public Vector2 Size;
    public int TextureId;

    public void Delete()
    {
        // Tofu.AssetManager.Unload(this, LoadSettings);
    }
}