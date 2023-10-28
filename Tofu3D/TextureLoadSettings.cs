using Microsoft.DotNet.PlatformAbstractions;

namespace Tofu3D;

public class TextureLoadSettings : AssetLoadSettings<Texture>
{
    public static TextureLoadSettings DefaultSettingsTexture2D = new(string.Empty,
        filterMode: TextureFilterMode.Bilinear,
        wrapMode: TextureWrapMode.Repeat,
        canSetDefaultSettings: false);

    public static TextureLoadSettings DefaultSettingsSpritePixelArt = new(string.Empty,
        filterMode: TextureFilterMode.Point,
        wrapMode: TextureWrapMode.Repeat,
        canSetDefaultSettings: false);

    public TextureFilterMode FilterMode { get; private set; }
    public TextureWrapMode WrapMode { get; private set; }

    public TextureLoadSettings(string? path = null,
        string[]? paths = null,
        TextureType? textureType = null,
        TextureFilterMode? filterMode = null,
        TextureWrapMode? wrapMode = null,
        bool? canSetDefaultSettings = true)
    {
        TextureLoadSettings defaultSettings =
            canSetDefaultSettings == true
                ? GetDefaultSettingsForTextureType(textureType)
                : null; // not possible if this isnt a struct

        Path = path ?? defaultSettings?.Path;
        FilterMode = filterMode ?? defaultSettings.FilterMode;
        WrapMode = wrapMode ?? defaultSettings.WrapMode;
    }

    public TextureLoadSettings()
    {
        TextureLoadSettings
            defaultSettings = GetDefaultSettingsForTextureType(null); // not possible if this isnt a struct


        Path = defaultSettings.Path;
        FilterMode = defaultSettings.FilterMode;
        WrapMode = defaultSettings.WrapMode;
    }

    private static TextureLoadSettings GetDefaultSettingsForTextureType(TextureType? textureType)
    {
        if (textureType == TextureType.Texture2D) return DefaultSettingsTexture2D;

        return DefaultSettingsTexture2D;
    }

    public override int GetHashCode()
    {
        HashCodeCombiner hashCodeCombiner = HashCodeCombiner.Start();
        hashCodeCombiner.Add(base.GetHashCode());
        hashCodeCombiner.Add(FilterMode.GetHashCode());
        hashCodeCombiner.Add(WrapMode.GetHashCode());
        return hashCodeCombiner.CombinedHash;
    }
}