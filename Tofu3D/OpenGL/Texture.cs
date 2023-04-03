namespace Tofu3D;

[Serializable]
public class Texture : Asset<Texture>
{
	public int TextureId
	{
		get { return AssetHandle.Id; }
	}
	public bool Loaded;
	public Vector2 Size;

	[XmlIgnore] // ignore for now
	public TextureLoadSettings LoadSettings;

	/*public Texture Load(string path, TextureLoadSettings loadSettings = null) // when we want to use default load settings but set different path
	{
		loadSettings = loadSettings ?? new TextureLoadSettings(path: path);
		loadSettings.Path = path;
		return Load(loadSettings);
	}

	public Texture Load(TextureLoadSettings loadSettings)
	{
		AssetPath = loadSettings.Path;
		// Paths = loadSettings.Paths;
		Texture loadedTexture = TextureCache.GetTexture(loadSettings);

		InitAssetHandle(loadedTexture.TextureId);
		// TextureId = loadedTexture.TextureId;
		Size = loadedTexture.Size;

		Loaded = true;
		return this;
	}*/

	public void Delete()
	{
		TextureCache.DeleteTexture(AssetPath);
	}
}