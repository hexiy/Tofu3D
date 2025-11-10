using System.Linq;
using TofuEngine.Rendering.Instancing;

public class TextRenderer : ModelRenderer
{
    private static readonly Dictionary<char, int> _fontMappings = new Dictionary<char, int>
    {
        { ' ', 35 },
        { '!', 36 },
        { '"', 37 },
        { '#', 38 },
        { '$', 39 },
        { '%', 40 },
        { '&', 41 },
        { '*', 42 },
        { '+', 43 },
        { ',', 44 },
        { '-', 45 },
        { '.', 46 },
        { '/', 47 },


        { '0', 48 },
        { '1', 49 },
        { '2', 50 },
        { '3', 51 },
        { '4', 52 },
        { '5', 53 },
        { '6', 54 },
        { '7', 55 },
        { '8', 56 },
        { '9', 57 },

        { ':', 58 },
        { ';', 59 },
        { '<', 60 },
        { '=', 61 },
        { '>', 62 },
        { '?', 63 },
        { '@', 64 },


        { 'A', 65 },
        { 'B', 66 },
        { 'C', 67 },
        { 'D', 68 },
        { 'E', 69 },
        { 'F', 70 },
        { 'G', 71 },
        { 'H', 72 },
        { 'I', 73 },
        { 'J', 74 },
        { 'K', 75 },
        { 'L', 76 },
        { 'M', 77 },
        { 'N', 78 },
        { 'O', 79 },
        { 'P', 80 },
        { 'Q', 81 },
        { 'R', 82 },
        { 'S', 83 },
        { 'T', 84 },
        { 'U', 85 },
        { 'V', 86 },
        { 'W', 87 },
        { 'X', 88 },
        { 'Y', 89 },
        { 'Z', 90 },

        { '[', 91 },
        { '\\', 92 },
        { ']', 93 },
        { '^', 94 },
        { '_', 95 },
        { '`', 96 },


        { 'a', 97 },
        { 'b', 98 },
        { 'c', 99 },
        { 'd', 100 },
        { 'e', 101 },
        { 'f', 102 },
        { 'g', 103 },
        { 'h', 104 },
        { 'i', 105 },
        { 'j', 106 },
        { 'k', 107 },
        { 'l', 108 },
        { 'm', 109 },
        { 'n', 110 },
        { 'o', 111 },
        { 'p', 112 },
        { 'q', 113 },
        { 'r', 114 },
        { 's', 115 },
        { 't', 116 },
        { 'u', 117 },
        { 'v', 118 },
        { 'w', 119 },
        { 'x', 120 },
        { 'y', 121 },
        { 'z', 122 },

        { '{', 123 },
        { '|', 124 },
        { '}', 125 },
        { '~', 126 },
    };

    private Text _text;
    private Vector2 _spritesCountInSpritesheet = new Vector2(16, 8);
    private Vector2 _characterSize = new Vector2(1, 2); // 1,2 because font w:h ratio is 1:2

    [XmlIgnore]
    public List<ObjectInstancingData> RendererInstancingDatas = new List<ObjectInstancingData>();

    private int _oldLength = -1;

    public override void OnDisabled()
    {
        for (int i = 0;
             i < RendererInstancingDatas.Count;
             i++)
        {
            ObjectInstancingData objectInstancingData = RendererInstancingDatas[i];
            Tofu.InstancedRenderingSystem.UpdateObjectData(this, ref objectInstancingData, remove: true,
                isStatic: GameObject.IsStaticSelf);
        }

        base.OnDisabled();
    }

    public override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        if (BoxShape != null)
        {
            BoxShape.Pivot = new Vector3(0, 0.5f, 1f);
        }

        base.Start();
    }

    public override void Update()
    {
        if (Material != null)
        {
            Material.Offset = Vector3.Zero;
        }

        base.Update();
    }

    public override void SetupMeshAndMaterial()
    {
        base.SetupMeshAndMaterial();

        Asset_Model model =
            Tofu.AssetLoadManager.Get<Asset_Model>(TofuPath.Combine(Folders.BasicModelsInAssets, "plane.obj"));
        RuntimeMesh = Tofu.AssetLoadManager.Get<RuntimeMesh>(model.PathsToMeshAssets.First());

        Material = Tofu.AssetLoadManager.CreateCopyFile(Material);
        Material.AlbedoTexture =
            Tofu.AssetLoadManager.Get<RuntimeTexture>(TofuPath.Combine(Folders.TexturesInAssets, "font.png"));

        Material.RenderMode = RenderMode.Transparent;
        Material.BlendMode = BlendMode.Cutout;
        Material.MaterialType = MaterialType.Unlit;
        Material.UVOffsetIsInstanced = true;
        Material.LoadShader();
    }

    public override void UploadRenderData()
    {
        UpdateModelMatrix();

        if (GameObject.ActiveInHierarchy == false)
        {
            return;
        }

        if (GameObject.IsStaticSelf && ObjectInstancingData.InstancingDataDirty == false &&
            ObjectInstancingData.MatrixDirty == false)
        {
            return;
        }

        if (RuntimeMesh == null)
        {
            return;
        }

        if (GetComponent<Text>(out _text) == false)
        {
            return;
        }

        if (_text.Value.Length != _oldLength)
        {
            for (int i = 0; i < RendererInstancingDatas.Count; i++)
            {
                ObjectInstancingData x = RendererInstancingDatas[i];
                x.InstancingDataDirty = true;
                RendererInstancingDatas[i] = x;
            }
        }

        // we dont need data instances for line break characters...
        int instancingDatasToRemove = RendererInstancingDatas.Count - _text.Value.Length;
        if (instancingDatasToRemove > 0)
        {
            for (int i = RendererInstancingDatas.Count - instancingDatasToRemove;
                 i < RendererInstancingDatas.Count;
                 i++)
            {
                ObjectInstancingData objectInstancingData = RendererInstancingDatas[i];

                Tofu.InstancedRenderingSystem.UpdateObjectData(this, ref objectInstancingData, remove: true,
                    isStatic: GameObject.IsStaticSelf);
            }

            RendererInstancingDatas.RemoveRange(RendererInstancingDatas.Count - instancingDatasToRemove,
                instancingDatasToRemove);
        }

        const int maxCharactersPerLine = 10;
        int charactersInCurrentLine = 0;
        float currentX = 0;
        float currentY = 0;
        float maxX = 0;
        float maxY = 0;
        float xOffset = 0;

        Vector3 scaleBefore = Transform.LocalScale;
        Transform.LocalScale = new Vector3(_text.Size / 2f, _text.Size, _text.Size);


        if (_text.Value.Length > 0)
        {
            maxY = -_text.Size; // base size
        }

        for (int i = 0; i < _text.Value.Length; i++)
        {
            while (RendererInstancingDatas.Count <= i)
            {
                RendererInstancingDatas.Add(new ObjectInstancingData());
            }

            // var instancingData = RendererInstancingDatas[i];
            char ch = _text.Value[i];
            bool isNewlineCharacter = ch.ToString() == Environment.NewLine;

            if (isNewlineCharacter == false)
            {
                int glyphMappingIndex = 0;

                if (_fontMappings.TryGetValue(ch.ToString()[0], out int mapping))
                {
                    glyphMappingIndex = mapping;
                }

                int columnIndex = glyphMappingIndex % (int)_spritesCountInSpritesheet.X;
                int rowIndex = (int)Math.Floor(glyphMappingIndex / _spritesCountInSpritesheet.X);
                rowIndex = 8 - rowIndex - 1;

                Material.Tiling = new Vector2(-1f / _spritesCountInSpritesheet.X, 1f / _spritesCountInSpritesheet.Y);

                // Vector2 offset =
                //     new Vector2(1f / _spritesCountInSpritesheet.X,
                //         1f / _spritesCountInSpritesheet.Y) +
                //     new Vector2(1f / _spritesCountInSpritesheet.X * columnIndex,
                //         1f / -_spritesCountInSpritesheet.Y * rowIndex);
                Vector2 unit = Vector2.One / _spritesCountInSpritesheet;

                Vector2 offset = unit.VectorX() + new Vector2(unit.X * columnIndex, unit.Y * rowIndex);
                Matrix4x4 offsetTranslation =
                    Matrix4x4.CreateTranslation((currentX + xOffset) * (1f / _text.Size) * 2f, 0,
                        currentY * (1f / _text.Size));
                BoxShape.Pivot = new Vector3(0, 0.5f, 1f);
                Matrix4x4 modelMatrix = offsetTranslation * GetModelMatrixWithoutBoxShape();
                BoxShape.Pivot = new Vector3(0, 0.5f, 0f);

                ObjectInstancingData objectInstancingData = RendererInstancingDatas[i];
                if (GameObject.IsStaticSelf == false || objectInstancingData.InstancingDataDirty ||
                    objectInstancingData.MatrixDirty)
                {
                    bool updatedData =
                        Tofu.InstancedRenderingSystem.UpdateObjectData(this, ref objectInstancingData,
                            modelMatrix: modelMatrix, uvOffset: offset,
                            indexForMultipleObjectsPerRenderer: i, isStatic: GameObject.IsStaticSelf);

                    if (updatedData)
                    {
                        objectInstancingData.InstancingDataDirty = false;
                    }
                }

                RendererInstancingDatas[i] = objectInstancingData;
            }

            // currentX += _text.Size * _characterSize.X;

            charactersInCurrentLine++;
            bool hitMaxCharactersPerLine = charactersInCurrentLine >= maxCharactersPerLine;

            bool nextCharacterWillBeNewLine = hitMaxCharactersPerLine || isNewlineCharacter;
            maxX = Mathf.Max(maxX, currentX);
            maxY = Mathf.Min(maxY, currentY);
            if (nextCharacterWillBeNewLine)
            {
                if (isNewlineCharacter == false)
                {
                    maxX = Mathf.Max(maxX, currentX + _text.Size * _characterSize.X);
                }

                currentX = 0;
                charactersInCurrentLine = 0;
                currentY -= _text.Size * _characterSize.Y;
            }
            else
            {
                currentX += _text.Size * _characterSize.X;
            }

            maxX = Mathf.Max(maxX, currentX);
            maxY = Mathf.Min(maxY, currentY);
        }


        currentY -= _text.Size * _characterSize.Y;
        maxY = Mathf.Min(maxY, currentY);

        // BoxShape.Size = new Vector3(maxX / _characterSpacing.X, 0.1f, 1 + maxY / _characterSpacing.Y);
        BoxShape.Size = new Vector3(maxX / 2f, 0.1f, maxY / 2f);
        Transform.LocalScale = scaleBefore;
    }
}

/*Tofu.ShaderManager.UseShader(Material.Shader);
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

            Material.Shader.SetVector2("offset", drawOffset);*/