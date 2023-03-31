namespace Tofu3D;

[Serializable]
public class Texture
{
	public int Id;
	public bool Loaded;
	public string Path = "";
	public string[] Paths;
	public Vector2 Size;

	[XmlIgnore] // ignore for now
	public TextureLoadSettings LoadSettings;

	public Texture Load(string path, TextureLoadSettings loadSettings) // when we want to use default load settings but set different path
	{
		TextureLoadSettings textureLoadSettings = new TextureLoadSettings(path: path, loadSettings: loadSettings);
		return Load(textureLoadSettings);
	}

	public Texture Load(string path)
	{
		TextureLoadSettings textureLoadSettings = new TextureLoadSettings(path: path);
		return Load(textureLoadSettings);
	}

	public Texture Load(TextureLoadSettings loadSettings)
	{
		Path = loadSettings.Path;
		Paths = loadSettings.Paths;
		Texture loadedTexture = TextureCache.GetTexture(loadSettings);

		Id = loadedTexture.Id;
		Size = loadedTexture.Size;

		Loaded = true;
		return this;
	}

	public void Delete()
	{
		TextureCache.DeleteTexture(Path);
	}
}