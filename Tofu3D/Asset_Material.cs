using Microsoft.DotNet.PlatformAbstractions;

[Serializable]
[XmlRoot("Material")]
public class Asset_Material : Asset<Asset_Material>
{
    public Shader Shader;
    // public bool IsValid = true;
    [Hide] public bool Additive = false;

    public RuntimeTexture AlbedoTexture;
    public Color AlbedoTint = Color.White;
    public RuntimeTexture AmbientOcclusionTexture;
    public RuntimeTexture NormalTexture;
    public Vector2 Offset;

    public RenderMode RenderMode = RenderMode.Opaque;


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
        hashCodeCombiner.Add(AmbientOcclusionTexture?.GetHashCode());
        hashCodeCombiner.Add(NormalTexture?.GetHashCode());
        hashCodeCombiner.Add(Tiling.GetHashCode());
        hashCodeCombiner.Add(Offset.GetHashCode());
        return hashCodeCombiner.CombinedHash;
    }

    public void LoadTextures()
    {
        if (AlbedoTexture?.PathToRawAsset.Length > 2)
        {
            AlbedoTexture = Tofu.AssetLoadManager.Load<RuntimeTexture>(AlbedoTexture.PathToRawAsset);
        }

        if (AmbientOcclusionTexture?.PathToRawAsset.Length > 2)
        {
            AmbientOcclusionTexture = Tofu.AssetLoadManager.Load<RuntimeTexture>(AmbientOcclusionTexture.PathToRawAsset);
        }
        
        if (NormalTexture?.PathToRawAsset.Length > 2)
        {
            NormalTexture = Tofu.AssetLoadManager.Load<RuntimeTexture>(NormalTexture.PathToRawAsset);
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