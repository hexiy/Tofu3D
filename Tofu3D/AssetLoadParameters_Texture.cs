public class AssetLoadParameters_Texture : AssetLoadParameters<RuntimeTexture>
{
    public TextureLoadType LoadType  = TextureLoadType.InAtlas;
}

[Flags]
public enum TextureLoadType
{
    Standalone = 0,
    InAtlas =1,
}