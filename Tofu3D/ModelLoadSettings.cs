using Microsoft.DotNet.PlatformAbstractions;

namespace Tofu3D;

public class ModelLoadSettings : AssetLoadSettings<Mesh>
{
    public static ModelLoadSettings DefaultSettingsModel = new(string.Empty);

    public ModelLoadSettings(string? path = null,
        bool? canSetDefaultSettings = true)
    {
        var
            defaultSettings =
                canSetDefaultSettings == true ? DefaultSettingsModel : null; // not possible if this isnt a struct

        Path = path ?? defaultSettings?.Path;
    }

    public ModelLoadSettings()
    {
        var defaultSettings = DefaultSettingsModel; // not possible if this isnt a struct
        Path = defaultSettings.Path;
    }

    public override int GetHashCode()
    {
        var hashCodeCombiner = HashCodeCombiner.Start();
        hashCodeCombiner.Add(base.GetHashCode());
        return hashCodeCombiner.CombinedHash;
    }
}