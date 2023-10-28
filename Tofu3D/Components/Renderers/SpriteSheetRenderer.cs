using System.IO;

namespace Scripts;

public class SpriteSheetRenderer : SpriteRenderer
{
    [Hide]
    public int CurrentSpriteIndex;

    private Vector2 _spritesCount = new(1, 1);

    [Hide]
    public Vector2 SpriteSize;

    [Hide]
    public override bool Batched { get; set; } = true;

    public Vector2 SpritesCount
    {
        get => _spritesCount;
        set
        {
            _spritesCount = value;
            if (Texture != null)
                SpriteSize = new Vector2(Texture.Size.X / SpritesCount.X, Texture.Size.Y / SpritesCount.Y);
        }
    }

    public override void SetDefaultMaterial()
    {
        if (Material == null) Material = Tofu.AssetManager.Load<Material>("SpriteSheetRenderer");

        base.SetDefaultMaterial();
    }

    internal override void UpdateBoxShapeSize()
    {
        if (BoxShape != null)
            // find size closes to 1
            BoxShape.Size = SpriteSize.Normalized() * 5;
    }

    public override void OnNewComponentAdded(Component comp)
    {
    }

    public override void LoadTexture(string texturePath)
    {
        if (texturePath.Contains("Assets") == false) texturePath = Path.Combine("Assets", texturePath);

        if (File.Exists(texturePath) == false) return;

        Texture = Tofu.AssetManager.Load<Texture>(texturePath);

        UpdateBoxShapeSize();
        if (Batched && false)
        {
            //BatchingManager.AddObjectToBatcher(texture.id, this);
        }
    }

    public override void Render()
    {
        if (BoxShape == null) return;

        if (Texture.Loaded == false) return;

        if (Batched && false)
        {
            float x = CurrentSpriteIndex % _spritesCount.X;
            float y = (float)Math.Floor(CurrentSpriteIndex / _spritesCount.X);

            Vector2 drawOffset = new Vector2(x, y) * SpriteSize * _spritesCount;

            //BatchingManager.UpdateAttribsSpriteSheet(texture.id, gameObjectID, transform.position, new Vector2(GetComponent<BoxShape>().size.X * transform.scale.X, GetComponent<BoxShape>().size.Y * transform.scale.Y),
            //                                         color, drawOffset);
        }
        else
        {
            Tofu.ShaderManager.UseShader(Material.Shader);
            Material.Shader.SetVector2("u_resolution", Texture.Size);
            Material.Shader.SetMatrix4X4("u_mvp", LatestModelViewProjection);
            Material.Shader.SetColor("u_color", Color.ToVector4());
            Material.Shader.SetVector2("u_scale", BoxShape.Size);


            float columnIndex = CurrentSpriteIndex % _spritesCount.X;
            float rowIndex = (float)Math.Floor(CurrentSpriteIndex / _spritesCount.X);

            Vector2 drawOffset = new(columnIndex * SpriteSize.X + SpriteSize.X / 2,
                -rowIndex * SpriteSize.Y - SpriteSize.Y / 2);

            Material.Shader.SetVector2("offset", drawOffset);

            //_zoomAmount = texture.size.X/spriteSize.X*2;
            Material.Shader.SetVector2("zoomAmount", _spritesCount);

            Tofu.ShaderManager.BindVertexArray(Material.Vao);

            if (Material.Additive)
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusConstantColor);
            else
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            TextureHelper.BindTexture(Texture.TextureId);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            //BufferCache.BindVAO(0);
            //GL.Disable(EnableCap.Blend);

            DebugHelper.LogDrawCall();
        }
    }
}
/*public override void OnTextureLoaded(Texture2D _texture, string _path)
{
    SpriteSize = new Vector2(_texture.Width / SpritesCount.X, _texture.Height / SpritesCount.Y);

    base.OnTextureLoaded(_texture, _path);
}#1#*/