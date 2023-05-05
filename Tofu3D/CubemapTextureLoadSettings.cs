using Microsoft.DotNet.PlatformAbstractions;

namespace Tofu3D;

public class CubemapTextureLoadSettings : AssetLoadSettings<CubemapTexture>
{
	public static CubemapTextureLoadSettings DefaultSettings = new CubemapTextureLoadSettings( // paths: new[] {string.Empty},
	                                                                                          flipX: false,
	                                                                                          filterMode: TextureFilterMode.Bilinear,
	                                                                                          wrapMode: TextureWrapMode.ClampToEdge,
	                                                                                          canSetDefaultSettings: false);

	// public TextureType Type { get; private set; }
	public string[] Paths { get; private set; }
	public TextureFilterMode FilterMode { get; private set; }
	public TextureWrapMode WrapMode { get; private set; }
	public bool FlipX { get; private set; }

	public CubemapTextureLoadSettings(string[]? paths = null, // set only one element for single texture cubemaps
	                                  bool? flipX = null,
	                                  TextureFilterMode? filterMode = null,
	                                  TextureWrapMode? wrapMode = null,
	                                  bool? canSetDefaultSettings = true)
	{
		CubemapTextureLoadSettings defaultSettings = canSetDefaultSettings == true ? DefaultSettings : null;
		/*if (path != null)
		{
			SetPath(path);
		}

		if (paths != null)
		{
			SetPaths(paths);
		}*/

		this.Paths = paths ?? defaultSettings?.Paths;
		this.FilterMode = filterMode ?? defaultSettings.FilterMode;
		this.WrapMode = wrapMode ?? defaultSettings.WrapMode;
		this.FlipX = flipX ?? defaultSettings.FlipX;
	}

	public CubemapTextureLoadSettings()
	{
		CubemapTextureLoadSettings defaultSettings = DefaultSettings;


		this.Paths = defaultSettings.Paths;
		// this.Paths = defaultSettings.Paths;
		this.FilterMode = defaultSettings.FilterMode;
		this.WrapMode = defaultSettings.WrapMode;
		this.FlipX = defaultSettings.FlipX;
	}

	// differentiate between textures based on their load settings
	public override int GetHashCode()
	{
		HashCodeCombiner hashCodeCombiner = HashCodeCombiner.Start();
		hashCodeCombiner.Add(base.GetHashCode());
		foreach (string path in Paths)
		{
			hashCodeCombiner.Add(path.GetHashCode());
		}

		hashCodeCombiner.Add(FilterMode.GetHashCode());
		hashCodeCombiner.Add(WrapMode.GetHashCode());
		hashCodeCombiner.Add(FlipX.GetHashCode());
		return hashCodeCombiner.CombinedHash;
	}
}