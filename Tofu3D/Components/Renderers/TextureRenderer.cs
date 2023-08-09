using System.IO;

public class TextureRenderer : Renderer
{
	[XmlIgnore]
	public Action SetNativeSize;
	public Texture Texture;
	public Vector2 Tiling = Vector2.One;
	public Vector2 Offset = Vector2.Zero;
	public override bool CanRender => Texture.Loaded;// && BoxShape != null;// && base.CanRender;


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

		Texture = Tofu.I.AssetManager.Load<Texture>(texturePath);
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