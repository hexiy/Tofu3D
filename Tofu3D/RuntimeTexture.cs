namespace Tofu3D;

public class RuntimeTexture : Asset<RuntimeTexture>
{
    public Vector4 BoundingBoxInAtlas;
    public int AtlasGLTextureId;

    public void Delete()
    {
        // Tofu.AssetManager.Unload(this, LoadSettings);
    }
}