namespace Tofu3D;

[Serializable]
public class Texture : Asset<Texture>
{
	public int TextureId
	{
		get { return AssetRuntimeHandle.Id; }
	}
	public bool Loaded;
	public Vector2 Size;

	[XmlIgnore] // ignore for now
	public TextureLoadSettings LoadSettings;

	public void Delete()
	{
		AssetManager.Unload(this);
	}

	public void BindTexture()
	{
		TextureTarget textureTarget = LoadSettings.Type == TextureType.Texture2D ? TextureTarget.Texture2D : TextureTarget.TextureCubeMap;
		GL.BindTexture(textureTarget, TextureId);
	}
}