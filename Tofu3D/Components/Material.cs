using Microsoft.DotNet.PlatformAbstractions;

namespace Scripts;

[Serializable]
public class Material : Asset<Material>
{
    // public bool IsValid = true;
    [Hide] public bool Additive = false;

    public Texture AlbedoTexture;
    public Color AlbedoTint = Color.White;
    public Texture AoTexture;
    public Vector2 Offset;

    public RenderMode RenderMode = RenderMode.Opaque;

    [Hide] public Shader Shader;

    public bool SpecularHighlightsEnabled;
    public float SpecularSmoothness;
    public Vector2 Tiling;

    [XmlIgnore] [Hide] public int Vao;

    public override int GetHashCode()
    {
        var hashCodeCombiner = HashCodeCombiner.Start();
        hashCodeCombiner.Add(base.GetHashCode());
        hashCodeCombiner.Add(Additive.GetHashCode());
        hashCodeCombiner.Add(Shader?.GetHashCode());
        hashCodeCombiner.Add(AlbedoTexture?.GetHashCode());
        hashCodeCombiner.Add(AlbedoTint.GetHashCode());
        hashCodeCombiner.Add(AoTexture?.GetHashCode());
        hashCodeCombiner.Add(Tiling.GetHashCode());
        hashCodeCombiner.Add(Offset.GetHashCode());
        return hashCodeCombiner.CombinedHash;
    }

    public void LoadTextures()
    {
        if (AlbedoTexture?.Path.Length > 2)
        {
            AlbedoTexture = Tofu.AssetManager.Load<Texture>(AlbedoTexture.Path);
        }

        if (AoTexture?.Path.Length > 2)
        {
            AoTexture = Tofu.AssetManager.Load<Texture>(AoTexture.Path);
        }
    }

    public void SetShader(Shader shader)
    {
        Shader = shader;

        if (Shader.IsLoaded == false)
        {
            Shader.Load();
            BufferFactory.CreateBufferForShader(this);
        }
    }

    public void InitShader()
    {
        SetShader(Shader);
    }

    public void Dispose()
    {
        Shader.Dispose();
    }
}