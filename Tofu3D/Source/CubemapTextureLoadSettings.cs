/*using Microsoft.DotNet.PlatformAbstractions;

namespace Tofu3D;

public class CubemapTextureLoadSettings : AssetLoadSettings<CubemapTexture>
{
    public static CubemapTextureLoadSettings DefaultSettings = new( // paths: new[] {string.Empty},
        filterMode: TextureFilterMode.Bilinear,
        wrapMode: TextureWrapMode.ClampToEdge,
        canSetDefaultSettings: false);

    public CubemapTextureLoadSettings(string[]? paths = null, // set only one element for single texture cubemaps
        TextureFilterMode? filterMode = null,
        TextureWrapMode? wrapMode = null,
        bool? canSetDefaultSettings = true)
    {
        var defaultSettings = canSetDefaultSettings == true ? DefaultSettings : null;
        /*if (path != null)
        {
            SetPath(path);
        }

        if (paths != null)
        {
            SetPaths(paths);
        }#1#

        Paths = paths ?? defaultSettings?.Paths;
        FilterMode = filterMode ?? defaultSettings.FilterMode;
        WrapMode = wrapMode ?? defaultSettings.WrapMode;
    }

    public CubemapTextureLoadSettings()
    {
        var defaultSettings = DefaultSettings;


        Paths = defaultSettings.Paths;
        // this.Paths = defaultSettings.Paths;
        FilterMode = defaultSettings.FilterMode;
        WrapMode = defaultSettings.WrapMode;
    }

    // public TextureType Type { get; private set; }
    public string[] Paths { get; }
    public TextureFilterMode FilterMode { get; }
    public TextureWrapMode WrapMode { get; }

    // differentiate between textures based on their load settings
    public override int GetHashCode()
    {
        var hashCodeCombiner = HashCodeCombiner.Start();
        hashCodeCombiner.Add(base.GetHashCode());
        foreach (var path in Paths)
        {
            hashCodeCombiner.Add(path.GetHashCode());
        }

        hashCodeCombiner.Add(FilterMode.GetHashCode());
        hashCodeCombiner.Add(WrapMode.GetHashCode());
        return hashCodeCombiner.CombinedHash;
    }
}*/