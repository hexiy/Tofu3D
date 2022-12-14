using System.IO;

public class TextureRenderer : Renderer
{
	[XmlIgnore]
	public Action SetNativeSize;
	public Texture Texture;
	public Vector2 Repeats = Vector2.One;

	public virtual void LoadTexture(string texturePath)
	{
		if (texturePath.Contains("Assets") == false)
		{
			texturePath = Path.Combine("Assets", texturePath);
		}

		if (File.Exists(texturePath) == false)
		{
			return;
		}

		if (Texture == null)
		{
			Texture = new Texture();
		}

		Texture.Load(texturePath);
	}

	public virtual void SetDefaultTexture(string texturePath)
	{
		if (Texture == null)
		{
			Texture = new Texture();
			Texture.Path = texturePath;
		}
	}
}