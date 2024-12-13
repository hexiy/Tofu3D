namespace Tofu3D;

public class RuntimeTexture : Asset<RuntimeTexture>
{
    public Vector2 Size { get; set; }
    public int TextureId { get; set; }

    public void Delete()
    {
        // Tofu.AssetManager.Unload(this, LoadSettings);
    }
}