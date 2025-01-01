using Microsoft.DotNet.PlatformAbstractions;

public class Asset_Material : Asset<Asset_Material>
{
    public Shader? Shader;

    // public bool IsValid = true;
    [Hide]
    public bool Additive = false;

    public RuntimeTexture? AlbedoTexture;
    public Color AlbedoTint = Color.White;
    public RuntimeTexture? AmbientOcclusionTexture;
    public RuntimeTexture? NormalTexture;
    public RuntimeTexture? RoughnessTexture;
    public RuntimeTexture? MetallicTexture;
    public RuntimeTexture? EmissiveTexture;

    [ColorHDR]
    public Vector4 EmissiveColor;


    public bool RefractionEnabled=false;
    public float RefractiveIndex=1.309f;
    
    public bool SpecularHighlightsEnabled;
    public float SpecularSmoothness;

    [SliderF(0, 1)]
    public float MetallicTextureStrength;

    [SliderF(0, 1)]
    public float Smoothness;

    public Vector2 Tiling = new Vector2(1, 1);
    public Vector2 Offset = new Vector2(0, 0);
    public bool UVOffsetIsInstanced = false;

    public RenderMode RenderMode = RenderMode.Opaque;
    public BlendMode BlendMode = BlendMode.Opaque;

    public override int GetHashCode()
    {
        var hashCodeCombiner = HashCodeCombiner.Start();
        hashCodeCombiner.Add(base.GetHashCode());
        hashCodeCombiner.Add(Additive.GetHashCode());
        hashCodeCombiner.Add(Shader?.GetHashCode());
        hashCodeCombiner.Add(Path.GetHashCode());
        hashCodeCombiner.Add(AlbedoTexture?.GetHashCode());
        // hashCodeCombiner.Add(AlbedoTint.GetHashCode());
        hashCodeCombiner.Add(AmbientOcclusionTexture?.GetHashCode());
        hashCodeCombiner.Add(NormalTexture?.GetHashCode());
        // hashCodeCombiner.Add(Tiling.GetHashCode());
        // hashCodeCombiner.Add(Offset.GetHashCode());
        return hashCodeCombiner.CombinedHash;
    }

    public void LoadTextures()
    {
        if (AlbedoTexture?.Path.Length > 2)
        {
            AlbedoTexture = Tofu.AssetLoadManager.Load<RuntimeTexture>(new AssetLoadParameters_Texture()
                { PathToAssetInLibrary = AlbedoTexture.Path });
        }

        if (AmbientOcclusionTexture?.Path.Length > 2)
        {
            AmbientOcclusionTexture =
                Tofu.AssetLoadManager.Load<RuntimeTexture>(AmbientOcclusionTexture.Path);
        }

        if (NormalTexture?.Path.Length > 2)
        {
            NormalTexture = Tofu.AssetLoadManager.Load<RuntimeTexture>(NormalTexture.Path);
        }

        if (RoughnessTexture?.Path.Length > 2)
        {
            RoughnessTexture = Tofu.AssetLoadManager.Load<RuntimeTexture>(RoughnessTexture.Path);
        }

        if (MetallicTexture?.Path.Length > 2)
        {
            MetallicTexture = Tofu.AssetLoadManager.Load<RuntimeTexture>(MetallicTexture.Path);
        }

        if (EmissiveTexture?.Path.Length > 2)
        {
            EmissiveTexture = Tofu.AssetLoadManager.Load<RuntimeTexture>(new AssetLoadParameters_Texture()
                { PathToAssetInLibrary = EmissiveTexture.Path });
        }
    }

    public void SetAndLoadShader(Shader shader)
    {
        Shader = shader;

        if (Shader.IsLoaded == false)
        {
            LoadShader();
            // BufferFactory.CreateBufferForShader(this);
        }
    }

    public void LoadShader()
    {
        Shader.Load(this);
    }

    public void Dispose()
    {
        Shader.Dispose();
    }
}