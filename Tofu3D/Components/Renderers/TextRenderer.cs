public class TextRendererInstanced : ModelRendererInstanced
{
    public override void Render()
    {
        if (GameObject.IsStatic && InstancingData.InstancingDataDirty == false &&
            InstancingData.MatrixDirty == false)
        {
            return;
        }

        if (RuntimeMesh == null)
        {
            return;
        }
        
        var updatedData =
            Tofu.InstancedRenderingSystem.UpdateObjectData(this, ref InstancingData, VertexBufferStructureType.Model);
        if (updatedData)
        {
            InstancingData.InstancingDataDirty = false;
        }
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