public class AssetLoadParameters_Texture : AssetLoadParameters<RuntimeTexture>
{
    public TextureFilterMode FilterMode { get; } = TextureFilterMode.Bilinear;
    public TextureWrapMode WrapMode { get; } = TextureWrapMode.Repeat;
}