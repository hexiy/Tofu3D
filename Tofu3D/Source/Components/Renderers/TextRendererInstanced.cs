using System.Linq;
using TofuEngine.Rendering.Instancing;

public class TextRendererInstanced : ModelRendererInstanced
{
    private readonly Dictionary<char, int> _fontMappings = new Dictionary<char, int>
    {
        { ' ', 0 },
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
        { 'Z', 90 }
    };

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
                isStatic: this.GameObject.IsStatic);
        }

        base.OnDisabled();
    }

    public override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        BoxShape.Pivot = new Vector3(0, 0.5f, 1f);
        base.Start();
    }

    public override void SetDefaultMaterial()
    {
        base.SetDefaultMaterial();

        Asset_Model model =
            Tofu.AssetLoadManager.Get<Asset_Model>(TofuPath.Combine(Folders.BasicModelsInAssets, "plane.obj"));
        RuntimeMesh = Tofu.AssetLoadManager.Get<RuntimeMesh>(model.PathsToMeshAssets.First());

        Material = Tofu.AssetLoadManager.CreateCopyFile(Material);
        Material.AlbedoTexture =
            Tofu.AssetLoadManager.Get<RuntimeTexture>(TofuPath.Combine(Folders.TexturesInAssets, "font.png"));

        Material.RenderMode = RenderMode.Transparent;
        Material.UVOffsetIsInstanced = true;
        Material.LoadShader();
    }

    public override void UploadRenderData()
    {
        UpdateModelMatrix();

        if (this.GameObject.ActiveInHierarchy == false)
        {
            return;
        }

        if (GameObject.IsStatic && ObjectInstancingData.InstancingDataDirty == false &&
            ObjectInstancingData.MatrixDirty == false)
        {
            return;
        }

        if (RuntimeMesh == null)
        {
            return;
        }

        if (GetComponent<Text>(out Text textComponent) == false)
        {
            return;
        }

        if (textComponent.Value.Length != _oldLength)
        {
            for (int i = 0; i < RendererInstancingDatas.Count; i++)
            {
                ObjectInstancingData x = RendererInstancingDatas[i];
                x.InstancingDataDirty = true;
                RendererInstancingDatas[i] = x;
            }
        }

        // we dont need data instances for line break characters...
        int instancingDatasToRemove = RendererInstancingDatas.Count - textComponent.Value.Length;
        if (instancingDatasToRemove > 0)
        {
            for (int i = RendererInstancingDatas.Count - instancingDatasToRemove;
                 i < RendererInstancingDatas.Count;
                 i++)
            {
                ObjectInstancingData objectInstancingData = RendererInstancingDatas[i];

                Tofu.InstancedRenderingSystem.UpdateObjectData(this, ref objectInstancingData, remove: true,
                    isStatic: this.GameObject.IsStatic);
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
        Transform.LocalScale = new Vector3(textComponent.Size / 2f, textComponent.Size, textComponent.Size);


        if (textComponent.Value.Length > 0)
        {
            maxY = -textComponent.Size; // base size
        }

        for (int i = 0; i < textComponent.Value.Length; i++)
        {
            while (RendererInstancingDatas.Count <= i)
            {
                RendererInstancingDatas.Add(new ObjectInstancingData());
            }

            // var instancingData = RendererInstancingDatas[i];
            char ch = textComponent.Value[i];
            bool isNewlineCharacter = ch.ToString() == Environment.NewLine;

            if (isNewlineCharacter == false)
            {
                int glyphMappingIndex = 0;

                if (_fontMappings.TryGetValue(ch.ToString().ToUpper()[0], out int mapping))
                {
                    glyphMappingIndex = mapping;
                }

                int columnIndex = glyphMappingIndex % (int)_spritesCountInSpritesheet.X;
                int rowIndex = (int)Math.Floor(glyphMappingIndex / _spritesCountInSpritesheet.X);


                Material.Tiling = new Vector2(1f / _spritesCountInSpritesheet.X, 1f / _spritesCountInSpritesheet.Y);

                Vector2 offset =
                    new Vector2(1f / _spritesCountInSpritesheet.X,
                        1f / _spritesCountInSpritesheet.Y) +
                    new Vector2(1f / _spritesCountInSpritesheet.X * columnIndex,
                        1f - 1f / -_spritesCountInSpritesheet.Y * rowIndex);


                Matrix4x4 offsetTranslation =
                    Matrix4x4.CreateTranslation(currentX + xOffset, 0, currentY);
                BoxShape.Pivot = new Vector3(0, 0.5f, 1f);
                Matrix4x4 modelMatrix = GetModelMatrixWithoutBoxShape() * offsetTranslation;
                BoxShape.Pivot = new Vector3(0, 0.5f, 0f);

                ObjectInstancingData objectInstancingData = RendererInstancingDatas[i];
                if (GameObject.IsStatic == false || objectInstancingData.InstancingDataDirty ||
                    objectInstancingData.MatrixDirty)
                {
                    bool updatedData =
                        Tofu.InstancedRenderingSystem.UpdateObjectData(this, ref objectInstancingData,
                            modelMatrix: modelMatrix, uvOffset: offset,
                            indexForMultipleObjectsPerRenderer: i, isStatic: GameObject.IsStatic);

                    if (updatedData)
                    {
                        objectInstancingData.InstancingDataDirty = false;
                    }
                }

                RendererInstancingDatas[i] = objectInstancingData;
            }

            // currentX += textComponent.Size * _characterSize.X;

            charactersInCurrentLine++;
            bool hitMaxCharactersPerLine = charactersInCurrentLine >= maxCharactersPerLine;

            bool nextCharacterWillBeNewLine = hitMaxCharactersPerLine || isNewlineCharacter;
            maxX = Mathf.Max(maxX, currentX);
            maxY = Mathf.Min(maxY, currentY);
            if (nextCharacterWillBeNewLine)
            {
                if (isNewlineCharacter == false)
                {
                    maxX = Mathf.Max(maxX, currentX + textComponent.Size * _characterSize.X);
                }

                currentX = 0;
                charactersInCurrentLine = 0;
                currentY -= textComponent.Size * _characterSize.Y;
            }
            else
            {
                currentX += textComponent.Size * _characterSize.X;
            }

            maxX = Mathf.Max(maxX, currentX);
            maxY = Mathf.Min(maxY, currentY);
        }


        currentY -= textComponent.Size * _characterSize.Y;
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