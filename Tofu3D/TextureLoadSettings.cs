namespace Tofu3D;

public struct TextureLoadSettings
{
	public TextureType Type { get; private set; }
	public string[] Paths { get; private set; }
	public string Path { get; private set; }
	public TextureFilterMode FilterMode { get; private set; }
	public TextureWrapMode WrapMode { get; private set; }
	public bool FlipX { get; private set; }

	public TextureLoadSettings(string? path = null,
	                           string[]? paths = null,
	                           TextureType? textureType = null,
	                           bool? flipX = null,
	                           TextureFilterMode? filterMode = null,
	                           TextureWrapMode? wrapMode = null)
	{
		this = GetDefaultSettingsForTextureType(textureType);
		if (path != null)
		{
			SetPath(path);
		}

		if (paths != null)
		{
			SetPaths(paths);
		}

		this.Type = textureType ?? this.Type;
		this.Path = path ?? this.Path;
		this.Paths = paths ?? this.Paths;
		this.FilterMode = filterMode ?? this.FilterMode;
		this.WrapMode = wrapMode ?? this.WrapMode;
		this.FlipX = flipX ?? this.FlipX;
	}

	public TextureLoadSettings(string path,
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
	}

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

	public static TextureLoadSettings DefaultSettingsTexture2D { get; } = new TextureLoadSettings(path: string.Empty,
	                                                                                              paths: null,
	                                                                                              flipX: false,
	                                                                                              filterMode: TextureFilterMode.Bilinear,
	                                                                                              wrapMode: TextureWrapMode.Repeat,
	                                                                                              textureType: TextureType.Texture2D);

	public static TextureLoadSettings DefaultSettingsCubemap { get; } = new TextureLoadSettings(path: null,
	                                                                                            paths: new[] {string.Empty},
	                                                                                            flipX: true,
	                                                                                            filterMode: TextureFilterMode.Bilinear,
	                                                                                            wrapMode: TextureWrapMode.ClampToEdge,
	                                                                                            textureType: TextureType.Cubemap);

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
}