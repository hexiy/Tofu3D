namespace Tofu3D;

public class InstancedRenderingSystem
{
    private List<InstancedRenderingObjectDefinition> _definitions = new();

    // index in _definitions
    private Dictionary<int, InstancedRenderingObjectBufferData> _objectBufferDatas = new();

    public void ClearBuffers()
    {
        foreach (var pair in _objectBufferDatas)
        {
            GL.DeleteBuffer(pair.Value.Vbo);
        }

        _objectBufferDatas = new Dictionary<int, InstancedRenderingObjectBufferData>();
        _definitions = new List<InstancedRenderingObjectDefinition>();
    }

    public void RenderInstances()
    {
        // GL.Enable(EnableCap.DepthTest);

        foreach (var objectDefinitionBufferPair in _objectBufferDatas)
        {
            if (objectDefinitionBufferPair.Value.NumberOfObjects == 0)
            {
                continue;
            }

            RenderSpecific(objectDefinitionBufferPair);
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
        // renderer.InstancedRenderingDefinitionIndex = -1;
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
        bufferData.FutureMaxNumberOfObjects += 10; // 10 in the tank
        if (bufferData.FutureMaxNumberOfObjects > 1000)
        {
            bufferData.FutureMaxNumberOfObjects += 500;
        }

        bufferData.MaxNumberOfObjects = bufferData.FutureMaxNumberOfObjects;
        // Debug.Log($"Resizing buffer to new size:{bufferData.MaxNumberOfObjects}");

        Array.Resize(ref bufferData.Buffer, bufferData.MaxNumberOfObjects * bufferData.InstancedVertexCountOfFloats);
        bufferData.Vbo = -1;
        bufferData.NeedsUpload = true;
    }

    private void RenderSpecific(KeyValuePair<int, InstancedRenderingObjectBufferData> objectBufferPair)
    {
        var definitionIndex = objectBufferPair.Key;
        var definition = _definitions[definitionIndex];
        var material = definition.Material;
        material = Tofu.AssetManager.Load<Material>(material.Path);
        var mesh = definition.Mesh;
        var bufferData = objectBufferPair.Value;
        // GL.Enable(EnableCap.DepthTest);
        if (Tofu.RenderPassSystem.CurrentRenderPassType is RenderPassType.DirectionalLightShadowDepth
            or RenderPassType.ZPrePass)
        {
            var depthMaterial = Tofu.AssetManager.Load<Material>("ModelRendererInstancedDepth");
            Tofu.ShaderManager.UseShader(depthMaterial.Shader);
            depthMaterial.Shader.SetMatrix4X4("u_viewProjection",
                Camera.MainCamera.ViewMatrix * Camera.MainCamera.ProjectionMatrix);


            Tofu.ShaderManager.BindVertexArray(mesh.Vao);

            GL_DrawArraysInstanced(PrimitiveType.Triangles, 0, mesh.VerticesCount,
                objectBufferPair.Value.NumberOfObjects);
        }

        else if (Tofu.RenderPassSystem.CurrentRenderPassType is RenderPassType.Opaques or RenderPassType.UI)
        {
            Tofu.ShaderManager.UseShader(material.Shader);


            material.Shader.SetFloat("u_renderMode",
                (int)Tofu.RenderSettings.CurrentRenderModeSettings.CurrentRenderMode);

            material.Shader.SetMatrix4X4("u_viewProjection",
                Camera.MainCamera.ViewMatrix * Camera.MainCamera.ProjectionMatrix);

            material.Shader.SetColor("u_albedoTint", material.AlbedoTint);
            // material.Shader.SetVector2("u_tiling", new Vector2(-1, -1)); //grass block
            material.Shader.SetVector2("u_tiling", material.Tiling); // normal 
            material.Shader.SetVector2("u_offset", material.Offset);

            material.Shader.SetVector3("u_camPos", Camera.MainCamera.Transform.WorldPosition);

            // LIGHTING
            material.Shader.SetMatrix4X4("u_lightSpaceViewProjection", DirectionalLight.LightSpaceViewProjectionMatrix);


            var ambientColor = SceneLightingManager.I.GetAmbientLightsColor().ToVector4();
            ambientColor = new Vector4(ambientColor.X, ambientColor.Y, ambientColor.Z,
                SceneLightingManager.I.GetAmbientLightsIntensity());
            material.Shader.SetVector4("u_ambientLightColor", ambientColor);

            var directionalLightColor = SceneLightingManager.I.GetDirectionalLightColor().ToVector4();
            directionalLightColor = new Vector4(directionalLightColor.X, directionalLightColor.Y,
                directionalLightColor.Z, SceneLightingManager.I.GetDirectionalLightIntensity());
            material.Shader.SetVector4("u_directionalLightColor", directionalLightColor);
            material.Shader.SetVector3("u_directionalLightDirection",
                SceneLightingManager.I.GetDirectionalLightDirection().Normalized());
Debug.Log("light dir:"+SceneLightingManager.I.GetDirectionalLightDirection().Normalized());

            material.Shader.SetFloat("u_specularSmoothness", material.SpecularSmoothness);
            material.Shader.SetFloat("u_specularHighlightsEnabled", material.SpecularHighlightsEnabled ? 1 : 0);

            //FOG
            var fogEnabled = Tofu.SceneManager.CurrentScene.SceneFogManager.FogEnabled;
            material.Shader.SetFloat("u_fogEnabled", fogEnabled ? 1 : 0);
            if (fogEnabled)
            {
                material.Shader.SetColor("u_fogColor", Tofu.SceneManager.CurrentScene.SceneFogManager.FogColor1);
                material.Shader.SetFloat("u_fogIntensity", Tofu.SceneManager.CurrentScene.SceneFogManager.Intensity);
                if (Tofu.SceneManager.CurrentScene.SceneFogManager.IsGradient)
                {
                    material.Shader.SetColor("u_fogColor2", Tofu.SceneManager.CurrentScene.SceneFogManager.FogColor2);
                    material.Shader.SetFloat("u_fogGradientSmoothness",
                        Tofu.SceneManager.CurrentScene.SceneFogManager.GradientSmoothness);
                }
                else
                {
                    material.Shader.SetColor("u_fogColor2", Tofu.SceneManager.CurrentScene.SceneFogManager.FogColor1);
                }

                material.Shader.SetFloat("u_fogStartDistance",
                    Tofu.SceneManager.CurrentScene.SceneFogManager.FogStartDistance);
                material.Shader.SetFloat("u_fogEndDistance",
                    Tofu.SceneManager.CurrentScene.SceneFogManager.FogEndDistance);
                material.Shader.SetFloat("u_fogPositionY",
                    Tofu.SceneManager.CurrentScene.SceneFogManager.FogPositionY);
            }

            // material.Shader.SetFloat("u_aoStrength", _normalDisabled ? 0 : 1);

            // Albedo Texture
            if (material.AlbedoTexture)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                TextureHelper.BindTexture(material.AlbedoTexture.TextureId);
            }

            // Normal Texture
            if (material.NormalTexture)
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                TextureHelper.BindTexture(material.NormalTexture.TextureId);
            }
            
            // Ambient Occlusion Texture
            if (material.AmbientOcclusionTexture)
            {
                GL.ActiveTexture(TextureUnit.Texture2);
                TextureHelper.BindTexture(material.AmbientOcclusionTexture.TextureId);
            }
            
            if (RenderPassDirectionalLightShadowDepth.I?.PassRenderTexture != null)
            {
                GL.ActiveTexture(TextureUnit.Texture3);
                TextureHelper.BindTexture(RenderPassDirectionalLightShadowDepth.I.PassRenderTexture.DepthAttachment);
            }


            Tofu.ShaderManager.BindVertexArray(mesh.Vao);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);


            GL_DrawArraysInstanced(PrimitiveType.Triangles, 0, mesh.VerticesCount,
                objectBufferPair.Value.NumberOfObjects);

            // GL.ActiveTexture(TextureUnit.Texture0); // DOESNT WORK
        }


        // resize the buffer if needed, after drawing the old one
        if (bufferData.Buffer.Length != bufferData.InstancedVertexCountOfFloats * bufferData.FutureMaxNumberOfObjects)
        {
            ResizeBufferData(bufferData);
        }

        if (objectBufferPair.Value.NeedsUpload)
        {
            UploadBufferData(objectBufferPair.Value);
            if (Tofu.RenderPassSystem.CurrentRenderPassType == RenderPassType.Opaques)
                // hmm i though this would fix the flicker
            {
                objectBufferPair.Value.NeedsUpload = false;
            }
        }
    }

    private void GL_DrawArraysInstanced(PrimitiveType primitiveType, int first, int verticesCount, int instancesCount)
    {
        GL.DrawArraysInstanced(primitiveType, first, verticesCount, instancesCount);
        DebugHelper.LogDrawCall();
        Debug.StatAddValue("Instanced objects drawn:", instancesCount);
        // Debug.StatAddValue("Total vertices:", verticesCount * instancesCount);
        DebugHelper.LogVerticesDrawCall(verticesCount: verticesCount * instancesCount);
    }

    public bool UpdateObjectData(Renderer renderer, ref RendererInstancingData instancingData,
        VertexBufferStructureType vertexBufferStructureType,
        Matrix4x4? modelMatrix = null, bool isStatic = false, bool remove = false, Color? color = null)
    {
        var mesh = renderer.Mesh;
        var material = renderer.Material;
        if (mesh == null)
        {
            return false;
        }

        InstancedRenderingObjectBufferData bufferData;
        if (instancingData.InstancedRenderingDefinitionIndex == -1)
        {
            // no buffer exists for this combination-create one
            InstancedRenderingObjectDefinition definition = new(mesh, material, isStatic, vertexBufferStructureType);
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
                _definitions.Add(definition);

                bufferData = InitializeBufferData(definition);
                _objectBufferDatas.Add(definitionIndex, bufferData);
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

        if (instancingData.InstancedRenderingStartingIndexInBuffer != -1 && remove)
        {
            RemoveObjectFromBuffer(bufferData, instancingData);
        }
        else if (instancingData.InstancedRenderingStartingIndexInBuffer != -1)
        {
            CopyObjectDataToBuffer(color ?? renderer.Color, modelMatrix ?? renderer.GetModelMatrix(),
                ref bufferData.Buffer,
                instancingData.InstancedRenderingStartingIndexInBuffer);
        }

        _objectBufferDatas[instancingData.InstancedRenderingDefinitionIndex] = bufferData;

        return true;
    }

    private void CopyObjectDataToBuffer(Color color, Matrix4x4 modelMatrix, ref float[] buffer, int startingIndex)
    {
        buffer[startingIndex + 0] = modelMatrix.M11;
        buffer[startingIndex + 1] = modelMatrix.M12;
        buffer[startingIndex + 2] = modelMatrix.M13;
        // buffer[startingIndex + 3] = 0;//
        buffer[startingIndex + 3] = modelMatrix.M21;
        buffer[startingIndex + 4] = modelMatrix.M22;
        buffer[startingIndex + 5] = modelMatrix.M23;
        // buffer[startingIndex + 7] = 0;//
        buffer[startingIndex + 6] = modelMatrix.M31;
        buffer[startingIndex + 7] = modelMatrix.M32;
        buffer[startingIndex + 8] = modelMatrix.M33;
        // buffer[startingIndex + 11] = 0;//
        buffer[startingIndex + 9] = modelMatrix.M41;
        buffer[startingIndex + 10] = modelMatrix.M42;
        buffer[startingIndex + 11] = modelMatrix.M43;
        // buffer[startingIndex + 15] = 1;//

        buffer[startingIndex + 12] = color.R / 255f;
        buffer[startingIndex + 13] = color.G / 255f;
        buffer[startingIndex + 14] = color.B / 255f;
        buffer[startingIndex + 15] = color.A / 255f;
    }

    private InstancedRenderingObjectBufferData InitializeBufferData(InstancedRenderingObjectDefinition objectDefinition)
    {
        Debug.Log("Initializing Instanced Buffer Data");
        GL.BindVertexArray(objectDefinition.Mesh.Vao);

        InstancedRenderingObjectBufferData bufferData = new()
        {
            VertexBufferStructureType = objectDefinition.vertexBufferStructureType,
            MaxNumberOfObjects = 1,
            FutureMaxNumberOfObjects = 1,
            Vbo = -1,
            NumberOfObjects = 0
        };

        bufferData.Buffer = new float[bufferData.MaxNumberOfObjects * bufferData.InstancedVertexCountOfFloats];
        bufferData.EmptyStartIndexes = new List<int>();

        UploadBufferData(bufferData);

        return bufferData;
    }

    private void UploadBufferData(InstancedRenderingObjectBufferData bufferData)
    {
        var newBuffer = false;
        if (bufferData.Vbo == -1)
        {
            newBuffer = true;
            bufferData.Vbo = GL.GenBuffer();
        }

        GL.BindBuffer(BufferTarget.ArrayBuffer, bufferData.Vbo);

        if (newBuffer)
        {
            GL.BufferData(BufferTarget.ArrayBuffer,
                bufferData._instancedVertexDataSizeInBytes * bufferData.MaxNumberOfObjects,
                bufferData.Buffer, BufferUsageHint.StaticDraw);

            if (bufferData.VertexBufferStructureType == VertexBufferStructureType.Model)
            {
                // unique attribs for each instance
                GL.EnableVertexAttribArray(5);
                GL.EnableVertexAttribArray(6);
                GL.EnableVertexAttribArray(7);
                GL.EnableVertexAttribArray(8);
                GL.EnableVertexAttribArray(9);

                // https://stackoverflow.com/a/28597384
                //  _vertexDataLength * sizeof(float) = 4 bytes * 16 numbers =  64
                GL.VertexAttribPointer(5, sizeof(float), VertexAttribPointerType.Float, false,
                    bufferData._instancedVertexDataSizeInBytes,
                    0);
                GL.VertexAttribPointer(6, sizeof(float), VertexAttribPointerType.Float, false,
                    bufferData._instancedVertexDataSizeInBytes,
                    1 * 3 * sizeof(float));
                GL.VertexAttribPointer(7, sizeof(float), VertexAttribPointerType.Float, false,
                    bufferData._instancedVertexDataSizeInBytes,
                    2 * 3 * sizeof(float));
                GL.VertexAttribPointer(8, sizeof(float), VertexAttribPointerType.Float, false,
                    bufferData._instancedVertexDataSizeInBytes,
                    3 * 3 * sizeof(float));
                GL.VertexAttribPointer(9, sizeof(float), VertexAttribPointerType.Float, false,
                    bufferData._instancedVertexDataSizeInBytes,
                    4 * 3 * sizeof(float));


                GL.VertexAttribDivisor(5, 1);
                GL.VertexAttribDivisor(6, 1);
                GL.VertexAttribDivisor(7, 1);
                GL.VertexAttribDivisor(8, 1);
                GL.VertexAttribDivisor(9, 1);
            }

            if (bufferData.VertexBufferStructureType == VertexBufferStructureType.Quad)
            {
                // unique attribs for each instance
                GL.EnableVertexAttribArray(5);
                GL.EnableVertexAttribArray(6);
                GL.EnableVertexAttribArray(7);
                GL.EnableVertexAttribArray(8);
                GL.EnableVertexAttribArray(9);

                // https://stackoverflow.com/a/28597384
                //  _vertexDataLength * sizeof(float) = 4 bytes * 16 numbers =  64
                GL.VertexAttribPointer(5, sizeof(float), VertexAttribPointerType.Float, false,
                    bufferData._instancedVertexDataSizeInBytes,
                    0);
                GL.VertexAttribPointer(6, sizeof(float), VertexAttribPointerType.Float, false,
                    bufferData._instancedVertexDataSizeInBytes,
                    1 * 3 * sizeof(float));
                GL.VertexAttribPointer(7, sizeof(float), VertexAttribPointerType.Float, false,
                    bufferData._instancedVertexDataSizeInBytes,
                    2 * 3 * sizeof(float));
                GL.VertexAttribPointer(8, sizeof(float), VertexAttribPointerType.Float, false,
                    bufferData._instancedVertexDataSizeInBytes,
                    3 * 3 * sizeof(float));
                GL.VertexAttribPointer(9, sizeof(float), VertexAttribPointerType.Float, false,
                    bufferData._instancedVertexDataSizeInBytes,
                    4 * 3 * sizeof(float));


                GL.VertexAttribDivisor(5, 1);
                GL.VertexAttribDivisor(6, 1);
                GL.VertexAttribDivisor(7, 1);
                GL.VertexAttribDivisor(8, 1);
                GL.VertexAttribDivisor(9, 1);
            }
        }
        else
        {
            GL.BufferSubData(BufferTarget.ArrayBuffer, 0,
                bufferData._instancedVertexDataSizeInBytes * bufferData.MaxNumberOfObjects,
                bufferData.Buffer);
            // GL.BufferData(BufferTarget.ArrayBuffer, _vertexDataSizeInBytes * bufferData.MaxNumberOfObjects, bufferData.Buffer, BufferUsageHint.StaticDraw);
        }
    }
}