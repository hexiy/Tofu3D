namespace Tofu3D;

public class ModelLoadSettings : AssetLoadSettings<Model>
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
}