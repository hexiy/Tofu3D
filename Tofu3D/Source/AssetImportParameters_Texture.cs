public class AssetImportParameters_Texture : AssetImportParameters<Asset_Texture>
{
    public TextureFilterMode FilterMode { get; set; } = TextureFilterMode.Bilinear;

    public TextureWrapMode WrapMode { get; set; } = TextureWrapMode.Repeat;
    public bool IsSrgb { get; set; } = false;
    public bool BlackIsTransparency { get; set; } = false;
}