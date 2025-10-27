using Microsoft.DotNet.PlatformAbstractions;

public class Asset_Material : Asset<Asset_Material>
{
    public Shader? Shader;

    // public bool IsValid = true;
    [Hide]
    public bool Additive = false;

    public RuntimeTexture? AlbedoTexture;
    public RuntimeTexture? AlphaMaskTexture;
    public Color AlbedoColor = Color.White;
    public bool ObjectSelected = false;
    public RuntimeTexture? AmbientOcclusionTexture;
    public RuntimeTexture? NormalTexture;
    public RuntimeTexture? RoughnessTexture;
    public RuntimeTexture? MetallicTexture;
    public RuntimeTexture? EmissiveTexture;

    [ColorHDR]
    public Vector4 EmissiveColor = Vector4.Zero;

    public bool SmoothShadows = false;

    [Space]
    public bool RefractionEnabled = false;

    public float RefractiveIndex = 1.309f;

    // [Space]
    // public bool SpecularHighlightsEnabled;

    // public float SpecularSmoothness;

    [SliderF(0, 1)]
    public float MetallicTextureStrength;

    [SliderF(0, 1)]
    public float Smoothness;

    [Space]
    public Vector2 Tiling = new Vector2(1, 1);

    public Vector2 Offset = new Vector2(0, 0);
    public bool UVOffsetIsInstanced = false;

    public RenderMode RenderMode = RenderMode.Opaque;
    public BlendMode BlendMode = BlendMode.Opaque;
    public MaterialType MaterialType = MaterialType.Lit;

    [Space]
    public bool NoDepth = false;
    [Space]
    public bool DepthOverrideEnabled { get; set; } = false;
    public float DepthOverride { get; set; } = 0;
    
    public override int GetHashCode()
    {
        HashCodeCombiner hashCodeCombiner = HashCodeCombiner.Start();
        hashCodeCombiner.Add(base.GetHashCode());
        hashCodeCombiner.Add(Additive.GetHashCode());
        hashCodeCombiner.Add(Shader?.GetHashCode());
        hashCodeCombiner.Add(PathInLibraryFolder.GetHashCode());
        hashCodeCombiner.Add(PathInAssetsFolder.GetHashCode());
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
        if (AlbedoTexture?.PathInLibraryFolder.Length > 2)
        {
            AlbedoTexture = Tofu.AssetLoadManager.Get<RuntimeTexture>(new AssetLoadParameters_RuntimeTexture()
                { PathToAssetInLibrary = AlbedoTexture.PathInLibraryFolder });
        }

        if (AlphaMaskTexture?.PathInLibraryFolder.Length > 2)
        {
            AlphaMaskTexture = Tofu.AssetLoadManager.Get<RuntimeTexture>(new AssetLoadParameters_RuntimeTexture()
                { PathToAssetInLibrary = AlphaMaskTexture.PathInLibraryFolder });
        }

        if (AmbientOcclusionTexture?.PathInLibraryFolder.Length > 2)
        {
            AmbientOcclusionTexture =
                Tofu.AssetLoadManager.Get<RuntimeTexture>(AmbientOcclusionTexture.PathInLibraryFolder);
        }

        if (NormalTexture?.PathInLibraryFolder.Length > 2)
        {
            NormalTexture = Tofu.AssetLoadManager.Get<RuntimeTexture>(NormalTexture.PathInLibraryFolder);
        }

        if (RoughnessTexture?.PathInLibraryFolder.Length > 2)
        {
            RoughnessTexture = Tofu.AssetLoadManager.Get<RuntimeTexture>(RoughnessTexture.PathInLibraryFolder);
        }

        if (MetallicTexture?.PathInLibraryFolder.Length > 2)
        {
            MetallicTexture = Tofu.AssetLoadManager.Get<RuntimeTexture>(MetallicTexture.PathInLibraryFolder);
        }

        if (EmissiveTexture?.PathInLibraryFolder.Length > 2)
        {
            EmissiveTexture = Tofu.AssetLoadManager.Get<RuntimeTexture>(new AssetLoadParameters_RuntimeTexture()
                { PathToAssetInLibrary = EmissiveTexture.PathInLibraryFolder });
        }
    }

    public void LoadShader()
    {
        Shader = Tofu.ShaderManager.LoadShader(Shader.Path);
        // Shader.Load(/*this*/);
    }

    public void Dispose()
    {
        // Shader.Dispose();
    }
}