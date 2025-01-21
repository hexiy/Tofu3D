namespace Tofu3D.Rendering.Instancing;

public class InstancedRenderingSystem
{
    private List<InstancedGroupDefinition> _groupDefinitions = new();

    // key is shaderID
    private Dictionary<int, ShaderGroup> _shaderGroups = new();

    // index in _definitions
    private Dictionary<int, SharedBuffer> _sharedBuffers = new();
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
        foreach (var pair in _sharedBuffers)
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

        _sharedBuffers = new Dictionary<int, SharedBuffer>();
        _groupDefinitions = new List<InstancedGroupDefinition>();
        _shaderGroups = new Dictionary<int, ShaderGroup>();
    }

    public void RenderShaderGroups(InstancingRenderMode renderMode)
    {
        // if mousepicking or depth, we set the shader first for all shadergroups
        if (Tofu.RenderPassSystem.CurrentRenderPassType == RenderPassType.MousePicking)
        {
            // _mousePickingMaterial = Tofu.AssetLoadManager.Load<Asset_Material>("ModelMousePicking.mat");
            Tofu.ShaderManager.UseShader(_mousePickingMaterial.Shader);

            _mousePickingMaterial.Shader.SetMatrix4X4("u_viewProjection",
                Camera.MainCamera.ViewMatrix * Camera.MainCamera.ProjectionMatrix);
        }

        else if (Tofu.RenderPassSystem.CurrentRenderPassType
                 is RenderPassType.DirectionalLightShadowDepth
                 or RenderPassType.PointLightShadowDepth
                 or RenderPassType.ZPrePass)
        {
            Tofu.ShaderManager.UseShader(_depthMaterial.Shader);

            // not material-dependent
            _depthMaterial.Shader.SetMatrix4X4("u_viewProjection",
                Camera.MainCamera.ViewMatrix * Camera.MainCamera.ProjectionMatrix);
        }

        // Iterate over shader groups
        foreach (var shaderGroup in _shaderGroups)
        {
            if (Tofu.RenderPassSystem.CurrentRenderPassType is RenderPassType.Opaques or RenderPassType.UI
                or RenderPassType.Transparency)
            {
                Shader shader = _groupDefinitions[shaderGroup.Value.DefinitionIndexes[0]].Material.Shader;
                // shader = Tofu.ShaderManager.LoadShader(shader.Path);
                Tofu.ShaderManager.UseShader(shader);

                SetGlobalUniforms(_groupDefinitions[shaderGroup.Value.DefinitionIndexes[0]].Material, shader);
            }

            foreach (var definitionIndexInThisShaderGroup in shaderGroup.Value.DefinitionIndexes)
            {
                if (_sharedBuffers.ContainsKey(definitionIndexInThisShaderGroup) == false)
                {
                    continue;
                }

                var bufferData = _sharedBuffers[definitionIndexInThisShaderGroup];
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

    private void RemoveObject(SharedBuffer groupBuffer,
        ref ObjectInstancingData objectInstancingData, int shaderGroupId)
    {
        int definitionIndex = objectInstancingData.InstancedRenderingDefinitionIndex;


        groupBuffer.RemoveObject(objectInstancingData);


        if (groupBuffer.NumberOfObjects == 0)
        {
            _sharedBuffers.Remove(definitionIndex);
            _groupDefinitions[definitionIndex] = null;


            /////////////////// remove SHADER GROUP
            _shaderGroups[shaderGroupId].DefinitionIndexes.Remove(definitionIndex);
            if (_shaderGroups[shaderGroupId].DefinitionIndexes.Count == 0)
            {
                _shaderGroups.Remove(shaderGroupId);
            }
            /////////////////// remove SHADER GROUP
        }


        objectInstancingData.StartingIndexInBuffer = -1;
        objectInstancingData.InstancedRenderingDefinitionIndex = -1;
    }


    private void RenderSpecific(int definitionIndex, SharedBuffer sharedBuffer)
    {
        // resize the buffer if needed, after drawing the old one
        // if (sharedBuffer.Buffer.Length !=
        //     sharedBuffer.InstancedVertexCountOfFloats * sharedBuffer.FutureMaxNumberOfObjects) this was better because we only want to resize once not for each new object
        // {
        //     sharedBuffer.ExpandBuffer();
        // }

        // sharedBuffer.NeedsUpload = true;


        sharedBuffer.SetupBufferAndUploadIfNeeded();


        InstancedGroupDefinition definition = _groupDefinitions[definitionIndex];
        Asset_Material material = definition.Material;
        int meshVao = definition.RuntimeMesh.Vao;
        int indicesCount = definition.RuntimeMesh.Mesh.Indices.Length;
        int numberOfObjects = sharedBuffer.NumberOfObjects;
        if (material.IgnoreDepth || material.NoDepth)
        {
            GL.Disable(EnableCap.DepthTest);
        }

        if (Tofu.RenderPassSystem.CurrentRenderPassType == RenderPassType.MousePicking)
        {
            RenderObjects_MousePickingPass(meshVao: meshVao, numberOfObjects: numberOfObjects,
                indicesCount: indicesCount, verticesCount: definition.RuntimeMesh.Mesh.VerticesCount,
                vbo: sharedBuffer.Vbo);
        }

        else if (Tofu.RenderPassSystem.CurrentRenderPassType
                 is RenderPassType.DirectionalLightShadowDepth
                 or RenderPassType.PointLightShadowDepth
                 or RenderPassType.ZPrePass)
        {
            if (material.NoDepth == false)
            {
                RenderObjects_DepthPasses(meshVao: meshVao, numberOfObjects: numberOfObjects,
                    indicesCount: indicesCount, verticesCount: definition.RuntimeMesh.Mesh.VerticesCount,
                    material: material, vbo: sharedBuffer.Vbo);
            }
        }

        else if (Tofu.RenderPassSystem.CurrentRenderPassType is RenderPassType.Opaques or RenderPassType.UI
                 or RenderPassType.Transparency)
        {
            RenderObjects_Opaques_UI_Transparency(meshVao: meshVao, numberOfObjects: numberOfObjects,
                indicesCount: indicesCount, verticesCount: definition.RuntimeMesh.Mesh.VerticesCount,
                material: material, vbo: sharedBuffer.Vbo);
        }

        if (material.IgnoreDepth || material.NoDepth)
        {
            GL.Enable(EnableCap.DepthTest);
        }

        Tofu.ShaderManager.BindVertexArray(0);

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

    private void SetGlobalUniforms(Asset_Material material, Shader shader)
    {
        TofuGL.CheckGlError("instanced rendering system 1");
        // uplad all the texture atlases only once per shader
        GL.ActiveTexture(TextureUnit.Texture0); // Activate texture unit 0
        GL.BindTexture(TextureTarget.Texture2DArray, Tofu.TextureAtlasManager.GLTextureArrayId);
        TofuGL.CheckGlError("instanced rendering system 2");

// Link the texture to the uniform in the shader
        int textureLocation = GL.GetUniformLocation(shader.ProgramId, "textureArray");
        GL.Uniform1(textureLocation, 0); // Texture array is bound to texture unit 0

        TofuGL.CheckGlError("instanced rendering system 3");

        bool discardTransparentPixels =
            material.BlendMode is BlendMode.Fade or BlendMode.PremultipliedAlpha or BlendMode.Additive;
        shader.SetInt("u_discardTransparentPixels", discardTransparentPixels ? 1 : 0);

        Tofu3D.Tofu.LightRenderingManager.BindPointLightsUBO(shader.ProgramId);
        shader.SetInt("_pointLightsCount", Tofu.LightRenderingManager.PointLightsCount);

        shader.SetFloat("u_cameraFrustumLength",
            Camera.MainCamera.FarPlaneDistance - Camera.MainCamera.NearPlaneDistance);

        shader.SetFloat("u_renderMode",
            (int)Tofu.RenderSettings.CurrentRenderModeSettings.CurrentRenderMode);

        shader.SetMatrix4X4("u_viewProjection",
            Camera.MainCamera.ViewMatrix * Camera.MainCamera.ProjectionMatrix);

        shader.SetVector3("u_camPosWorldSpace", Camera.MainCamera.Transform.WorldPosition);

        // LIGHTING
        shader.SetMatrix4X4("u_lightSpaceViewProjection", DirectionalLight.LightSpaceViewProjectionMatrix);


        var ambientColor = SceneLightingManager.I.GetAmbientLightsColor().ToVector4();
        ambientColor = new Vector4(ambientColor.X, ambientColor.Y, ambientColor.Z,
            Mathf.ClampMin(SceneLightingManager.I.GetAmbientLightsIntensity(), 0));
        shader.SetVector4("u_ambientLightColor", ambientColor);

        var directionalLightColor = SceneLightingManager.I.GetDirectionalLightColor().ToVector4();
        directionalLightColor.W = Mathf.ClampMin(SceneLightingManager.I.GetDirectionalLightIntensity(), 0);
        shader.SetVector4("u_directionalLightColor", directionalLightColor);

        var dir = SceneLightingManager.I.GetDirectionalLightDirection().Normalized();
        shader.SetVector3("u_directionalLightDirection",
            // SceneLightingManager.I.GetDirectionalLightDirection().Normalized());
            dir);
        // new Vector3(0,-1,0));


        //FOG
        var fogEnabled = Tofu.SceneManager.CurrentScene.SceneFogManager.FogEnabled;
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

        material.Shader.SetColor("u_albedoTint", material.AlbedoTint);
        // material.Shader.SetVector2("u_tiling", new Vector2(-1, -1)); //grass block
        material.Shader.SetVector2("u_tiling", material.Tiling); // normal 
        material.Shader.SetVector2("u_offset", material.Offset);


        material.Shader.SetInt("u_smoothShadows", material.SmoothShadows ? 1 : 0);


        material.Shader.SetInt("u_refractionEnabled", material.RefractionEnabled ? 1 : 0);
        material.Shader.SetFloat("u_refractiveIndex", material.RefractiveIndex);


        // Albedo Texture
        material.Shader.SetInt("u_hasAlbedoTexture", material.AlbedoTexture != null ? 1 : 0);
        if (material.Shader.IsLoaded == false)
        {
            return;
        }

        if (false)
        {
            if (material.AlbedoTexture != null && material.Shader?.AlbedoTextureIndexUnit != null)
            {
                GL.ActiveTexture(material.Shader.AlbedoTextureIndexUnit.Value);
                TextureHelper.BindTexture(material.AlbedoTexture.AtlasGLTextureArrayId);
            }


            // Alpha mask Texture
            material.Shader.SetInt("u_hasAlphaMaskTexture", material.AlphaMaskTexture != null ? 1 : 0);
            if (material.AlphaMaskTexture != null && material.Shader?.AlphaMaskTextureIndexUnit != null)
            {
                GL.ActiveTexture(material.Shader.AlphaMaskTextureIndexUnit.Value);
                TextureHelper.BindTexture(material.AlphaMaskTexture.AtlasGLTextureArrayId);
            }


            // Normal Texture
            material.Shader.SetInt("u_hasNormalTexture", material.NormalTexture != null ? 1 : 0);
            if (material.NormalTexture != null && material.Shader?.NormalTextureIndexUnit != null)
            {
                GL.ActiveTexture(material.Shader.NormalTextureIndexUnit.Value);
                TextureHelper.BindTexture(material.NormalTexture.AtlasGLTextureArrayId);
            }

            // Ambient Occlusion Texture
            material.Shader.SetInt("u_hasAmbientOcclusionTexture", material.AmbientOcclusionTexture != null ? 1 : 0);
            if (material.AmbientOcclusionTexture != null && material.Shader?.AmbientOcclusionTextureUnit != null)
            {
                GL.ActiveTexture(material.Shader.AmbientOcclusionTextureUnit.Value);
                TextureHelper.BindTexture(material.AmbientOcclusionTexture.AtlasGLTextureArrayId);
            }

            material.Shader.SetInt("u_hasShadowmapTexture",
                RenderPassDirectionalLightShadowDepth.I?.MainFramebuffer != null &&
                RenderPassDirectionalLightShadowDepth.I.Enabled &&
                material.Shader.ShadowMapTextureUnit != null
                    ? 1
                    : 0);

            if (RenderPassDirectionalLightShadowDepth.I?.MainFramebuffer != null &&
                material.Shader.ShadowMapTextureUnit != null)
            {
                GL.ActiveTexture(material.Shader.ShadowMapTextureUnit.Value);
                TextureHelper.BindTexture(RenderPassDirectionalLightShadowDepth.I.MainFramebuffer.DepthTextureId);
            }


            material.Shader.SetInt("u_hasEnvironmentCubemap",
                Camera.MainCamera?.GetComponent<Skybox>() != null ? 1 : 0);
            if (Camera.MainCamera?.GetComponent<Skybox>() != null && material.Shader.EnvironmentTextureUnit != null)
            {
                GL.ActiveTexture(material.Shader.EnvironmentTextureUnit.Value);
                TextureHelper.BindTexture(Camera.MainCamera.GetComponent<Skybox>().GetCubemapTexture().TextureId,
                    TextureType.Cubemap);
            }

            // Roughness Texture
            material.Shader.SetInt("u_hasRoughnessTexture", material.RoughnessTexture != null ? 1 : 0);

            if (material.RoughnessTexture != null && material.Shader.RoughnessTextureUnit != null)
            {
                GL.ActiveTexture(material.Shader.RoughnessTextureUnit.Value);
                TextureHelper.BindTexture(material.RoughnessTexture.AtlasGLTextureArrayId);
            }

            material.Shader.SetFloat("u_metallic", material.MetallicTextureStrength);
            material.Shader.SetFloat("u_smoothness", material.Smoothness);

            // Metallic Texture
            material.Shader.SetInt("u_hasMetallicTexture", material.MetallicTexture != null ? 1 : 0);

            if (material.MetallicTexture != null && material.Shader.MetallicTextureUnit != null)
            {
                GL.ActiveTexture(material.Shader.MetallicTextureUnit.Value);
                TextureHelper.BindTexture(material.MetallicTexture.AtlasGLTextureArrayId);
            }

            material.Shader.SetVector4("u_emissiveColor", material.EmissiveColor);
            if (material.EmissiveTexture != null && material.Shader.EmissiveTextureUnit != null)
            {
                GL.ActiveTexture(material.Shader.EmissiveTextureUnit.Value);
                TextureHelper.BindTexture(material.EmissiveTexture.AtlasGLTextureArrayId);
            }
        }
    }

    private void GL_DrawArraysInstanced(PrimitiveType primitiveType, int first, int verticesCount, int instancesCount)
    {
        GL.DrawArraysInstanced(primitiveType, first, verticesCount, instancesCount);
        DebugHelper.LogDrawCall();
        Debug.StatAddValue("Instanced objects drawn(arrays):", instancesCount);
        DebugHelper.LogVerticesDrawCall(verticesCount: verticesCount * instancesCount);
    }

    private void GL_DrawElementsInstanced(PrimitiveType primitiveType, int indicesCount, int instancesCount)
    {
        GL.DrawElementsInstanced(primitiveType, indicesCount, DrawElementsType.UnsignedInt, IntPtr.Zero,
            instancesCount);
        DebugHelper.LogDrawCall();
        Debug.StatAddValue("Instanced objects drawn(elements):", instancesCount);
    }

    public bool UpdateObjectData(Renderer renderer, ref ObjectInstancingData objectInstancingData,
        VertexBufferStructureType vertexBufferStructureType,
        Matrix4x4? modelMatrix = null, bool isStatic = false, bool remove = false, Color? color = null,
        Vector2? uvOffset = null, int indexForMultipleObjectsPerRenderer = 0)
    {
        RuntimeMesh mesh = renderer.RuntimeMesh;
        Asset_Material material = renderer.Material;
        if (mesh == null || material == null)
        {
            return false;
        }

        SharedBuffer sharedBuffer;
        if (objectInstancingData.InstancedRenderingDefinitionIndex == -1)
        {
            // no buffer exists for this combination-create one
            InstancedGroupDefinition definition = new(
                GameObjectNameForTestingIdentification: renderer.GameObject.Name, RuntimeMesh: mesh,
                Material: material,
                IsStatic: isStatic,
                vertexBufferStructureType: vertexBufferStructureType);
            // index: indexForMultipleObjectsPerRenderer);

            var definitionIndex = _groupDefinitions.Contains(definition)
                ? _groupDefinitions.IndexOf(definition)
                : _groupDefinitions.Count;


            // find bufferData if its already created
            if (_sharedBuffers.TryGetValue(definitionIndex, out var data))
            {
                sharedBuffer = data;
            }
            else
            {
                if (remove)
                {
                    //buffer not created yet, we just return
                    return true;
                }

                _groupDefinitions.Add(definition);

                sharedBuffer = InitializeSharedBufferData(definition);
                _sharedBuffers.Add(definitionIndex, sharedBuffer);


                // int shaderId = renderer.Material.Shader.ProgramId;
                // Get or create a group for this shader
                // Add definition index to this group
                int groupId = GetOrCreateGroupByShader(renderer.Material.Shader);
                _shaderGroups[groupId].DefinitionIndexes.Add(definitionIndex);
            }

            objectInstancingData.InstancedRenderingDefinitionIndex = definitionIndex;
        }
        else
        {
            if (_sharedBuffers.ContainsKey(objectInstancingData.InstancedRenderingDefinitionIndex) == false)
            {
                // on scene reload the definitionIndex is 0 but its not created in the system...
                objectInstancingData.InstancedRenderingDefinitionIndex = -1;
                return false;
            }

            sharedBuffer = _sharedBuffers[objectInstancingData.InstancedRenderingDefinitionIndex];
        }


        if (objectInstancingData.StartingIndexInBuffer == -1 && remove == false)
        {
            // assign new InstancedRenderingIndex

            sharedBuffer.AddObject(ref objectInstancingData);
        }


        if (objectInstancingData.StartingIndexInBuffer != -1)
        {
            if (remove)
            {
                int groupId = GetOrCreateGroupByShader(renderer.Material.Shader);
                RemoveObject(sharedBuffer, ref objectInstancingData, groupId);

                return true;
            }

            else
            {
                CopyObjectDataToBuffer(modelMatrix ?? renderer.LatestModelMatrix.Value,
                    ref sharedBuffer.Buffer,
                    objectInstancingData.StartingIndexInBuffer, material: material, uvOffset: uvOffset,
                    mousePickingId: renderer.MousePickingId);
            }
        }

        sharedBuffer.NeedsUpload = true;
        _sharedBuffers[objectInstancingData.InstancedRenderingDefinitionIndex] = sharedBuffer;
        return true;
    }

    private void CopyObjectDataToBuffer(Matrix4x4 modelMatrix, ref float[] buffer,
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

        if (uvOffset != null)
        {
            buffer[bufferIndex++] = uvOffset.Value.X;
            buffer[bufferIndex++] = uvOffset.Value.Y;
        }

        buffer[bufferIndex++] = material.AlbedoTexture?.BoundingBoxInAtlas.X ?? 0;
        buffer[bufferIndex++] = material.AlbedoTexture?.BoundingBoxInAtlas.Y ?? 0;
        buffer[bufferIndex++] = material.AlbedoTexture?.BoundingBoxInAtlas.Z ?? 0;
        buffer[bufferIndex++] = material.AlbedoTexture?.BoundingBoxInAtlas.W ?? 0;


        buffer[bufferIndex++] = material.AlbedoTexture?.IndexInAtlasTextureArray ?? 0;
    }

    private SharedBuffer InitializeSharedBufferData(InstancedGroupDefinition instancedGroupDefinition)
    {
        // Debug.Log("Initializing Instanced Buffer Data");
        Tofu.ShaderManager.BindVertexArray(instancedGroupDefinition.RuntimeMesh.Vao);

        instancedGroupDefinition.Material.LoadShader();
        if (instancedGroupDefinition.Material.Shader.IsLoaded == false)
        {
            Debug.LogError("Couldnt load shader");
            throw new Exception("Couldnt load shader");
        }

        SharedBuffer sharedBuffer = new()
        {
            VertexBufferStructureType = instancedGroupDefinition.vertexBufferStructureType,
            MaxNumberOfObjects = 1,
            // FutureMaxNumberOfObjects = 1,
            Vbo = -1,
            Vao = instancedGroupDefinition.RuntimeMesh.Vao,
            // Ebo = objectDefinition.RuntimeMesh.Ebo,
            ShaderId = instancedGroupDefinition.Material.Shader.ProgramId,
            UVOffsetIsInstanced = instancedGroupDefinition.Material.UVOffsetIsInstanced,
            RenderMode = instancedGroupDefinition.Material.RenderMode,
        };
        sharedBuffer.Init();

        sharedBuffer.Buffer = new float[sharedBuffer.MaxNumberOfObjects *
                                        sharedBuffer.InstancedVertexCountOfFloats];

        sharedBuffer.SetupBufferAndUploadIfNeeded();

        return sharedBuffer;
    }
}