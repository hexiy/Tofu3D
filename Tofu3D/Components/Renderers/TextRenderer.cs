/*using System.IO;

namespace Scripts;

public class TextRenderer : SpriteRenderer
{
    private readonly Dictionary<char, int> _fontMappings = new()
    {
        { ' ', 0 },
        { '0', 16 },
        { '1', 17 },
        { '2', 18 },
        { '3', 19 },
        { '4', 20 },
        { '5', 21 },
        { '6', 22 },
        { '7', 23 },
        { '8', 24 },
        { '9', 25 },

        { 'A', 33 },
        { 'B', 34 },
        { 'C', 35 },
        { 'D', 36 },
        { 'E', 37 },
        { 'F', 38 },
        { 'G', 39 },
        { 'H', 40 },
        { 'I', 41 },
        { 'J', 42 },
        { 'K', 43 },
        { 'L', 44 },
        { 'M', 45 },
        { 'N', 46 },
        { 'O', 47 },
        { 'P', 48 },
        { 'Q', 49 },
        { 'R', 50 },
        { 'S', 51 },
        { 'T', 52 },
        { 'U', 53 },
        { 'V', 54 },
        { 'W', 55 },
        { 'X', 56 },
        { 'Y', 57 },
        { 'Z', 58 }
    };

    private Vector2 _spritesCount = new(16, 8);

    public float CharSpacing = 0;

    [ShowIf(nameof(IsGradient))] public Color GradientColor1 = Color.White;

    [ShowIf(nameof(IsGradient))] public Color GradientColor2 = Color.White;
    // texture will be font signed distance field texture,
    // render will be basically going through all the characters in Text component and rendering each symbol

    public bool IsGradient = false;

    [Hide] public Vector2 SpriteSize;

    // //[LinkableComponent]
    public Text Text;

    public Vector2 SpritesCount
    {
        get => _spritesCount;
        set
        {
            _spritesCount = value;
            if (Texture != null)
            {
                SpriteSize = new Vector2(Texture.Size.X / SpritesCount.X, Texture.Size.Y / SpritesCount.Y);
            }
        }
    }

    public override void Awake()
    {
        SpritesCount = SpritesCount;
        SetDefaultTexture(Path.Combine(Folders.Textures, "font.png"));
        Text = GetComponent<Text>();
        BoxShape = GetComponent<BoxShape>();


        if (Texture == null)
        {
            Texture = new RuntimeTexture();
        }
        else
        {
            // var textureLoadSettings = TextureLoadSettings.DefaultSettingsTexture2D;
            // TextureLoadSettings textureLoadSettings = TextureLoadSettings.DefaultSettingsSpritePixelArt;
            Texture = Tofu.AssetLoadManager.Load<RuntimeTexture>(Texture.PathToRawAsset);
        }

        base.Awake();
    }

    public override void SetDefaultMaterial()
    {
        Material = Tofu.AssetLoadManager.Load<Asset_Material>("Assets/Materials/TextRenderer.mat");

        Material.Additive = false;
    }

    public override void OnNewComponentAdded(Component comp)
    {
        Text = GetComponent<Text>();
        BoxShape = GetComponent<BoxShape>();
    }

    public override void LoadTexture(string texturePath)
    {
        if (texturePath.Contains("Assets") == false)
        {
            texturePath = Path.Combine("Assets", texturePath);
        }

        if (File.Exists(texturePath) == false)
        {
            return;
        }


        Texture = Tofu.AssetLoadManager.Load<RuntimeTexture>(texturePath);
    }

    public override void Render()
    {
        if (BoxShape == null || Texture.Loaded == false || Text == null)
        {
            return;
        }

        //Debug.Log("Draw text:" + text?.text);

        Tofu.ShaderManager.UseShader(Material.Shader);
        Material.Shader.SetVector2("u_resolution", Texture.Size);

        if (Transform.IsInCanvas)
        {
            Material.Shader.SetMatrix4X4("u_mvp",
                GetModelMatrixForCanvasObject()); // * Camera.I.ViewMatrix * Camera.I.ProjectionMatrix);
        }
        else
        {
            Material.Shader.SetMatrix4X4("u_mvp", LatestModelViewProjection);
        }

        Material.Shader.SetColor("u_color", Color.ToVector4());
        Material.Shader.SetVector2("u_scale", BoxShape.Size);
        Material.Shader.SetVector2("zoomAmount", _spritesCount * 2);
        Material.Shader.SetFloat("isGradient", IsGradient ? 1 : 0);
        if (IsGradient)
        {
            Material.Shader.SetVector4("u_color_a", GradientColor1.ToVector4());
            Material.Shader.SetVector4("u_color_b", GradientColor2.ToVector4());
        }

        BoxShape.Size = Vector3.One * 1.2f; // bigger individual characters


        var charSpacing = CharSpacing;
        var originalPosition = Transform.WorldPosition;
        var originalPivot = Transform.Pivot;

        var symbolInLineIndex = 0;
        var line = 0;
        float lineSpacing = Text.Size / 15;


        Vector2 originalScale = Transform.LocalScale;
        Vector2 fontSizeScale = Vector3.One * Mathf.Clamp(Text.Size / 40f, 0, 1000);
        Transform.LocalScale = originalScale * fontSizeScale;

        var textWidth = charSpacing * (Text.Value.Length - 1);

        for (var symbolIndex = 0;
             symbolIndex < Text.Value.Length;
             symbolIndex++,
             symbolInLineIndex++)
        {
            // Transform.WorldPosition = new Vector3(originalPosition.X + charSpacing * symbolInLineIndex - charSpacing * Transform.Pivot.X,
            //                                       originalPosition.Y + line * lineSpacing, Transform.WorldPosition.Z);
            Transform.Pivot = new Vector3(originalPivot.X + symbolInLineIndex * -0.5f * charSpacing,
                originalPivot.Y + line * 0.5f * lineSpacing,
                0);

            // if (GetComponent<TextReactToMouse>() != null)
            // {
            // 	Transform.WorldPosition = Transform.WorldPosition + new Vector2(0, (float) MathHelper.Sin(Time.EditorElapsedTime + symbolIndex * 0.1f) * 1);
            //
            // 	float distanceToCursor = Vector2.Distance(Transform.WorldPosition, Tofu.MouseInput.WorldPosition);
            // 	Transform.WorldScale = originalScale * fontSizeScale * Mathf.Clamp((0.2f / distanceToCursor + 1f), 1, 1.3f);
            // 	Debug.StatSetValue("MouseWOrldPos:", $"MouseWorldPos:{Tofu.MouseInput.WorldPosition}");
            // }

            UpdateMvp();
            // Material.Shader.SetMatrix4X4("u_mvp", LatestModelViewProjection);
            if (Transform.IsInCanvas)
                // Material.Shader.SetMatrix4X4("u_mvp", GetModelMatrix() * Matrix4x4.CreateScale(1f/Units.OneWorldUnit));
            {
                Material.Shader.SetMatrix4X4("u_mvp",
                    GetModelMatrixForCanvasObject()); // * Camera.I.ViewMatrix * Camera.I.ProjectionMatrix);
            }
            else
            {
                Material.Shader.SetMatrix4X4("u_mvp", LatestModelViewProjection);
            }

            var ch = Text.Value[symbolIndex].ToString().ToUpper()[0];
            if (ch == '\n')
            {
                symbolInLineIndex = -1;
                line++;
                //symbolIndex++;
                continue;
            }

            var glyphMappingIndex = 0;

            if (_fontMappings.ContainsKey(ch))
            {
                glyphMappingIndex = _fontMappings[ch];
            }

            var columnIndex = glyphMappingIndex % (int)_spritesCount.X;
            var rowIndex = (int)Math.Floor(glyphMappingIndex / _spritesCount.Y);

            Vector2 drawOffset = new(columnIndex * SpriteSize.X + SpriteSize.X / 2,
                -rowIndex * SpriteSize.Y - SpriteSize.Y / 2);

            Material.Shader.SetVector2("offset", drawOffset);

            Tofu.ShaderManager.BindVertexArray(Material.Vao);

            if (Material.Additive)
            {
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusConstantColor);
            }
            else
            {
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            }

            // GL.Disable(EnableCap.Blend);
            // GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusConstantColor);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);

            GL.ActiveTexture(TextureUnit.Texture0);
            TextureHelper.BindTexture(Texture.TextureId);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            DebugHelper.LogDrawCall();
        }

        Transform.Pivot = originalPivot;
        Transform.WorldPosition = originalPosition;
        Transform.LocalScale = originalScale;
    }
}*/