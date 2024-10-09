namespace Tofu3D;

public class RuntimeTexture : Asset<RuntimeTexture>
{
    public Vector2 Size;
    public int TextureId;
    public void Delete()
    {
        // Tofu.AssetManager.Unload(this, LoadSettings);
    }
}