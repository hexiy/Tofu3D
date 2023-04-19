using Microsoft.DotNet.PlatformAbstractions;

namespace Tofu3D;

public class MaterialLoadSettings : AssetLoadSettings<Material>
{
	public static MaterialLoadSettings DefaultSettingsTexture2D = new MaterialLoadSettings(path: string.Empty,
	                                                                                       canSetDefaultSettings: false);

	public MaterialLoadSettings(string? path = null,
	                            bool? canSetDefaultSettings = true)
	{
		MaterialLoadSettings defaultSettings = canSetDefaultSettings == true ? DefaultSettingsTexture2D : null; // not possible if this isnt a struct
		this.Path = path ?? defaultSettings?.Path;
	}

	public MaterialLoadSettings()
	{
		MaterialLoadSettings defaultSettings = DefaultSettingsTexture2D; // not possible if this isnt a struct

		this.Path = defaultSettings.Path;
	}

	public override int GetHashCode()
	{
		HashCodeCombiner hashCodeCombiner = HashCodeCombiner.Start();
		hashCodeCombiner.Add(base.GetHashCode());
		return hashCodeCombiner.CombinedHash;
	}
}