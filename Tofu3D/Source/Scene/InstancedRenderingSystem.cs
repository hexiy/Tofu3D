namespace TofuEngine.Rendering.Instancing;

public class InstancedRenderingSystem
{
    private List<InstancedGroupDefinition> _groupDefinitions = new List<InstancedGroupDefinition>();

    // key is shaderID
    private Dictionary<int, ShaderGroup> _shaderGroups = new Dictionary<int, ShaderGroup>();

    // index in _definitions
    private Dictionary<int, SharedInstancingBuffer> _sharedInstancedBuffers =
        new Dictionary<int, SharedInstancingBuffer>();

    private Asset_Material _mousePickingMaterial;
    private Asset_Material _depthMaterial;
    // private Asset_Material _customDepthMaterial;

    public InstancedRenderingSystem()
    {
        {
            _mousePickingMaterial = new Asset_Material()
            {
                Shader = Tofu.ShaderManager.LoadShader(TofuPath.Combine(Folders.ShadersInAssets,
                    "ModelMousePicking.glsl"))
            };
            _mousePickingMaterial.LoadShader();
        }

        {
            _depthMaterial = new Asset_Material()
            {
                Shader = Tofu.ShaderManager.LoadShader(TofuPath.Combine(Folders.ShadersInAssets,
                    "ModelRendererInstancedDepth.glsl"))
            };

            _depthMaterial.LoadShader();
        }

        // {
        //     _customDepthMaterial = new Asset_Material()
        //     {
        //         Shader = Tofu.ShaderManager.LoadShader(TofuPath.Combine(Folders.ShadersInAssets,
        //             "ModelRendererInstancedCustomDepth.glsl"))
        //     };
        //
        //     _customDepthMaterial.LoadShader();
        // }
    }

    private int GetOrCreateGroupByShader(Shader shader)
    {
        if (_shaderGroups.ContainsKey(shader.ProgramId) == false)
        {
            _shaderGroups[shader.ProgramId] = new ShaderGroup() { Shader = shader };
            return shader.ProgramId;
        }

        return shader.ProgramId;
    }

    public void ClearBuffers()
    {
        foreach (KeyValuePair<int, SharedInstancingBuffer> pair in _sharedInstancedBuffers)
        {
            // need to care for left objects that use the same vao
            if (pair.Value.Vao == -1)
            {
                continue;
            }

            Tofu.ShaderManager.BindVertexArray(pair.Value.Vao);
            if (pair.Value.Vbo > 0)
            {
                GL.DeleteBuffer(pair.Value.Vbo);
            }

            if (pair.Value.Ebo > 0)
            {
                GL.DeleteBuffer(pair.Value.Ebo);
            }

            if (pair.Value.ShaderId > 0)
            {
                GL.DeleteProgram(pair.Value.ShaderId);
            }

            Tofu.ShaderManager.BindVertexArray(-1);

            GL.DeleteVertexArray(pair.Value.Vao);
        }

        _sharedInstancedBuffers = new Dictionary<int, SharedInstancingBuffer>();
        _groupDefinitions = new List<InstancedGroupDefinition>();
        _shaderGroups = new Dictionary<int, ShaderGroup>();
    }

    public void RenderShaderGroups(InstancingRenderMode renderMode)
    {
        // if mousepicking or depth, we set the shader first for all shadergroups
        if (Tofu.RenderingSystem.CurrentlyExecutingPipeline.CurrentRenderPassType == RenderPassType.MousePicking)
        {
            // _mousePickingMaterial = Tofu.AssetLoadManager.Load<Asset_Material>("ModelMousePicking.mat");
            Tofu.ShaderManager.UseShader(_mousePickingMaterial.Shader);

            _mousePickingMaterial.Shader.SetMatrix4X4("u_viewProjection",
                Camera.CurrentlyRenderingCamera.ViewMatrix * Camera.CurrentlyRenderingCamera.ProjectionMatrix);
        }

        else if (Tofu.RenderingSystem.CurrentlyExecutingPipeline.CurrentRenderPassType
                 is RenderPassType.DirectionalLightShadowDepth
                 or RenderPassType.PointLightShadowDepth
                 or RenderPassType.ZPrePass)
        {
            Tofu.ShaderManager.UseShader(_depthMaterial.Shader);

            // not material-dependent
            _depthMaterial.Shader.SetMatrix4X4("u_viewProjection",
                Camera.CurrentlyRenderingCamera.ViewMatrix * Camera.CurrentlyRenderingCamera.ProjectionMatrix);
        }

        // Iterate over shader groups
        foreach (KeyValuePair<int, ShaderGroup> shaderGroup in _shaderGroups)
        {
            if (shaderGroup.Value.GroupDefinitionIndexes.Count == 0)
            {
                continue;
            }

            if (Tofu.RenderingSystem.CurrentlyExecutingPipeline.CurrentRenderPassType is RenderPassType.Opaques
                or RenderPassType.UI
                or RenderPassType.Transparency)
            {
                Shader shader = _groupDefinitions[shaderGroup.Value.GroupDefinitionIndexes[0]].Material.Shader;
                // shader = Tofu.ShaderManager.LoadShader(shader.Path);
                Tofu.ShaderManager.UseShader(shader);

                SetGlobalUniforms(_groupDefinitions[shaderGroup.Value.GroupDefinitionIndexes[0]].Material);
            }

            foreach (int definitionIndexInThisShaderGroup in shaderGroup.Value.GroupDefinitionIndexes)
            {
                if (_sharedInstancedBuffers.ContainsKey(definitionIndexInThisShaderGroup) == false)
                {
                    continue;
                }

                SharedInstancingBuffer bufferData = _sharedInstancedBuffers[definitionIndexInThisShaderGroup];
                if (bufferData.NumberOfObjects == 0)
                {
                    continue;
                }

                // Continue with filtering logic
                if (renderMode != InstancingRenderMode.All &&
                    bufferData.RenderMode != (RenderMode)renderMode)
                {
                    continue;
                }

                RenderSpecific(definitionIndexInThisShaderGroup, bufferData);
            }
        }
    }

    private void RemoveObject(SharedInstancingBuffer sharedInstancingBuffer,
        ref ObjectInstancingData objectInstancingData, int shaderGroupId)
    {
        int definitionIndex = objectInstancingData.InstancedRenderingDefinitionIndex;


        sharedInstancingBuffer.RemoveObject(objectInstancingData);


        if (sharedInstancingBuffer.NumberOfObjects == 0)
        {
            _sharedInstancedBuffers.Remove(definitionIndex);
            // _groupDefinitions[definitionIndex] = null;

            _groupDefinitions.Remove(sharedInstancingBuffer.InstancedGroupDefinition);
            // _groupDefinitions.RemoveAt(definitionIndex);


            /////////////////// remove SHADER GROUP
            _shaderGroups[shaderGroupId].GroupDefinitionIndexes.Remove(definitionIndex);
            if (_shaderGroups[shaderGroupId].GroupDefinitionIndexes.Count == 0)
            {
                _shaderGroups.Remove(shaderGroupId);
            }
            /////////////////// remove SHADER GROUP
        }


        objectInstancingData.StartingIndexInBuffer = -1;
        objectInstancingData.InstancedRenderingDefinitionIndex = -1;
    }


    private void RenderSpecific(int definitionIndex, SharedInstancingBuffer sharedInstancingBuffer)
    {
        // resize the buffer if needed, after drawing the old one
        // if (sharedBuffer.Buffer.Length !=
        //     sharedBuffer.InstancedVertexDataLayoutDefinition.CountOfFloats * sharedBuffer.FutureMaxNumberOfObjects) this was better because we only want to resize once not for each new object
        // {
        //     sharedBuffer.ExpandBuffer();
        // }

        // sharedBuffer.NeedsUpload = true;


        sharedInstancingBuffer.SetupInstancedBufferAndUploadIfNeeded();

        if (_groupDefinitions.Count <= definitionIndex)
        {
            return;
        }

        InstancedGroupDefinition definition = _groupDefinitions[definitionIndex];
        Asset_Material material = definition.Material;
        int meshVao = definition.RuntimeMesh.Vao;
        int indicesCount = definition.RuntimeMesh.Mesh.IndicesLength;
        int numberOfObjects = sharedInstancingBuffer.NumberOfObjects;
        if (material.IgnoreDepth || material.NoDepth)
        {
            GL.Disable(EnableCap.DepthTest);
        }

        // if (material.RenderMode == RenderMode.Transparent)
        // {
        //     GL.Disable(EnableCap.CullFace);
        //     GL.CullFace(CullFaceMode.Back);
        // }
        // else
        // {
        //     GL.Enable(EnableCap.CullFace);
        //     GL.CullFace(CullFaceMode.Back);
        // }
        if (Tofu.RenderingSystem.CurrentlyExecutingPipeline.CurrentRenderPassType == RenderPassType.MousePicking)
        {
            RenderObjects_MousePickingPass(meshVao: meshVao, numberOfObjects: numberOfObjects,
                indicesCount: indicesCount, verticesCount: definition.RuntimeMesh.Mesh.VerticesCount,
                vbo: sharedInstancingBuffer.Vbo);
        }

        else if (Tofu.RenderingSystem.CurrentlyExecutingPipeline.CurrentRenderPassType
                 is RenderPassType.DirectionalLightShadowDepth
                 or RenderPassType.PointLightShadowDepth
                 or RenderPassType.ZPrePass)
        {
            if (material.NoDepth == false)
            {
                RenderObjects_DepthPasses(meshVao: meshVao, numberOfObjects: numberOfObjects,
                    indicesCount: indicesCount, verticesCount: definition.RuntimeMesh.Mesh.VerticesCount,
                    material: material, vbo: sharedInstancingBuffer.Vbo);
            }
        }

        else if (Tofu.RenderingSystem.CurrentlyExecutingPipeline.CurrentRenderPassType is RenderPassType.Opaques
                 or RenderPassType.UI
                 or RenderPassType.Transparency)
        {
            RenderObjects_Opaques_UI_Transparency(meshVao: meshVao, numberOfObjects: numberOfObjects,
                indicesCount: indicesCount, verticesCount: definition.RuntimeMesh.Mesh.VerticesCount,
                material: material, vbo: sharedInstancingBuffer.Vbo);
        }

        if (material.IgnoreDepth || material.NoDepth)
        {
            GL.Enable(EnableCap.DepthTest);
        }

        Tofu.ShaderManager.BindVertexArray(0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        TofuGL.CheckGlError("instanced rendering error");
    }

    private void RenderObjects_MousePickingPass(int meshVao, int numberOfObjects, int indicesCount, int verticesCount,
        int vbo)
    {
        // Tofu.ShaderManager.BindVertexArray(meshVao);
        // GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);


        if (RenderingSettings.USE_INDICES)
        {
            if (indicesCount > 0)
            {
                GL_DrawElementsInstanced(PrimitiveType.Triangles, indicesCount,
                    numberOfObjects);
            }
        }
        else
        {
            GL_DrawArraysInstanced(PrimitiveType.Triangles, 0, verticesCount,
                numberOfObjects);
        }
    }

    private void RenderObjects_DepthPasses(int meshVao, int numberOfObjects, int indicesCount, int verticesCount,
        Asset_Material material, int vbo)
    {
        // if (material.CustomRenderQueue != null)
        // {
        //     Tofu.ShaderManager.UseShader(_customDepthMaterial.Shader);
        //     _customDepthMaterial.Shader.SetFloat("u_customRenderQueue", material.CustomRenderQueue.Value);
        // }
        // if (material.RenderMode == RenderMode.Transparent)
        // {
        // dont render depth for transparent objects
        // return;
        // }


        // Tofu.ShaderManager.BindVertexArray(meshVao);
        // GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);


        if (RenderingSettings.USE_INDICES)
        {
            if (indicesCount > 0)
            {
                GL_DrawElementsInstanced(PrimitiveType.Triangles, indicesCount,
                    numberOfObjects);
            }
        }
        else
        {
            GL_DrawArraysInstanced(PrimitiveType.Triangles, 0, verticesCount,
                numberOfObjects);
        }

        // if (material.CustomRenderQueue != null)
        // {
        // Tofu.ShaderManager.UseShader(_depthMaterial.Shader);
        // }
    }

    private void RenderObjects_Opaques_UI_Transparency(int meshVao, int numberOfObjects, int indicesCount,
        int verticesCount,
        Asset_Material material, int vbo)
    {
        SetMaterialSpecificUniforms(material);

        RenderingBlendingHelper.SetBlendMode(material.BlendMode);

        // Tofu.ShaderManager.BindVertexArray(meshVao);
        // GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

        if (RenderingSettings.USE_INDICES)
        {
            if (indicesCount > 0)
            {
                GL_DrawElementsInstanced(PrimitiveType.Triangles, indicesCount,
                    numberOfObjects);
            }
        }
        else
        {
            GL_DrawArraysInstanced(PrimitiveType.Triangles, 0, verticesCount,
                numberOfObjects);
        }
    }

    private void SetGlobalUniforms(Asset_Material material)
    {
        Shader shader = material.Shader;
        if (shader.AtlasUniformIsSet == false)
        {
            // uplad all the texture atlases only once per shader
            if (shader.AtlasArrayTextureUnit != null)
            {
                GL.ActiveTexture(shader.AtlasArrayTextureUnit.Value); // Activate texture unit 0

                GL.BindTexture(TextureTarget.Texture2DArray, Tofu.TextureAtlasManager.GLTextureArrayId);

                shader.AtlasUniformIsSet = true;
            }
        }

        bool discardTransparentPixels =
            material.BlendMode is BlendMode.Fade or BlendMode.PremultipliedAlpha or BlendMode.Additive;

        shader.SetInt("u_discardTransparentPixels", discardTransparentPixels ? 1 : 0);

        Tofu.LightRenderingManager.BindPointLightsUBO(shader.ProgramId);
        shader.SetInt("_pointLightsCount", Tofu.LightRenderingManager.PointLightsCount);

        shader.SetFloat("u_cameraFrustumLength",
            Camera.CurrentlyRenderingCamera.FarPlaneDistance - Camera.CurrentlyRenderingCamera.NearPlaneDistance);

        shader.SetFloat("u_renderMode",
            (int)Tofu.RenderingSystem.CurrentlyExecutingPipeline.RenderSettings.RenderModeSettings.CurrentRenderMode);

        if (Tofu.RenderingSystem.CurrentlyExecutingPipeline.CurrentRenderPassType is RenderPassType.UI &&
            Tofu.RenderingSystem.CurrentlyExecutingPipeline.ViewType is RenderTargetPipelineType.GameView)
        {
            shader.SetMatrix4X4("u_viewProjection",
                Matrix4x4.Identity * Camera.CurrentlyRenderingCamera.GetOrthographicProjectionMatrix());
        }
        else
        {
            shader.SetMatrix4X4("u_viewProjection",
                Camera.CurrentlyRenderingCamera.ViewMatrix * Camera.CurrentlyRenderingCamera.ProjectionMatrix);
        }

        shader.SetVector3("u_camPosWorldSpace", Camera.CurrentlyRenderingCamera.Transform.WorldPosition);

        // LIGHTING
        shader.SetMatrix4X4("u_lightSpaceViewProjection", DirectionalLight.LightSpaceViewProjectionMatrix);


        Vector4 ambientColor = SceneLightingManager.I.GetAmbientLightsColor().ToVector4();
        ambientColor = new Vector4(ambientColor.X, ambientColor.Y, ambientColor.Z,
            Mathf.ClampMin(SceneLightingManager.I.GetAmbientLightsIntensity(), 0));
        shader.SetVector4("u_ambientLightColor", ambientColor);

        Vector4 directionalLightColor = SceneLightingManager.I.GetDirectionalLightColor().ToVector4();
        directionalLightColor.W = Mathf.ClampMin(SceneLightingManager.I.GetDirectionalLightIntensity(), 0);
        shader.SetVector4("u_directionalLightColor", directionalLightColor);

        Vector3 dir = SceneLightingManager.I.GetDirectionalLightDirection().Normalized();
        shader.SetVector3("u_directionalLightDirection",
            // SceneLightingManager.I.GetDirectionalLightDirection().Normalized());
            dir);
        // new Vector3(0,-1,0));


        //FOG
        bool fogEnabled = Tofu.SceneManager.CurrentScene.SceneFogManager.FogEnabled;
        shader.SetFloat("u_fogEnabled", fogEnabled ? 1 : 0);
        if (fogEnabled)
        {
            shader.SetColor("u_fogColor", Tofu.SceneManager.CurrentScene.SceneFogManager.FogColor1);
            shader.SetFloat("u_fogIntensity", Tofu.SceneManager.CurrentScene.SceneFogManager.Intensity);
            if (Tofu.SceneManager.CurrentScene.SceneFogManager.IsGradient)
            {
                shader.SetColor("u_fogColor2", Tofu.SceneManager.CurrentScene.SceneFogManager.FogColor2);
                shader.SetFloat("u_fogGradientSmoothness",
                    Tofu.SceneManager.CurrentScene.SceneFogManager.GradientSmoothness);
            }
            else
            {
                shader.SetColor("u_fogColor2", Tofu.SceneManager.CurrentScene.SceneFogManager.FogColor1);
            }

            shader.SetFloat("u_fogStartDistance",
                Tofu.SceneManager.CurrentScene.SceneFogManager.FogStartDistance);
            shader.SetFloat("u_fogEndDistance",
                Tofu.SceneManager.CurrentScene.SceneFogManager.FogEndDistance);
            shader.SetFloat("u_fogPositionY",
                Tofu.SceneManager.CurrentScene.SceneFogManager.FogPositionY);
        }
    }

    private void SetMaterialSpecificUniforms(Asset_Material material)
    {
        material.Shader.SetInt("u_materialType", (int)material.MaterialType);


        if (material.ObjectSelected)
        {
            material.Shader.SetVector4("u_emissiveColor", new Vector4(1, 0, 0, 1));
        }
        else
        {
            material.Shader.SetVector4("u_emissiveColor", material.EmissiveColor);
        }

        material.Shader.SetColor("u_albedoTint", material.AlbedoColor);
        // material.Shader.SetVector2("u_tiling", new Vector2(-1, -1)); //grass block
        material.Shader.SetVector2("u_tiling", material.Tiling); // normal 
        material.Shader.SetVector2("u_offset", material.Offset);


        material.Shader.SetInt("u_smoothShadows", material.SmoothShadows ? 1 : 0);


        material.Shader.SetInt("u_refractionEnabled", material.RefractionEnabled ? 1 : 0);
        material.Shader.SetFloat("u_refractiveIndex", material.RefractiveIndex);

        // Albedo Texture

        // if (material.AlbedoTexture != null && material.Shader?.AlbedoTextureIndexUnit != null)
        // {
        //     material.Shader.SetInt("u_hasAlbedoTexture", 1);
        //
        //     GL.ActiveTexture(material.Shader.AlbedoTextureIndexUnit.Value);
        //     TextureHelper.BindTexture(material.AlbedoTexture.StandaloneGLTextureId.Value);
        // }
        // else
        // {
        //     material.Shader.SetInt("u_hasAlbedoTexture", 0);
        // }

        if (material.Shader.IsLoaded == false)
        {
            return;
        }

        // Shadowmap
        {
            bool shadowMapReady = RenderPassDirectionalLightShadowDepth.I?.MainFramebuffer != null &&
                                  RenderPassDirectionalLightShadowDepth.I.Enabled &&
                                  material.Shader.ShadowMapTextureUnit != null;

            material.Shader.SetInt("u_hasShadowmapTexture", shadowMapReady ? 1 : 0);

            if (shadowMapReady)
            {
                GL.ActiveTexture(material.Shader.ShadowMapTextureUnit.Value);
                TextureHelper.BindTexture(RenderPassDirectionalLightShadowDepth.I.MainFramebuffer.DepthTextureId);
            }
        }

        if (false)
        {
            // Albedo Texture
            material.Shader.SetInt("u_hasAlbedoTexture", material.AlbedoTexture != null ? 1 : 0);
            // if (material.AlbedoTexture != null && material.Shader?.AlbedoTextureIndexUnit != null)
            // {
            //     GL.ActiveTexture(material.Shader.AlbedoTextureIndexUnit.Value);
            //     TextureHelper.BindTexture(material.AlbedoTexture.AtlasGLTextureArrayId);
            // }


            // Alpha mask Texture
            material.Shader.SetInt("u_hasAlphaMaskTexture", material.AlphaMaskTexture != null ? 1 : 0);
            // if (material.AlphaMaskTexture != null && material.Shader?.AlphaMaskTextureIndexUnit != null)
            // {
            //     GL.ActiveTexture(material.Shader.AlphaMaskTextureIndexUnit.Value);
            //     TextureHelper.BindTexture(material.AlphaMaskTexture.AtlasGLTextureArrayId);
            // }


            // Normal Texture
            material.Shader.SetInt("u_hasNormalTexture", material.NormalTexture != null ? 1 : 0);
            // if (material.NormalTexture != null && material.Shader?.NormalTextureIndexUnit != null)
            // {
            //     GL.ActiveTexture(material.Shader.NormalTextureIndexUnit.Value);
            //     TextureHelper.BindTexture(material.NormalTexture.AtlasGLTextureArrayId);
            // }

            // Ambient Occlusion Texture
            // material.Shader.SetInt("u_hasAmbientOcclusionTexture", material.AmbientOcclusionTexture != null ? 1 : 0);
            // if (material.AmbientOcclusionTexture != null && material.Shader?.AmbientOcclusionTextureUnit != null)
            // {
            //     GL.ActiveTexture(material.Shader.AmbientOcclusionTextureUnit.Value);
            //     TextureHelper.BindTexture(material.AmbientOcclusionTexture.AtlasGLTextureArrayId);
            // }


            material.Shader.SetInt("u_hasEnvironmentCubemap",
                Camera.CurrentlyRenderingCamera?.GetComponent<Skybox>() != null ? 1 : 0);
            if (Camera.CurrentlyRenderingCamera?.GetComponent<Skybox>() != null &&
                material.Shader.EnvironmentTextureUnit != null)
            {
                GL.ActiveTexture(material.Shader.EnvironmentTextureUnit.Value);
                TextureHelper.BindTexture(
                    Camera.CurrentlyRenderingCamera.GetComponent<Skybox>().GetCubemapTexture().TextureId,
                    TextureType.Cubemap);
            }

            // Roughness Texture
            material.Shader.SetInt("u_hasRoughnessTexture", material.RoughnessTexture != null ? 1 : 0);

            // if (material.RoughnessTexture != null && material.Shader.RoughnessTextureUnit != null)
            // {
            //     GL.ActiveTexture(material.Shader.RoughnessTextureUnit.Value);
            //     TextureHelper.BindTexture(material.RoughnessTexture.AtlasGLTextureArrayId);
            // }

            material.Shader.SetFloat("u_metallic", material.MetallicTextureStrength);
            material.Shader.SetFloat("u_smoothness", material.Smoothness);

            // Metallic Texture
            material.Shader.SetInt("u_hasMetallicTexture", material.MetallicTexture != null ? 1 : 0);

            // if (material.MetallicTexture != null && material.Shader.MetallicTextureUnit != null)
            // {
            //     GL.ActiveTexture(material.Shader.MetallicTextureUnit.Value);
            //     TextureHelper.BindTexture(material.MetallicTexture.AtlasGLTextureArrayId);
            // }

            material.Shader.SetVector4("u_emissiveColor", material.EmissiveColor);
            // if (material.EmissiveTexture != null && material.Shader.EmissiveTextureUnit != null)
            // {
            //     GL.ActiveTexture(material.Shader.EmissiveTextureUnit.Value);
            //     TextureHelper.BindTexture(material.EmissiveTexture.AtlasGLTextureArrayId);
            // }
        }
    }

    private void GL_DrawArraysInstanced(PrimitiveType primitiveType, int first, int verticesCount, int instancesCount)
    {
        GL.DrawArraysInstanced(primitiveType, first, verticesCount, instancesCount);
        if (false)
        {
            DebugHelper.LogDrawCall();
            Debug.StatAddValue("Instanced objects drawn(arrays):", instancesCount);
            DebugHelper.LogVerticesDrawCall(verticesCount: verticesCount * instancesCount);
        }
    }

    private void GL_DrawElementsInstanced(PrimitiveType primitiveType, int indicesCount, int instancesCount)
    {
        GL.DrawElementsInstanced(primitiveType, indicesCount, DrawElementsType.UnsignedInt, IntPtr.Zero,
            instancesCount);
        DebugHelper.LogDrawCall();
        Debug.StatAddValue("Instanced objects drawn(elements):", instancesCount);
    }

    public bool UpdateObjectData(Renderer renderer, ref ObjectInstancingData objectInstancingData,
        // VertexBufferStructureType vertexBufferStructureType,
        Matrix4x4? modelMatrix = null, bool isStatic = false, bool remove = false, Color? color = null,
        Vector2? uvOffset = null, int indexForMultipleObjectsPerRenderer = 0)
    {
        RuntimeMesh mesh = renderer.RuntimeMesh;
        Asset_Material material = renderer.Material;
        if (mesh == null || material == null)
        {
            return false;
        }

        SharedInstancingBuffer sharedInstancingBuffer;
        if (objectInstancingData.InstancedRenderingDefinitionIndex == -1)
        {
            // no buffer exists for this combination-create one
            InstancedGroupDefinition definition = new InstancedGroupDefinition(
                // GameObjectNameForTestingIdentification: renderer.GameObject.Name,
                RuntimeMesh: mesh, Material: material, IsStatic: isStatic
                // vertexBufferStructureType: vertexBufferStructureType
            );
            // index: indexForMultipleObjectsPerRenderer);

            int definitionIndex = _groupDefinitions.Contains(definition)
                ? _groupDefinitions.IndexOf(definition)
                : _groupDefinitions.Count;


            // find bufferData if its already created
            if (_sharedInstancedBuffers.TryGetValue(definitionIndex, out SharedInstancingBuffer? data))
            {
                sharedInstancingBuffer = data;
            }
            else
            {
                if (remove)
                {
                    //buffer not created yet, we just return
                    return true;
                }

                _groupDefinitions.Add(definition);

                sharedInstancingBuffer = new SharedInstancingBuffer(definition);
                _sharedInstancedBuffers.Add(definitionIndex, sharedInstancingBuffer);


                // int shaderId = renderer.Material.Shader.ProgramId;
                // Get or create a group for this shader
                // Add definition index to this group
                int groupId = GetOrCreateGroupByShader(renderer.Material.Shader);
                _shaderGroups[groupId].GroupDefinitionIndexes.Add(definitionIndex);
            }

            objectInstancingData.InstancedRenderingDefinitionIndex = definitionIndex;
        }
        else
        {
            if (_sharedInstancedBuffers.ContainsKey(objectInstancingData.InstancedRenderingDefinitionIndex) == false)
            {
                // on scene reload the definitionIndex is 0 but its not created in the system...
                objectInstancingData.InstancedRenderingDefinitionIndex = -1;
                return false;
            }

            sharedInstancingBuffer = _sharedInstancedBuffers[objectInstancingData.InstancedRenderingDefinitionIndex];
        }


        if (objectInstancingData.StartingIndexInBuffer == -1 && remove == false)
        {
            // assign new InstancedRenderingIndex

            sharedInstancingBuffer.AddObject(ref objectInstancingData);
        }


        if (objectInstancingData.StartingIndexInBuffer != -1)
        {
            if (remove)
            {
                int groupId = GetOrCreateGroupByShader(renderer.Material.Shader);
                RemoveObject(sharedInstancingBuffer, ref objectInstancingData, groupId);

                return true;
            }

            else
            {
                CopyObjectDataToBuffer(modelMatrix ?? renderer.LatestModelMatrix.Value,
                    ref sharedInstancingBuffer.InstancingBuffer,
                    objectInstancingData.StartingIndexInBuffer, material: material, uvOffset: uvOffset,
                    mousePickingId: renderer.MousePickingId);
            }
        }

        sharedInstancingBuffer.NeedsUpload = true;
        _sharedInstancedBuffers[objectInstancingData.InstancedRenderingDefinitionIndex] = sharedInstancingBuffer;
        return true;
    }

    public void CopyObjectDataToBuffer(ref float[] buffer, InstancedRenderingBufferParameters bufferParameters)
    {
        CopyObjectDataToBuffer(modelMatrix: bufferParameters.ModelMatrix, ref buffer,
            startingIndex: bufferParameters.StartingIndexInInstancedBuffer,
            bufferParameters.Material, bufferParameters.UvOffset, bufferParameters.MousePickingId);
    }

    public void CopyObjectDataToBuffer(Matrix4x4 modelMatrix, ref float[] buffer,
        int startingIndex, Asset_Material material,
        Vector2? uvOffset = null, uint mousePickingId = 0)
    {
        int bufferIndex = startingIndex;
        buffer[bufferIndex++] = modelMatrix.M11;
        buffer[bufferIndex++] = modelMatrix.M12;
        buffer[bufferIndex++] = modelMatrix.M13;

        buffer[bufferIndex++] = modelMatrix.M21;
        buffer[bufferIndex++] = modelMatrix.M22;
        buffer[bufferIndex++] = modelMatrix.M23;

        buffer[bufferIndex++] = modelMatrix.M31;
        buffer[bufferIndex++] = modelMatrix.M32;
        buffer[bufferIndex++] = modelMatrix.M33;

        buffer[bufferIndex++] = modelMatrix.M41;
        buffer[bufferIndex++] = modelMatrix.M42;
        buffer[bufferIndex++] = modelMatrix.M43;

        buffer[bufferIndex++] = mousePickingId;


        // i dont have to add the atlas index to the whole vector4 but for now i will
        buffer[bufferIndex++] =
            (material.AlbedoTexture?.BoundingBoxInAtlas.X ?? 0) + material.AlbedoTexture?.IndexInAtlasTextureArray ?? 0;
        buffer[bufferIndex++] =
            (material.AlbedoTexture?.BoundingBoxInAtlas.Y ?? 0) + material.AlbedoTexture?.IndexInAtlasTextureArray ?? 0;
        buffer[bufferIndex++] =
            (material.AlbedoTexture?.BoundingBoxInAtlas.Z ?? 0) + material.AlbedoTexture?.IndexInAtlasTextureArray ?? 0;
        buffer[bufferIndex++] =
            (material.AlbedoTexture?.BoundingBoxInAtlas.W ?? 0) + material.AlbedoTexture?.IndexInAtlasTextureArray ?? 0;
        // uv = 0 - 1 = atlas 0
        // uv = 1 - 2 = atlas 1

        // buffer[bufferIndex++] = material.AlbedoTexture?.IndexInAtlasTextureArray ?? 0;

        // if (uvOffset != null)
        // {
        buffer[bufferIndex++] = uvOffset?.X ?? 0;
        buffer[bufferIndex++] = uvOffset?.Y ?? 0;
        // }
    }
}