public class AssetLoadParameters_CubemapTexture :  AssetLoadParameters<RuntimeCubemapTexture>
{
    public string[] PathsToSourceTextures;
    public TextureFilterMode FilterMode { get; } = TextureFilterMode.Bilinear;
    public TextureWrapMode WrapMode { get; } = TextureWrapMode.ClampToEdge;
}