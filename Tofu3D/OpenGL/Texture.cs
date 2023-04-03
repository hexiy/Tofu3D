namespace Tofu3D;

[Serializable]
public class Texture : Asset<Texture>
{
	public int TextureId;
	public bool Loaded;
	public Vector2 Size;

	[XmlIgnore] // ignore for now
	public TextureLoadSettings LoadSettings;

	public Texture Load(string path, TextureLoadSettings loadSettings) // when we want to use default load settings but set different path
	{
		loadSettings.Path = path;
		return Load(loadSettings);
	}

	public Texture Load(string path)
	{
		TextureLoadSettings textureLoadSettings = new TextureLoadSettings(path: path);
		return Load(textureLoadSettings);
	}

	public Texture Load(TextureLoadSettings loadSettings)
	{
		AssetPath = loadSettings.Path;
		// Paths = loadSettings.Paths;
		Texture loadedTexture = TextureCache.GetTexture(loadSettings);

		TextureId = loadedTexture.TextureId;
		Size = loadedTexture.Size;

		Loaded = true;
		return this;
	}

	public void Delete()
	{
		TextureCache.DeleteTexture(AssetPath);
	}
}