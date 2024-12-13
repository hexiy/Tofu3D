using Microsoft.DotNet.PlatformAbstractions;
using OpenTK.Mathematics;

[Serializable]
[XmlRoot("Material")]
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
        hashCodeCombiner.Add(PathToRawAsset.GetHashCode());
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
            AlbedoTexture = Tofu.AssetLoadManager.Load<RuntimeTexture>(new AssetLoadParameters_Texture()
                { PathToAsset = AlbedoTexture.PathToRawAsset });
        }
        else
        {
            AlbedoTexture = Tofu.AssetLoadManager.Load<RuntimeTexture>(new AssetLoadParameters_Texture()
                { PathToAsset = "Resources/whitePixel.png" });
        }

        if (AmbientOcclusionTexture?.PathToRawAsset.Length > 2)
        {
            AmbientOcclusionTexture =
                Tofu.AssetLoadManager.Load<RuntimeTexture>(AmbientOcclusionTexture.PathToRawAsset);
        }

        if (NormalTexture?.PathToRawAsset.Length > 2)
        {
            NormalTexture = Tofu.AssetLoadManager.Load<RuntimeTexture>(NormalTexture.PathToRawAsset);
        }

        if (RoughnessTexture?.PathToRawAsset.Length > 2)
        {
            RoughnessTexture = Tofu.AssetLoadManager.Load<RuntimeTexture>(RoughnessTexture.PathToRawAsset);
        }

        if (MetallicTexture?.PathToRawAsset.Length > 2)
        {
            MetallicTexture = Tofu.AssetLoadManager.Load<RuntimeTexture>(MetallicTexture.PathToRawAsset);
        }

        if (EmissiveTexture?.PathToRawAsset.Length > 2)
        {
            EmissiveTexture = Tofu.AssetLoadManager.Load<RuntimeTexture>(new AssetLoadParameters_Texture()
                { PathToAsset = EmissiveTexture.PathToRawAsset });
        }
        else
        {
            EmissiveTexture = Tofu.AssetLoadManager.Load<RuntimeTexture>(new AssetLoadParameters_Texture()
                { PathToAsset = "Resources/whitePixel.png" });
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