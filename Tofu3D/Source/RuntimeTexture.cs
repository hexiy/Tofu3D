namespace TofuEngine;

public class RuntimeTexture : Asset<RuntimeTexture>
{
    public Vector4 BoundingBoxInAtlas { get; set; }

    public int AtlasGLTextureArrayId => Tofu.TextureAtlasManager.GLTextureArrayId;
    public int IndexInAtlasTextureArray;


    // public int? StandaloneGLTextureId = null;

    public void Delete()
    {
        // Tofu.AssetManager.Unload(this, LoadSettings);
    }
}