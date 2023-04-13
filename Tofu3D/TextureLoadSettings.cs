using Microsoft.DotNet.PlatformAbstractions;

namespace Tofu3D;

public class TextureLoadSettings : AssetLoadSettings<Texture>
{
	public static TextureLoadSettings DefaultSettingsTexture2D = new TextureLoadSettings(path: string.Empty,
	                                                                                     // paths: null,
	                                                                                     flipX: false,
	                                                                                     filterMode: TextureFilterMode.Bilinear,
	                                                                                     wrapMode: TextureWrapMode.Repeat,
	                                                                                     textureType: TextureType.Texture2D,
	                                                                                     canSetDefaultSettings: false);

	public static TextureLoadSettings DefaultSettingsSpritePixelArt = new TextureLoadSettings(path: string.Empty,
	                                                                                          // paths: null,
	                                                                                          flipX: false,
	                                                                                          filterMode: TextureFilterMode.Point,
	                                                                                          wrapMode: TextureWrapMode.Repeat,
	                                                                                          textureType: TextureType.Texture2D,
	                                                                                          canSetDefaultSettings: false);

	public static TextureLoadSettings DefaultSettingsCubemap = new TextureLoadSettings(path: string.Empty,
	                                                                                   // paths: new[] {string.Empty},
	                                                                                   flipX: true,
	                                                                                   filterMode: TextureFilterMode.Bilinear,
	                                                                                   wrapMode: TextureWrapMode.ClampToEdge,
	                                                                                   textureType: TextureType.Cubemap,
	                                                                                   canSetDefaultSettings: false);

	public TextureType Type { get; private set; }
	// public string[] Paths { get; private set; }
	public TextureFilterMode FilterMode { get; private set; }
	public TextureWrapMode WrapMode { get; private set; }
	public bool FlipX { get; private set; }

	public TextureLoadSettings(string? path = null,
	                           string[]? paths = null,
	                           TextureType? textureType = null,
	                           bool? flipX = null,
	                           TextureFilterMode? filterMode = null,
	                           TextureWrapMode? wrapMode = null,
	                           bool? canSetDefaultSettings = true)
	{
		TextureLoadSettings defaultSettings = canSetDefaultSettings == true ? GetDefaultSettingsForTextureType(textureType) : null; // not possible if this isnt a struct
		/*if (path != null)
		{
			SetPath(path);
		}

		if (paths != null)
		{
			SetPaths(paths);
		}*/

		this.Type = textureType ?? defaultSettings.Type;
		this.Path = path ?? defaultSettings?.Path;
		// this.Paths = paths ?? defaultSettings?.Paths;
		this.FilterMode = filterMode ?? defaultSettings.FilterMode;
		this.WrapMode = wrapMode ?? defaultSettings.WrapMode;
		this.FlipX = flipX ?? defaultSettings.FlipX;
	}

	public TextureLoadSettings()
	{
		TextureLoadSettings defaultSettings = GetDefaultSettingsForTextureType(null); // not possible if this isnt a struct


		this.Type = defaultSettings.Type;
		this.Path = defaultSettings.Path;
		// this.Paths = defaultSettings.Paths;
		this.FilterMode = defaultSettings.FilterMode;
		this.WrapMode = defaultSettings.WrapMode;
		this.FlipX = defaultSettings.FlipX;
	}

	/*public TextureLoadSettings(string path,
	                           TextureLoadSettings loadSettings)
	{
		this = loadSettings;
		SetPath(path);
	}

	public TextureLoadSettings(string[] paths,
	                           TextureLoadSettings loadSettings)
	{
		this = loadSettings;
		SetPaths(paths);
	}

	public TextureLoadSettings(TextureLoadSettings loadSettings)
	{
		this = loadSettings;
	}*/

	/*
	private void SetPath(string path)
	{
		this.Path = path;
		this.Paths = new[] {path};
	}

	private void SetPaths(string[] paths)
	{
		this.Paths = paths;
		this.Path = paths[0];
	}
	*/

	private static TextureLoadSettings GetDefaultSettingsForTextureType(TextureType? textureType)
	{
		if (textureType == TextureType.Texture2D)
		{
			return DefaultSettingsTexture2D;
		}

		if (textureType == TextureType.Cubemap)
		{
			return DefaultSettingsCubemap;
		}

		return DefaultSettingsTexture2D;
	}

	public override int GetHashCode()
	{
		HashCodeCombiner hashCodeCombiner = HashCodeCombiner.Start();
		hashCodeCombiner.Add(base.GetHashCode());
		hashCodeCombiner.Add(FilterMode.GetHashCode());
		hashCodeCombiner.Add(WrapMode.GetHashCode());
		hashCodeCombiner.Add(FlipX.GetHashCode());
		return hashCodeCombiner.CombinedHash;
	}
}