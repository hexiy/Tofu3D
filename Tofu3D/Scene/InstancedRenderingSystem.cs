﻿using Tofu3D.Rendering;

namespace Tofu3D;

public class InstancedRenderingSystem
{
    // index in _definitions
    Dictionary<int, InstancedRenderingObjectBufferData> _objectBufferDatas =
        new Dictionary<int, InstancedRenderingObjectBufferData>();

    List<InstancedRenderingObjectDefinition> _definitions = new List<InstancedRenderingObjectDefinition>();

    readonly int
        _vertexDataSizeInBytes = (sizeof(float) * 4 * 3) + (sizeof(float) * 4); // 4xvec 3's for matrix+vec4 color

    int VertexCountOfFloats
    {
        get { return _vertexDataSizeInBytes / sizeof(float); }
    }

    public InstancedRenderingSystem()
    {
    }

    public void ClearBuffers()
    {
        foreach (KeyValuePair<int, InstancedRenderingObjectBufferData> pair in _objectBufferDatas)
        {
            GL.DeleteBuffer(pair.Value.Vbo);
        }

        _objectBufferDatas = new Dictionary<int, InstancedRenderingObjectBufferData>();
        _definitions = new List<InstancedRenderingObjectDefinition>();
    }

    public void RenderInstances()
    {
        // GL.Enable(EnableCap.DepthTest);

        foreach (KeyValuePair<int, InstancedRenderingObjectBufferData> objectDefinitionBufferPair in _objectBufferDatas)
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
        for (int i = 0; i < VertexCountOfFloats; i++)
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
            int index = bufferData.EmptyStartIndexes[0];
            bufferData.EmptyStartIndexes.RemoveAt(0);
            return index;
        }
        else
        {
            if (bufferData.NumberOfObjects == bufferData.MaxNumberOfObjects)
            {
                bufferData.FutureMaxNumberOfObjects += 1;
                return -1;
            }

            if (bufferData.Buffer.Length < VertexCountOfFloats * bufferData.MaxNumberOfObjects)
            {
                return -1;
            }

            return bufferData.NumberOfObjects * VertexCountOfFloats;
        }
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

        Array.Resize(ref bufferData.Buffer, bufferData.MaxNumberOfObjects * VertexCountOfFloats);
        bufferData.Vbo = -1;
        bufferData.NeedsUpload = true;
    }

    private void RenderSpecific(KeyValuePair<int, InstancedRenderingObjectBufferData> objectBufferPair)
    {
        int definitionIndex = objectBufferPair.Key;
        InstancedRenderingObjectDefinition definition = _definitions[definitionIndex];
        Material material = definition.Material;
        material = Tofu.I.AssetManager.Load<Material>(material.Path);
        Mesh mesh = definition.Mesh;
        InstancedRenderingObjectBufferData bufferData = objectBufferPair.Value;
        // GL.Enable(EnableCap.DepthTest);
        if (Tofu.I.RenderPassSystem.CurrentRenderPassType is RenderPassType.DirectionalLightShadowDepth
            or RenderPassType.ZPrePass)
        {
            Material depthMaterial = Tofu.I.AssetManager.Load<Material>("ModelRendererInstancedDepth");
            Tofu.I.ShaderManager.UseShader(depthMaterial.Shader);
            depthMaterial.Shader.SetMatrix4X4("u_viewProjection",
                Camera.MainCamera.ViewMatrix * Camera.MainCamera.ProjectionMatrix);


            Tofu.I.ShaderManager.BindVertexArray(mesh.Vao);

            GL_DrawArraysInstanced(PrimitiveType.Triangles, 0, mesh.VerticesCount,
                instancesCount: objectBufferPair.Value.NumberOfObjects);
        }

        else if (Tofu.I.RenderPassSystem.CurrentRenderPassType is RenderPassType.Opaques or RenderPassType.UI)
        {
            Tofu.I.ShaderManager.UseShader(material.Shader);


            material.Shader.SetFloat("u_renderMode",
                (int)Tofu.I.RenderSettings.CurrentRenderModeSettings.CurrentRenderMode);

            material.Shader.SetMatrix4X4("u_viewProjection",
                Camera.MainCamera.ViewMatrix * Camera.MainCamera.ProjectionMatrix);

            material.Shader.SetColor("u_albedoTint", material.AlbedoTint);
            // material.Shader.SetVector2("u_tiling", new Vector2(-1, -1)); //grass block
            material.Shader.SetVector2("u_tiling", material.Tiling); // normal 
            material.Shader.SetVector2("u_offset", material.Offset);

            material.Shader.SetVector3("u_camPos", Camera.MainCamera.Transform.WorldPosition);

            // LIGHTING
            material.Shader.SetMatrix4X4("u_lightSpaceViewProjection", DirectionalLight.LightSpaceViewProjectionMatrix);


            Vector4 ambientColor = SceneLightingManager.I.GetAmbientLightsColor().ToVector4();
            ambientColor = new Vector4(ambientColor.X, ambientColor.Y, ambientColor.Z,
                SceneLightingManager.I.GetAmbientLightsIntensity());
            material.Shader.SetVector4("u_ambientLightColor", ambientColor);

            Vector4 directionalLightColor = SceneLightingManager.I.GetDirectionalLightColor().ToVector4();
            directionalLightColor = new Vector4(directionalLightColor.X, directionalLightColor.Y,
                directionalLightColor.Z, SceneLightingManager.I.GetDirectionalLightIntensity());
            material.Shader.SetVector4("u_directionalLightColor", directionalLightColor);
            material.Shader.SetVector3("u_directionalLightDirection",
                SceneLightingManager.I.GetDirectionalLightDirection());


            material.Shader.SetFloat("u_specularSmoothness", material.SpecularSmoothness);
            material.Shader.SetFloat("u_specularHighlightsEnabled", material.SpecularHighlightsEnabled ? 1 : 0);

            //FOG
            bool fogEnabled = Tofu.I.SceneManager.CurrentScene.SceneFogManager.FogEnabled;
            material.Shader.SetFloat("u_fogEnabled", fogEnabled ? 1 : 0);
            if (fogEnabled)
            {
                material.Shader.SetColor("u_fogColor", Tofu.I.SceneManager.CurrentScene.SceneFogManager.FogColor1);
                material.Shader.SetFloat("u_fogIntensity", Tofu.I.SceneManager.CurrentScene.SceneFogManager.Intensity);
                if (Tofu.I.SceneManager.CurrentScene.SceneFogManager.IsGradient)
                {
                    material.Shader.SetColor("u_fogColor2", Tofu.I.SceneManager.CurrentScene.SceneFogManager.FogColor2);
                    material.Shader.SetFloat("u_fogGradientSmoothness",
                        Tofu.I.SceneManager.CurrentScene.SceneFogManager.GradientSmoothness);
                }
                else
                {
                    material.Shader.SetColor("u_fogColor2", Tofu.I.SceneManager.CurrentScene.SceneFogManager.FogColor1);
                }

                material.Shader.SetFloat("u_fogStartDistance",
                    Tofu.I.SceneManager.CurrentScene.SceneFogManager.FogStartDistance);
                material.Shader.SetFloat("u_fogEndDistance",
                    Tofu.I.SceneManager.CurrentScene.SceneFogManager.FogEndDistance);
                material.Shader.SetFloat("u_fogPositionY",
                    Tofu.I.SceneManager.CurrentScene.SceneFogManager.FogPositionY);
            }

            // material.Shader.SetFloat("u_aoStrength", _normalDisabled ? 0 : 1);

            // ALBEDO
            if (material.AlbedoTexture)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                TextureHelper.BindTexture(material.AlbedoTexture.TextureId);
            }

            // NORMAL
            // TextureHelper.BindTexture(Tofu.I.AssetManager.Load<Texture>("Assets/2D/solidColor.png").TextureId);
            // GL.ActiveTexture(TextureUnit.Texture1);
            // TextureHelper.BindTexture(_idNormal);

            // AO
            if (material.AoTexture)
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                TextureHelper.BindTexture(material.AoTexture.TextureId);
            }

            if (RenderPassDirectionalLightShadowDepth.I?.PassRenderTexture != null)
            {
                GL.ActiveTexture(TextureUnit.Texture2);
                TextureHelper.BindTexture(RenderPassDirectionalLightShadowDepth.I.PassRenderTexture.DepthAttachment);
            }


            Tofu.I.ShaderManager.BindVertexArray(mesh.Vao);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);


            GL_DrawArraysInstanced(PrimitiveType.Triangles, 0, mesh.VerticesCount,
                instancesCount: objectBufferPair.Value.NumberOfObjects);

            // GL.ActiveTexture(TextureUnit.Texture0); // DOESNT WORK
        }


// resize the buffer if needed, after drawing the old one
        if (bufferData.Buffer.Length != VertexCountOfFloats * bufferData.FutureMaxNumberOfObjects)
        {
            ResizeBufferData(bufferData);
        }

        if (objectBufferPair.Value.NeedsUpload)
        {
            UploadBufferData(objectBufferPair.Value);
            if (Tofu.I.RenderPassSystem.CurrentRenderPassType == RenderPassType.Opaques)
            {
                // hmm i though this would fix the flicker
                objectBufferPair.Value.NeedsUpload = false;
            }
        }
    }

    void GL_DrawArraysInstanced(PrimitiveType primitiveType, int first, int verticesCount, int instancesCount)
    {
        GL.DrawArraysInstanced(primitiveType, first, verticesCount, instancesCount);
        DebugHelper.LogDrawCall();
        Debug.StatAddValue("Instanced objects drawn:", instancesCount);
        // Debug.StatAddValue("Total vertices:", verticesCount * instancesCount);
        DebugHelper.LogVerticesDrawCall(verticesCount: verticesCount * instancesCount);
    }

    public bool UpdateObjectData(Renderer renderer, ref RendererInstancingData instancingData,
        Matrix4x4? modelMatrix = null, bool isStatic = false, bool remove = false)
    {
        Mesh mesh = renderer.Mesh;
        Material material = renderer.Material;
        if (mesh == null)
        {
            return false;
        }

        InstancedRenderingObjectBufferData bufferData;
        if (instancingData.InstancedRenderingDefinitionIndex == -1)
        {
            // no buffer exists for this combination-create one
            InstancedRenderingObjectDefinition definition =
                new InstancedRenderingObjectDefinition(mesh, material, isStatic);
            int definitionIndex = _definitions.Contains(definition)
                ? _definitions.IndexOf(definition)
                : _definitions.Count;

            // find bufferData if its already created
            if (_objectBufferDatas.TryGetValue(definitionIndex, out InstancedRenderingObjectBufferData? data))
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

        if (instancingData.InstancedRenderingStartingIndexInBuffer != -1 && remove == true)
        {
            RemoveObjectFromBuffer(bufferData, instancingData);
        }
        else if (instancingData.InstancedRenderingStartingIndexInBuffer != -1)
        {
            CopyObjectDataToBuffer(renderer, modelMatrix ?? renderer.GetModelMatrix(), ref bufferData.Buffer,
                instancingData.InstancedRenderingStartingIndexInBuffer);
        }

        _objectBufferDatas[instancingData.InstancedRenderingDefinitionIndex] = bufferData;

        return true;
    }

    void CopyObjectDataToBuffer(Renderer renderer, Matrix4x4 modelMatrix, ref float[] buffer, int startingIndex)
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

        Color color = renderer.Color;

        buffer[startingIndex + 12] = color.R / 255f;
        buffer[startingIndex + 13] = color.G / 255f;
        buffer[startingIndex + 14] = color.B / 255f;
        buffer[startingIndex + 15] = color.A / 255f;
    }

    private InstancedRenderingObjectBufferData InitializeBufferData(InstancedRenderingObjectDefinition objectDefinition)
    {
        Debug.Log("Initializing Instanced Buffer Data");
        GL.BindVertexArray(objectDefinition.Mesh.Vao);

        InstancedRenderingObjectBufferData bufferData = new InstancedRenderingObjectBufferData
        {
            MaxNumberOfObjects = 1,
            FutureMaxNumberOfObjects = 1,
            Vbo = -1
        };

        bufferData.Buffer = new float[bufferData.MaxNumberOfObjects * VertexCountOfFloats];
        bufferData.EmptyStartIndexes = new List<int>();

        UploadBufferData(bufferData);

        return bufferData;
    }

    private void UploadBufferData(InstancedRenderingObjectBufferData bufferData)
    {
        bool newBuffer = false;
        if (bufferData.Vbo == -1)
        {
            newBuffer = true;
            bufferData.Vbo = GL.GenBuffer();
        }

        GL.BindBuffer(BufferTarget.ArrayBuffer, bufferData.Vbo);

        if (newBuffer)
        {
            GL.BufferData(BufferTarget.ArrayBuffer, _vertexDataSizeInBytes * bufferData.MaxNumberOfObjects,
                bufferData.Buffer, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(3);
            GL.EnableVertexAttribArray(4);
            GL.EnableVertexAttribArray(5);
            GL.EnableVertexAttribArray(6);
            GL.EnableVertexAttribArray(7);

            // https://stackoverflow.com/a/28597384
            //  _vertexDataLength * sizeof(float) = 4 bytes * 16 numbers =  64
            GL.VertexAttribPointer(3, size: sizeof(float), VertexAttribPointerType.Float, false, _vertexDataSizeInBytes,
                0);
            GL.VertexAttribPointer(4, size: sizeof(float), VertexAttribPointerType.Float, false, _vertexDataSizeInBytes,
                1 * 3 * sizeof(float));
            GL.VertexAttribPointer(5, size: sizeof(float), VertexAttribPointerType.Float, false, _vertexDataSizeInBytes,
                2 * 3 * sizeof(float));
            GL.VertexAttribPointer(6, size: sizeof(float), VertexAttribPointerType.Float, false, _vertexDataSizeInBytes,
                3 * 3 * sizeof(float));
            GL.VertexAttribPointer(7, size: sizeof(float), VertexAttribPointerType.Float, false, _vertexDataSizeInBytes,
                4 * 3 * sizeof(float));


            GL.VertexAttribDivisor(3, divisor: 1);
            GL.VertexAttribDivisor(4, divisor: 1);
            GL.VertexAttribDivisor(5, divisor: 1);
            GL.VertexAttribDivisor(6, divisor: 1);
            GL.VertexAttribDivisor(7, divisor: 1);
        }
        else
        {
            GL.BufferSubData(BufferTarget.ArrayBuffer, 0, _vertexDataSizeInBytes * bufferData.MaxNumberOfObjects,
                bufferData.Buffer);
            // GL.BufferData(BufferTarget.ArrayBuffer, _vertexDataSizeInBytes * bufferData.MaxNumberOfObjects, bufferData.Buffer, BufferUsageHint.StaticDraw);
        }
    }
}