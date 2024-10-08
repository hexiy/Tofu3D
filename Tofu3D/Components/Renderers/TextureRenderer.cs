using System.IO;

public abstract class TextureRenderer : Renderer
{
    public Vector2 Offset = Vector2.Zero;

    [XmlIgnore] public Action SetNativeSize;

    public Asset_Texture Texture;
    public Vector2 Tiling = Vector2.One;
    public override bool CanRender => Texture.Loaded; // && BoxShape != null;// && base.CanRender;


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
            Texture = new Asset_Texture();
        }

        Texture = Tofu.AssetManager.Load<Asset_Texture>(texturePath);
    }

    public virtual void SetDefaultTexture(string texturePath)
    {
        if (Texture == null)
        {
            Texture = new Asset_Texture();
            Texture.Path = texturePath;
        }
    }
}