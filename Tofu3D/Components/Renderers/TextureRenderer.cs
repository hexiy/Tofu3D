/*using System.IO;

public abstract class TextureRenderer : Renderer
{
    public Vector2 Offset = Vector2.Zero;

    [XmlIgnore] public Action SetNativeSize;

    public RuntimeTexture Texture;
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
            Texture = new RuntimeTexture();
        }

        Texture = Tofu.AssetLoadManager.Load<RuntimeTexture>(texturePath);
    }

    public virtual void SetDefaultTexture(string texturePath)
    {
        if (Texture == null)
        {
            Texture = new RuntimeTexture();
            Texture.PathToRawAsset = texturePath;
        }
    }
}*/