using System.Collections.Immutable;
using System.IO;
using Tofu3D.Rendering;

namespace Tofu3D;

public class InstancedRenderingSystem
{
    private List<InstancedRenderingObjectDefinition> _definitions = new();

    // key is shaderID
    private Dictionary<int, InstancedRenderingShaderGroup> _shaderGroups = new();

    // index in _definitions
    private Dictionary<int, InstancedRenderingObjectBufferData> _objectBufferDatas = new();
    private Asset_Material _mousePickingMaterial;
    private Asset_Material _depthMaterial;

    private int GetOrCreateGroupByShader(Shader shader)
    {
        if (_shaderGroups.ContainsKey(shader.ProgramId) == false)
        {
            _shaderGroups[shader.ProgramId] = new InstancedRenderingShaderGroup() { Shader = shader };
            return shader.ProgramId;
        }

        return shader.ProgramId;
    }

    public void ClearBuffers()
    {
        /*foreach (var pair in _objectBufferDatas)
        { // need to care for left objects that use the same vao
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

        }*/

        _objectBufferDatas = new Dictionary<int, InstancedRenderingObjectBufferData>();
        _definitions = new List<InstancedRenderingObjectDefinition>();
        _shaderGroups = new Dictionary<int, InstancedRenderingShaderGroup>();
    }

    public void RenderShaderGroups(InstancingRenderMode renderMode)
    {
        // if mousepicking or depth, we set the shader first for all shadergroups
        if (Tofu.RenderPassSystem.CurrentRenderPassType == RenderPassType.MousePicking)
        {
            if (_mousePickingMaterial == null)
            {
                _mousePickingMaterial = new Asset_Material()
                {
                    Shader = Tofu.ShaderManager.LoadShader(TofuPath.Combine(Folders.ShadersInAssets,
                        "ModelMousePicking.glsl"))
                };
                _mousePickingMaterial.LoadShader();
            }

            // _mousePickingMaterial = Tofu.AssetLoadManager.Load<Asset_Material>("ModelMousePicking.mat");
            Tofu.ShaderManager.UseShader(_mousePickingMaterial.Shader);

            _mousePickingMaterial.Shader.SetMatrix4X4("u_viewProjection",
                Camera.MainCamera.ViewMatrix * Camera.MainCamera.ProjectionMatrix);
        }

        else if (Tofu.RenderPassSystem.CurrentRenderPassType is RenderPassType.DirectionalLightShadowDepth
                 or RenderPassType.ZPrePass)
        {
            if (_depthMaterial == null)
            {
                _depthMaterial = new Asset_Material()
                {
                    Shader = Tofu.ShaderManager.LoadShader(TofuPath.Combine(Folders.ShadersInAssets,
                        "ModelRendererInstancedDepth.glsl"))
                };

                _depthMaterial.LoadShader();
            }


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
                Shader shader = _definitions[shaderGroup.Value.DefinitionIndexes[0]].Material.Shader;
                // shader = Tofu.ShaderManager.LoadShader(shader.Path);
                Tofu.ShaderManager.UseShader(shader);

                SetGlobalUniforms(shader);
            }


            foreach (var definitionIndexInThisShaderGroup in shaderGroup.Value.DefinitionIndexes)
            {
                var bufferData = _objectBufferDatas[definitionIndexInThisShaderGroup];
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

    private void RemoveObjectFromBuffer(InstancedRenderingObjectBufferData bufferData,
        RendererInstancingData instancingData)
    {
        for (var i = 0; i < bufferData.InstancedVertexCountOfFloats; i++)
        {
            bufferData.Buffer[instancingData.InstancedRenderingStartingIndexInBuffer + i] = 0;
        }

        bufferData.EmptyStartIndexes.Add(instancingData.InstancedRenderingStartingIndexInBuffer);

        instancingData.InstancedRenderingStartingIndexInBuffer = -1;
        instancingData.InstancedRenderingDefinitionIndex = -1;
        bufferData.NumberOfObjects--;
    }

    private int GetEmptyIndexInBuffer(InstancedRenderingObjectBufferData bufferData)
    {
        if (bufferData.EmptyStartIndexes.Count > 0)
        {
            var index = bufferData.EmptyStartIndexes[0];
            bufferData.EmptyStartIndexes.RemoveAt(0);
            return index;
        }

        if (bufferData.NumberOfObjects == bufferData.MaxNumberOfObjects)
        {
            bufferData.FutureMaxNumberOfObjects += 1;
            return -1;
        }

        if (bufferData.Buffer.Length < bufferData.InstancedVertexCountOfFloats * bufferData.MaxNumberOfObjects)
        {
            return -1;
        }

        return bufferData.NumberOfObjects * bufferData.InstancedVertexCountOfFloats;
    }

    private void ResizeBufferData(InstancedRenderingObjectBufferData bufferData)
    {
        bufferData.FutureMaxNumberOfObjects += 5;
        if (bufferData.FutureMaxNumberOfObjects > 1000)
        {
            bufferData.FutureMaxNumberOfObjects += 20;
        }

        bufferData.MaxNumberOfObjects = bufferData.FutureMaxNumberOfObjects;
        Debug.Log($"Resizing buffer to new size:{bufferData.MaxNumberOfObjects}");

        Array.Resize(ref bufferData.Buffer, bufferData.MaxNumberOfObjects * bufferData.InstancedVertexCountOfFloats);
        GL.DeleteBuffer(bufferData.Vbo);
        bufferData.Vbo = -1;
        bufferData.NeedsUpload = true;
    }

    private void RenderSpecific(int definitionIndex, InstancedRenderingObjectBufferData bufferData)
    {
        // resize the buffer if needed, after drawing the old one
        if (bufferData.Buffer.Length != bufferData.InstancedVertexCountOfFloats * bufferData.FutureMaxNumberOfObjects)
        {
            ResizeBufferData(bufferData);
        }

        if (bufferData.NeedsUpload)
        {
            UploadBufferData(bufferData);
            // if (Tofu.RenderPassSystem.CurrentRenderPassType is RenderPassType.Opaques or RenderPassType.Transparency)
            // {
            //     objectBufferPair.Value.NeedsUpload = false;
            // }
            bufferData.NeedsUpload = false;
        }

        // if (material == TransformHandle.I?.ModelRendererX?.Material)
        // {
        //     GL.Disable(EnableCap.DepthTest);
        // }
        // else
        // {
        //     GL.Enable(EnableCap.DepthTest);
        // }

        InstancedRenderingObjectDefinition definition = _definitions[definitionIndex];
        Asset_Material material = definition.Material;
        int meshVao = definition.RuntimeMesh.Vao;
        int indicesCount = definition.RuntimeMesh.Mesh.Indices.Length;
        int numberOfObjects = bufferData.NumberOfObjects;

        // GL.Enable(EnableCap.DepthTest);
        if (Tofu.RenderPassSystem.CurrentRenderPassType == RenderPassType.MousePicking)
        {
            RenderObjects_MousePickingPass(meshVao: meshVao, numberOfObjects: numberOfObjects,
                indicesCount: indicesCount, verticesCount: definition.RuntimeMesh.Mesh.VerticesCount);
        }

        else if (Tofu.RenderPassSystem.CurrentRenderPassType is RenderPassType.DirectionalLightShadowDepth
                 or RenderPassType.ZPrePass)
        {
            RenderObjects_DepthPasses(meshVao: meshVao, numberOfObjects: numberOfObjects,
                indicesCount: indicesCount, verticesCount: definition.RuntimeMesh.Mesh.VerticesCount,
                material: material);
        }

        else if (Tofu.RenderPassSystem.CurrentRenderPassType is RenderPassType.Opaques or RenderPassType.UI
                 or RenderPassType.Transparency)
        {
            RenderObjects_Opaques_UI_Transparency(meshVao: meshVao, numberOfObjects: numberOfObjects,
                indicesCount: indicesCount, verticesCount: definition.RuntimeMesh.Mesh.VerticesCount,
                material: material, vbo: bufferData.Vbo);
        }

        ImGuiController.CheckGlError("instanced rendering error");
    }

    private void RenderObjects_MousePickingPass(int meshVao, int numberOfObjects, int indicesCount, int verticesCount)
    {
        Tofu.ShaderManager.BindVertexArray(meshVao);


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
        Asset_Material material)
    {
        // if (material.RenderMode == RenderMode.Transparent)
        // {
        // dont render depth for transparent objects
        // return;
        // }


        Tofu.ShaderManager.BindVertexArray(meshVao);

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

    private void RenderObjects_Opaques_UI_Transparency(int meshVao, int numberOfObjects, int indicesCount,
        int verticesCount,
        Asset_Material material, int vbo)
    {
        SetMaterialSpecificUniforms(material);

        RenderingBlendingHelper.SetBlendMode(material.BlendMode);

        Tofu.ShaderManager.BindVertexArray(meshVao);

        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
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

    private void SetGlobalUniforms(Shader shader)
    {
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

        if (material.AlbedoTexture != null && material.Shader?.AlbedoTextureIndexUnit != null)
        {
            GL.ActiveTexture(material.Shader.AlbedoTextureIndexUnit.Value);
            TextureHelper.BindTexture(material.AlbedoTexture.TextureId);
        }


        // Alpha mask Texture
        material.Shader.SetInt("u_hasAlphaMaskTexture", material.AlphaMaskTexture != null ? 1 : 0);
        if (material.AlphaMaskTexture != null && material.Shader?.AlphaMaskTextureIndexUnit != null)
        {
            GL.ActiveTexture(material.Shader.AlphaMaskTextureIndexUnit.Value);
            TextureHelper.BindTexture(material.AlphaMaskTexture.TextureId);
        }


        // Normal Texture
        material.Shader.SetInt("u_hasNormalTexture", material.NormalTexture != null ? 1 : 0);
        if (material.NormalTexture != null && material.Shader?.NormalTextureIndexUnit != null)
        {
            GL.ActiveTexture(material.Shader.NormalTextureIndexUnit.Value);
            TextureHelper.BindTexture(material.NormalTexture.TextureId);
        }

        // Ambient Occlusion Texture
        material.Shader.SetInt("u_hasAmbientOcclusionTexture", material.AmbientOcclusionTexture != null ? 1 : 0);
        if (material.AmbientOcclusionTexture != null && material.Shader?.AmbientOcclusionTextureUnit != null)
        {
            GL.ActiveTexture(material.Shader.AmbientOcclusionTextureUnit.Value);
            TextureHelper.BindTexture(material.AmbientOcclusionTexture.TextureId);
        }

        material.Shader.SetInt("u_hasShadowmapTexture",
            RenderPassDirectionalLightShadowDepth.I?.MainFramebuffer != null &&
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
            TextureHelper.BindTexture(material.RoughnessTexture.TextureId);
        }

        material.Shader.SetFloat("u_metallic", material.MetallicTextureStrength);
        material.Shader.SetFloat("u_smoothness", material.Smoothness);

        // Metallic Texture
        material.Shader.SetInt("u_hasMetallicTexture", material.MetallicTexture != null ? 1 : 0);

        if (material.MetallicTexture != null && material.Shader.MetallicTextureUnit != null)
        {
            GL.ActiveTexture(material.Shader.MetallicTextureUnit.Value);
            TextureHelper.BindTexture(material.MetallicTexture.TextureId);
        }

        material.Shader.SetVector4("u_emissiveColor", material.EmissiveColor);
        if (material.EmissiveTexture != null && material.Shader.EmissiveTextureUnit != null)
        {
            GL.ActiveTexture(material.Shader.EmissiveTextureUnit.Value);
            TextureHelper.BindTexture(material.EmissiveTexture.TextureId);
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

    public bool UpdateObjectData(Renderer renderer, ref RendererInstancingData instancingData,
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

        InstancedRenderingObjectBufferData bufferData;
        if (instancingData.InstancedRenderingDefinitionIndex == -1)
        {
            // no buffer exists for this combination-create one
            InstancedRenderingObjectDefinition definition = new(
                GameObjectNameForTestingIdentification: renderer.GameObject.Name, RuntimeMesh: mesh,
                Material: material,
                IsStatic: isStatic,
                vertexBufferStructureType: vertexBufferStructureType);
            // index: indexForMultipleObjectsPerRenderer);

            var definitionIndex = _definitions.Contains(definition)
                ? _definitions.IndexOf(definition)
                : _definitions.Count;


            // find bufferData if its already created
            if (_objectBufferDatas.TryGetValue(definitionIndex, out var data))
            {
                bufferData = data;
            }
            else
            {
                if (remove)
                {
                    //buffer not created yet, we just return
                    return true;
                }

                _definitions.Add(definition);

                bufferData = InitializeBufferData(definition);
                _objectBufferDatas.Add(definitionIndex, bufferData);


                int shaderId = renderer.Material.Shader.ProgramId;
                // Get or create a group for this shader
                // Add definition index to this group
                int groupId = GetOrCreateGroupByShader(renderer.Material.Shader);
                _shaderGroups[groupId].DefinitionIndexes.Add(definitionIndex);
            }

            instancingData.InstancedRenderingDefinitionIndex = definitionIndex;
        }
        else
        {
            if (_objectBufferDatas.ContainsKey(instancingData.InstancedRenderingDefinitionIndex) == false)
            {
                // on scene reload the definitionIndex is 0 but its not created in the system...
                instancingData.InstancedRenderingDefinitionIndex = -1;
                return false;
            }

            bufferData = _objectBufferDatas[instancingData.InstancedRenderingDefinitionIndex];
        }


        if (instancingData.InstancedRenderingStartingIndexInBuffer == -1 && remove == false)
        {
            // assign new InstancedRenderingIndex
            instancingData.InstancedRenderingStartingIndexInBuffer = GetEmptyIndexInBuffer(bufferData);

            if (instancingData.InstancedRenderingStartingIndexInBuffer == -1)
            {
                return false;
            }

            bufferData.NumberOfObjects++;
        }

        bufferData.NeedsUpload = true;

        if (instancingData.InstancedRenderingStartingIndexInBuffer != -1)
        {
            if (remove)
            {
                CopyObjectDataToBuffer(Matrix4x4.CreateScale(0, 0, 0),
                    ref bufferData.Buffer,
                    instancingData.InstancedRenderingStartingIndexInBuffer, uvOffset: uvOffset,
                    mousePickingId: renderer.MousePickingId);
                UploadBufferData(bufferData);
                RemoveObjectFromBuffer(bufferData, instancingData);
            }

            else
            {
                CopyObjectDataToBuffer(modelMatrix ?? renderer.LatestModelMatrix,
                    ref bufferData.Buffer,
                    instancingData.InstancedRenderingStartingIndexInBuffer, uvOffset: uvOffset,
                    mousePickingId: renderer.MousePickingId);
            }
        }


        _objectBufferDatas[instancingData.InstancedRenderingDefinitionIndex] = bufferData;
        return true;
    }

    private void CopyObjectDataToBuffer(Matrix4x4 modelMatrix, ref float[] buffer,
        int startingIndex,
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
    }

    private InstancedRenderingObjectBufferData InitializeBufferData(InstancedRenderingObjectDefinition objectDefinition)
    {
        // Debug.Log("Initializing Instanced Buffer Data");
        GL.BindVertexArray(objectDefinition.RuntimeMesh.Vao);

        objectDefinition.Material.LoadShader();
        if (objectDefinition.Material.Shader.IsLoaded == false)
        {
            Debug.LogError("Couldnt load shader");
            throw new Exception("Couldnt load shader");
        }

        InstancedRenderingObjectBufferData bufferData = new()
        {
            VertexBufferStructureType = objectDefinition.vertexBufferStructureType,
            MaxNumberOfObjects = 1,
            FutureMaxNumberOfObjects = 1,
            Vbo = -1,
            Vao = objectDefinition.RuntimeMesh.Vao,
            // Ebo = objectDefinition.RuntimeMesh.Ebo,
            ShaderId = objectDefinition.Material.Shader.ProgramId,
            NumberOfObjects = 0,
            UVOffsetIsInstanced = objectDefinition.Material.UVOffsetIsInstanced,
            RenderMode = objectDefinition.Material.RenderMode,
        };
        bufferData.Init();

        bufferData.Buffer = new float[bufferData.MaxNumberOfObjects * bufferData.InstancedVertexCountOfFloats];
        bufferData.EmptyStartIndexes = new List<int>();

        UploadBufferData(bufferData);

        return bufferData;
    }

    private void UploadBufferData(InstancedRenderingObjectBufferData bufferData)
    {
        GL.BindVertexArray(bufferData.Vao);

        var newBuffer = false;
        if (bufferData.Vbo == -1)
        {
            newBuffer = true;
            bufferData.Vbo = GL.GenBuffer();
        }

        GL.BindBuffer(BufferTarget.ArrayBuffer, bufferData.Vbo);

        // unique attribs for each instance
        GL.EnableVertexAttribArray(5);
        GL.EnableVertexAttribArray(6);
        GL.EnableVertexAttribArray(7);
        GL.EnableVertexAttribArray(8);
        GL.EnableVertexAttribArray(9);

        // https://stackoverflow.com/a/28597384
        //  _vertexDataLength * sizeof(float) = 4 bytes * 16 numbers =  64
        int offset = 0;
        GL.VertexAttribPointer(5, 3, VertexAttribPointerType.Float, false,
            bufferData.InstancedVertexDataSizeInBytes,
            offset);
        offset += 3 * sizeof(float);
        GL.VertexAttribPointer(6, 3, VertexAttribPointerType.Float, false,
            bufferData.InstancedVertexDataSizeInBytes,
            offset);
        offset += 3 * sizeof(float);

        GL.VertexAttribPointer(7, 3, VertexAttribPointerType.Float, false,
            bufferData.InstancedVertexDataSizeInBytes,
            offset);
        offset += 3 * sizeof(float);

        GL.VertexAttribPointer(8, 3, VertexAttribPointerType.Float, false,
            bufferData.InstancedVertexDataSizeInBytes,
            offset);
        offset += 3 * sizeof(float);

        GL.VertexAttribPointer(9, 1, VertexAttribPointerType.Float, false,
            bufferData.InstancedVertexDataSizeInBytes,
            offset);
        offset += sizeof(float);

        if (bufferData.UVOffsetIsInstanced)
        {
            GL.EnableVertexAttribArray(10);
            GL.VertexAttribPointer(10, 2, VertexAttribPointerType.Float, false,
                bufferData.InstancedVertexDataSizeInBytes,
                offset);
            offset += 2 * sizeof(float);

            GL.VertexAttribDivisor(10, 1);
        }

        GL.VertexAttribDivisor(5, 1);
        GL.VertexAttribDivisor(6, 1);
        GL.VertexAttribDivisor(7, 1);
        GL.VertexAttribDivisor(8, 1);
        GL.VertexAttribDivisor(9, 1);


        if (newBuffer)
        {
            GL.BufferData(BufferTarget.ArrayBuffer,
                sizeof(float) * bufferData.Buffer.Length,
                bufferData.Buffer, BufferUsageHint.StaticDraw);
        }
        else
        {
            GL.BufferSubData(BufferTarget.ArrayBuffer, 0,
                sizeof(float) * bufferData.Buffer.Length,
                bufferData.Buffer);
        }


        GL.BindVertexArray(0);
    }
}