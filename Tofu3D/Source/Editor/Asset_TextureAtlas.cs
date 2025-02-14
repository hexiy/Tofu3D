namespace TofuEngine;

[Serializable]
public class Asset_TextureAtlas : Asset_Texture
{
    public TextureAtlasMember[] TextureAtlasMembers;
    public int IndexInTextureArray;
}