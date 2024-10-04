public class ModelRenderer : TextureRenderer
{
    // public Mesh Mesh;
    public bool CastShadow = true;

    // Color _mousePickingColor;
    [SliderF(0, 1)] public float SmoothShading = 0;

    public bool SpecularHighlightsEnabled = true;

    [SliderF(0, 1)] public float SpecularSmoothness = 0.01f;

    public override void Awake()
    {
        // _mousePickingColor = MousePickingSystem.RegisterObject(this);

        if (Texture == null)
        {
            Texture = new Texture();
        }
        else
        {
            LoadTexture(Texture.Path);
        }

        base.Awake();
    }

    public override void SetDefaultMaterial()
    {
        if (Material?.Path.Length == 0 || Material == null)
        {
            Material = Tofu.AssetManager.Load<Material>("ModelRenderer");
        }
        else
        {
            Material = Tofu.AssetManager.Load<Material>(Material.Path);
        }

        if (Mesh?.Path.Length > 0)
        {
            Mesh = Tofu.AssetManager.Load<Mesh>(Mesh.Path);
        }
        else
        {
            Mesh = null;
        }
        // Material = Tofu.AssetManager.Load<Material>("ModelRenderer");

        // base.CreateMaterial();
    }

    public override void OnDestroyed()
    {
        //BatchingManager.RemoveAttribs(texture.id, gameObjectID);
        base.OnDestroyed();
    }

    public void Update()
    {
        // if (RenderMode == RenderMode.Opaque && Material.RenderMode != RenderMode)
        // {
        // 	Material = Tofu.AssetManager.Load<Material>("ModelRenderer");
        // }
        //
        // if (RenderMode == RenderMode.Transparent && Material.RenderMode != RenderMode)
        // {
        // 	Material = Tofu.AssetManager.Load<Material>("ModelRendererUnlit");
        // }
    }

    public override void Render()
    {
        var isTransformHandle = GameObject == TransformHandle.I.GameObject;
        if (isTransformHandle && Tofu.RenderPassSystem.CurrentRenderPassType != RenderPassType.Opaques &&
            Tofu.RenderPassSystem.CurrentRenderPassType != RenderPassType.UI)
        {
            return;
        }

        if (CastShadow == false &&
            Tofu.RenderPassSystem.CurrentRenderPassType == RenderPassType.DirectionalLightShadowDepth)
        {
            return;
        }

        if ((Transform.IsInCanvas && Tofu.RenderPassSystem.CurrentRenderPassType != RenderPassType.UI) ||
            (Transform.IsInCanvas == false && Tofu.RenderPassSystem.CurrentRenderPassType == RenderPassType.UI))
        {
            return;
        }

        if (Mesh == null)
        {
            return;
        }


        // if (GameObject == TransformHandle.I?.GameObject)
        // {
        // 	GL.Disable(EnableCap.DepthTest);
        // }
        // else
        // {
        // 	GL.Enable(EnableCap.DepthTest);
        // }

        var drawOutline = GameObject.Selected;

        //GL.Enable(EnableCap.DepthTest);
        if (Tofu.RenderPassSystem.CurrentRenderPassType is RenderPassType.DirectionalLightShadowDepth
            or RenderPassType.ZPrePass)
        {
            var depthMaterial = Tofu.AssetManager.Load<Material>("DepthModel");
            Tofu.ShaderManager.UseShader(depthMaterial.Shader);
            Material.Shader.SetMatrix4X4("u_mvp", LatestModelViewProjection);


            Tofu.ShaderManager.BindVertexArray(Mesh.Vao);
            GL_DrawArrays(PrimitiveType.Triangles, 0, Mesh.VerticesCount);
        }

        if (Tofu.RenderPassSystem.CurrentRenderPassType is RenderPassType.Opaques or RenderPassType.UI)
        {
            // GL.Enable(EnableCap.DepthTest);

            if (drawOutline)
            {
                GL.Enable(EnableCap.StencilTest);

                GL.Clear(ClearBufferMask.StencilBufferBit);
                GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
                GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
                GL.StencilMask(0xFF);
            }
            else
            {
                GL.Clear(ClearBufferMask.StencilBufferBit);

                GL.Disable(EnableCap.StencilTest);
            }

            Tofu.ShaderManager.UseShader(Material.Shader);
            Material.Shader.SetMatrix4X4("u_lightSpaceMatrix", DirectionalLight.LightSpaceViewProjectionMatrix);

            if (Transform.IsInCanvas)
            {
                Material.Shader.SetMatrix4X4("u_mvp", GetModelMatrixForCanvasObject());
            }
            else
            {
                Material.Shader.SetMatrix4X4("u_mvp", LatestModelViewProjection);
            }

            Material.Shader.SetFloat("u_renderMode",
                (int)Tofu.RenderSettings.CurrentRenderModeSettings.CurrentRenderMode);
            Material.Shader.SetFloat("u_time", Time.EditorElapsedTime);

            var fogEnabled = Tofu.SceneManager.CurrentScene.SceneFogManager.FogEnabled;
            Material.Shader.SetFloat("u_fogEnabled", fogEnabled ? 1 : 0);
            if (fogEnabled)
            {
                Material.Shader.SetColor("u_fogColor", Tofu.SceneManager.CurrentScene.SceneFogManager.FogColor1);
                Material.Shader.SetFloat("u_fogIntensity", Tofu.SceneManager.CurrentScene.SceneFogManager.Intensity);
                if (Tofu.SceneManager.CurrentScene.SceneFogManager.IsGradient)
                {
                    Material.Shader.SetColor("u_fogColor2", Tofu.SceneManager.CurrentScene.SceneFogManager.FogColor2);
                    Material.Shader.SetFloat("u_fogGradientSmoothness",
                        Tofu.SceneManager.CurrentScene.SceneFogManager.GradientSmoothness);
                }
                else
                {
                    Material.Shader.SetColor("u_fogColor2", Tofu.SceneManager.CurrentScene.SceneFogManager.FogColor1);
                }

                Material.Shader.SetFloat("u_fogStartDistance",
                    Tofu.SceneManager.CurrentScene.SceneFogManager.FogStartDistance);
                Material.Shader.SetFloat("u_fogEndDistance",
                    Tofu.SceneManager.CurrentScene.SceneFogManager.FogEndDistance);
                Material.Shader.SetFloat("u_fogPositionY", Tofu.SceneManager.CurrentScene.SceneFogManager.FogPositionY);
            }

            Material.Shader.SetMatrix4X4("u_model", GetModelMatrix());
            Material.Shader.SetColor("u_rendererColor", Color);
            Material.Shader.SetVector2("u_tiling", Tiling);
            Material.Shader.SetVector2("u_offset", Offset);

            Material.Shader.SetFloat("u_smoothShading", SmoothShading);
            Material.Shader.SetFloat("u_specularSmoothness", 0);
            Material.Shader.SetFloat("u_specularHighlightsEnabled", SpecularHighlightsEnabled ? 1 : 0);

            // Material.Shader.SetVector3("u_lightPos", SceneLightingManager.I.GetDirectionalLightPosition()); // moves with camera but rotated wrong
            Material.Shader.SetVector3("u_camPos", Camera.MainCamera.Transform.WorldPosition);
            // Material.Shader.SetVector3("u_lightPos", SceneLightingManager.I.GetDirectionalLightPosition());
            // Material.Shader.SetVector3("u_camPos", Camera.MainCamera.Transform.WorldPosition);

            Material.Shader.SetVector3("u_ambientLightsColor",
                SceneLightingManager.I.GetAmbientLightsColor().ToVector3());
            Material.Shader.SetFloat("u_ambientLightsIntensity", SceneLightingManager.I.GetAmbientLightsIntensity());

            Material.Shader.SetVector3("u_directionalLightColor",
                SceneLightingManager.I.GetDirectionalLightColor().ToVector3());
            Material.Shader.SetFloat("u_directionalLightIntensity",
                SceneLightingManager.I.GetDirectionalLightIntensity());

            // Vector3 adjustedLightDirection = Transform.RotateVectorByRotation(SceneLightingManager.I.GetDirectionalLightDirection(), -Transform.Rotation);
            // Vector3 adjustedLightDirection = SceneLightingManager.I.GetDirectionalLightDirection();
            var adjustedLightDirection = SceneLightingManager.I.GetDirectionalLightDirection();

            // we can compute light direction 2 in relation to our rotation so we dont have to rotate normals in shader 
            Material.Shader.SetVector3("u_directionalLightDirection", adjustedLightDirection);


            GL.ActiveTexture(TextureUnit.Texture0);
            TextureHelper.BindTexture(Texture.TextureId);
            if (RenderPassDirectionalLightShadowDepth.I?.DebugGrayscaleTexture != null)
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                TextureHelper.BindTexture(RenderPassDirectionalLightShadowDepth.I.DebugGrayscaleTexture
                    .ColorAttachment);
            }


            // if (Model != null)
            // {
            Tofu.ShaderManager.BindVertexArray(Mesh.Vao);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            if (isTransformHandle == false)
            {
                RenderWireframe(Mesh.VerticesCount);
            }

            // GL_DrawArrays(PrimitiveType.Triangles, 0, Mesh.IndicesCount);
            GL_DrawArrays(PrimitiveType.Triangles, 0, Mesh.VerticesCount);


            // }
            // else
            // {
            // 	ShaderCache.BindVertexArray(Material.Vao);
            // 	GL.Enable(EnableCap.Blend);
            // 	GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            // 	GL_DrawArrays(PrimitiveType.Triangles, 0, 36);
            // }
        }
        else
        {
            if (Mesh != null)
            {
                Tofu.ShaderManager.BindVertexArray(Mesh.Vao);

                GL_DrawArrays(PrimitiveType.Triangles, 0, Mesh.VerticesCount);
            }
            else
            {
                Tofu.ShaderManager.BindVertexArray(Material.Vao);
                GL_DrawArrays(PrimitiveType.Triangles, 0, 36);
            }
        }


        if (drawOutline &&
            Tofu.RenderPassSystem.CurrentRenderPassType is not RenderPassType.DirectionalLightShadowDepth)
        {
            GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
            GL.StencilMask(0x00);
            // GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);

            var outlineMaterial = Tofu.AssetManager.Load<Material>("ModelRendererUnlit");
            Tofu.ShaderManager.UseShader(outlineMaterial.Shader);

            if (Transform.IsInCanvas)
            {
                outlineMaterial.Shader.SetMatrix4X4("u_mvp", GetCanvasMvpForOutline());
            }
            else
            {
                outlineMaterial.Shader.SetMatrix4X4("u_mvp", GetMvpForOutline());
            }

            outlineMaterial.Shader.SetMatrix4X4("u_model", GetModelMatrix());
            outlineMaterial.Shader.SetColor("u_rendererColor", new Vector4(1, 1, 1, 1f));

            Tofu.ShaderManager.BindVertexArray(Mesh.Vao);
            GL.ActiveTexture(TextureUnit.Texture0);
            TextureHelper.BindTexture(Tofu.AssetManager.Load<Texture>("Assets/2D/solidColor.png").TextureId);
            GL_DrawArrays(PrimitiveType.Triangles, 0, Mesh.VerticesCount);


            GL.StencilMask(0xFF);
            GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
            GL.Enable(EnableCap.DepthTest);

            // GL.BindVertexArray(0);
            // GL.Disable(EnableCap.Blend);
        }

        /*else if (RenderPassSystem.CurrentRenderPassType == RenderPassType.MousePicking)
        {
            if (GameObject == TransformHandle.I?.GameObject)
            {
                GL.Disable(EnableCap.DepthTest);
            }
            else
            {
                GL.Enable(EnableCap.DepthTest);
            }

            Material mousePickingMaterial = Tofu.AssetManager.Load<Material>("ModelMousePicking");
            ShaderCache.UseShader(mousePickingMaterial.Shader);

            mousePickingMaterial.Shader.SetMatrix4X4("u_mvp", LatestModelViewProjection);
            mousePickingMaterial.Shader.SetMatrix4X4("u_model", GetModelMatrixForLight());

            mousePickingMaterial.Shader.SetColor("u_rendererColor", _mousePickingColor);

            ShaderCache.BindVertexArray(Material.Vao);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6 * 2 * 3);
        }*/

        if (drawOutline)
        {
            // GL.BindVertexArray(0);
        }


        // DebugHelper.LogDrawCall();
        if (drawOutline)
        {
            // DebugHelper.LogDrawCall();
        }
    }
}