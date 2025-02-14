namespace Tofu3D;

public record struct TextureViewerTextureData : IComparable<TextureViewerTextureData>
{
    public ITexture Texture;
    public string Name;

    public int CompareTo(TextureViewerTextureData other) => string.Compare(Name, other.Name, StringComparison.Ordinal);
}