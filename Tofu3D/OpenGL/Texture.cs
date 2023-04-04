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

	public void Delete()
	{
		AssetManager.Unload(this);

	}
}