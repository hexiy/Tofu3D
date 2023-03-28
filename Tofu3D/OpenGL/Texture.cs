namespace Tofu3D;

[Serializable]
public class Texture
{
	public int Id;
	public bool Loaded;
	public string Path = "";
	public Vector2 Size;

	public Texture Load(string path, bool flipX = true, bool smooth = false)
	{
		Path = path;
		Texture loadedTexture = TextureCache.GetTexture(path, flipX, smooth);

		Id = loadedTexture.Id;
		Size = loadedTexture.Size;

		Loaded = true;
		return this;
	}

	public Texture LoadCubemap(string path)
	{
		Path = path;
		Texture loadedTexture = TextureCache.GetCubemapTexture(path);

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