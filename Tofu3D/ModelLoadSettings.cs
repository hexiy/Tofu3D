using Microsoft.DotNet.PlatformAbstractions;

namespace Tofu3D;

public class ModelLoadSettings : AssetLoadSettings<Mesh>
{
	public static ModelLoadSettings DefaultSettingsModel = new ModelLoadSettings(path: string.Empty);

	public ModelLoadSettings(string? path = null,
	                         bool? canSetDefaultSettings = true)
	{
		ModelLoadSettings defaultSettings = canSetDefaultSettings == true ? DefaultSettingsModel : null; // not possible if this isnt a struct

		this.Path = path ?? defaultSettings?.Path;
	}

	public ModelLoadSettings()
	{
		ModelLoadSettings defaultSettings = DefaultSettingsModel; // not possible if this isnt a struct
		this.Path = defaultSettings.Path;
	}

	public override int GetHashCode()
	{
		HashCodeCombiner hashCodeCombiner = HashCodeCombiner.Start();
		hashCodeCombiner.Add(base.GetHashCode());
		return hashCodeCombiner.CombinedHash;
	}
}