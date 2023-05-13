﻿using Tofu3D.Rendering;

namespace Tofu3D;

public class InstancedRenderingSystem
{
	// index in _definitions
	Dictionary<int, InstancedRenderingObjectBufferData> _objectBufferDatas = new Dictionary<int, InstancedRenderingObjectBufferData>();
	List<InstancedRenderingObjectDefinition> _definitions = new List<InstancedRenderingObjectDefinition>();
	readonly int _vertexDataLength = (sizeof(float) * 4 * 3) + (sizeof(float) * 4); // 4xvec 3's for matrix+vec4 color

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
		GL.Enable(EnableCap.DepthTest);

		foreach (KeyValuePair<int, InstancedRenderingObjectBufferData> objectDefinitionBufferPair in _objectBufferDatas)
		{
			if (objectDefinitionBufferPair.Value.NumberOfObjects == 0)
			{
				continue;
			}

			RenderSpecific(objectDefinitionBufferPair);
		}
	}

	private void RemoveObjectFromBuffer(InstancedRenderingObjectBufferData bufferData, ModelRendererInstanced renderer)
	{
		for (int i = 0; i < _vertexDataLength; i++)
		{
			bufferData.Buffer[renderer.InstancedRenderingDefinitionIndex + i] = -1;
		}

		bufferData.EmptyStartIndexes.Add(renderer.InstancedRenderingStartingIndexInBuffer);

		renderer.InstancedRenderingStartingIndexInBuffer = -1;
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
			return bufferData.NumberOfObjects * _vertexDataLength;
		}
	}

	private void RenderSpecific(KeyValuePair<int, InstancedRenderingObjectBufferData> objectBufferPair)
	{
		int definitionIndex = objectBufferPair.Key;
		InstancedRenderingObjectDefinition definition = _definitions[definitionIndex];
		Material material = definition.Material;
		material = AssetManager.Load<Material>(material.AssetPath);
		Model model = definition.Model;
		if (RenderPassSystem.CurrentRenderPassType is RenderPassType.Opaques or RenderPassType.UI)
		{
			ShaderManager.UseShader(material.Shader);


			material.Shader.SetFloat("u_renderMode", (int) RenderSettings.CurrentRenderModeSettings.CurrentRenderMode);


			material.Shader.SetMatrix4X4("u_viewProjection", Camera.MainCamera.ViewMatrix * Camera.MainCamera.ProjectionMatrix);

			material.Shader.SetColor("u_rendererColor", Color.White);
			// material.Shader.SetVector2("u_tiling", new Vector2(-1, -1)); //grass block
			material.Shader.SetVector2("u_tiling", material.Tiling); // normal 
			material.Shader.SetVector2("u_offset", material.Offset);

			material.Shader.SetVector3("u_camPos", Camera.MainCamera.Transform.WorldPosition);

			// LIGHTING
			material.Shader.SetMatrix4X4("u_lightSpaceMatrix", DirectionalLight.LightSpaceMatrix);


			Vector4 ambientColor = SceneLightingManager.I.GetAmbientLightsColor().ToVector4();
			ambientColor = new Vector4(ambientColor.X, ambientColor.Y, ambientColor.Z, SceneLightingManager.I.GetAmbientLightsIntensity());
			material.Shader.SetVector4("u_ambientLightColor", ambientColor);

			Vector4 directionalLightColor = SceneLightingManager.I.GetDirectionalLightColor().ToVector4();
			directionalLightColor = new Vector4(directionalLightColor.X, directionalLightColor.Y, directionalLightColor.Z, SceneLightingManager.I.GetDirectionalLightIntensity());
			material.Shader.SetVector4("u_directionalLightColor", directionalLightColor);
			material.Shader.SetVector3("u_directionalLightDirection", SceneLightingManager.I.GetDirectionalLightDirection());


			material.Shader.SetFloat("u_specularSmoothness", 0f);
			material.Shader.SetFloat("u_specularHighlightsEnabled", 0);

			//FOG
			bool fogEnabled = SceneManager.CurrentScene.SceneFogManager.FogEnabled;
			material.Shader.SetFloat("u_fogEnabled", fogEnabled ? 1 : 0);
			if (fogEnabled)
			{
				material.Shader.SetColor("u_fogColor", SceneManager.CurrentScene.SceneFogManager.FogColor1);
				material.Shader.SetFloat("u_fogIntensity", SceneManager.CurrentScene.SceneFogManager.Intensity);
				if (SceneManager.CurrentScene.SceneFogManager.IsGradient)
				{
					material.Shader.SetColor("u_fogColor2", SceneManager.CurrentScene.SceneFogManager.FogColor2);
					material.Shader.SetFloat("u_fogGradientSmoothness", SceneManager.CurrentScene.SceneFogManager.GradientSmoothness);
				}
				else
				{
					material.Shader.SetColor("u_fogColor2", SceneManager.CurrentScene.SceneFogManager.FogColor1);
				}

				material.Shader.SetFloat("u_fogStartDistance", SceneManager.CurrentScene.SceneFogManager.FogStartDistance);
				material.Shader.SetFloat("u_fogEndDistance", SceneManager.CurrentScene.SceneFogManager.FogEndDistance);
				material.Shader.SetFloat("u_fogPositionY", SceneManager.CurrentScene.SceneFogManager.FogPositionY);
			}

			// material.Shader.SetFloat("u_aoStrength", _normalDisabled ? 0 : 1);

			// ALBEDO
			// TextureHelper.BindTexture(AssetManager.Load<Texture>("Assets/2D/solidColor.png").TextureId);
			// TextureHelper.BindTexture(AssetManager.Load<Texture>("Assets/3D/Grass_Block_TEX.png").TextureId);
			if (material.AlbedoTexture)
			{
				GL.ActiveTexture(TextureUnit.Texture0);
				TextureHelper.BindTexture(material.AlbedoTexture.TextureId);
			}

			// NORMAL
			// TextureHelper.BindTexture(AssetManager.Load<Texture>("Assets/2D/solidColor.png").TextureId);
			// GL.ActiveTexture(TextureUnit.Texture1);
			// TextureHelper.BindTexture(_idNormal);

			// AO
			// TextureHelper.BindTexture(AssetManager.Load<Texture>("Assets/2D/solidColor.png").TextureId);
			if (material.AoTexture)
			{
				GL.ActiveTexture(TextureUnit.Texture1);
				TextureHelper.BindTexture(material.AoTexture.TextureId);
			}

			ShaderManager.BindVertexArray(model.Vao);

			if (objectBufferPair.Value.Dirty)
			{
				UploadBufferData(objectBufferPair.Value);
				objectBufferPair.Value.Dirty = false;
			}

			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);


			GL_DrawArraysInstanced(PrimitiveType.Triangles, 0, model.IndicesCount, instanceCount: objectBufferPair.Value.NumberOfObjects);

			GL.ActiveTexture(TextureUnit.Texture0); // DOESNT WORK
		}
	}

	void GL_DrawArraysInstanced(PrimitiveType primitiveType, int first, int count, int instanceCount)
	{
		GL.DrawArraysInstanced(primitiveType, first, count, instanceCount);
		DebugHelper.LogDrawCall();
		Debug.StatAddValue("Instanced objects count", instanceCount);
	}

	public void UpdateObjectData(ModelRendererInstanced renderer, bool remove = false)
	{
		InstancedRenderingObjectBufferData bufferData;
		Material material = renderer.Material;
		Model model = renderer.Model;
		if (renderer.InstancedRenderingDefinitionIndex == -1)
		{
			// no buffer exists for this combination-create one
			InstancedRenderingObjectDefinition definition = new InstancedRenderingObjectDefinition(model, material, renderer.GameObject.IsStaticSelf);
			int definitionIndex = _definitions.Contains(definition) ? _definitions.IndexOf(definition) : _definitions.Count;

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

			renderer.InstancedRenderingDefinitionIndex = definitionIndex;
		}
		else
		{
			if (_objectBufferDatas.ContainsKey(renderer.InstancedRenderingDefinitionIndex) == false)
			{
				// on scene reload the definitionIndex is 0 but its not created in the system...
				renderer.InstancedRenderingDefinitionIndex = -1;
				return;
			}

			bufferData = _objectBufferDatas[renderer.InstancedRenderingDefinitionIndex];
		}


		if (renderer.InstancedRenderingStartingIndexInBuffer == -1 && remove == false)
		{
			// assign new InstancedRenderingIndex
			renderer.InstancedRenderingStartingIndexInBuffer = GetEmptyIndexInBuffer(bufferData);
			bufferData.NumberOfObjects++;
		}

		bufferData.Dirty = true;

		if (renderer.InstancedRenderingStartingIndexInBuffer != -1 && remove == true)
		{
			RemoveObjectFromBuffer(bufferData, renderer);
		}
		else
		{
			CopyObjectDataToBuffer(renderer, ref bufferData.Buffer, renderer.InstancedRenderingStartingIndexInBuffer);
		}

		_objectBufferDatas[renderer.InstancedRenderingDefinitionIndex] = bufferData;
	}

	void CopyObjectDataToBuffer(Renderer renderer, ref float[] buffer, int startingIndex)
	{
		if (startingIndex == -1)
		{
			return;
		}

		if (startingIndex >= buffer.Length)
		{
			// needs to resize buffer or smtn
			return;
		}

		Matrix4x4 m = renderer.GetModelMatrix();

		buffer[startingIndex + 0] = m.M11;
		buffer[startingIndex + 1] = m.M12;
		buffer[startingIndex + 2] = m.M13;
		// buffer[startingIndex + 3] = 0;//
		buffer[startingIndex + 3] = m.M21;
		buffer[startingIndex + 4] = m.M22;
		buffer[startingIndex + 5] = m.M23;
		// buffer[startingIndex + 7] = 0;//
		buffer[startingIndex + 6] = m.M31;
		buffer[startingIndex + 7] = m.M32;
		buffer[startingIndex + 8] = m.M33;
		// buffer[startingIndex + 11] = 0;//
		buffer[startingIndex + 9] = m.M41;
		buffer[startingIndex + 10] = m.M42;
		buffer[startingIndex + 11] = m.M43;
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
		GL.BindVertexArray(objectDefinition.Model.Vao);

		InstancedRenderingObjectBufferData bufferData = new InstancedRenderingObjectBufferData
		                                                {
			                                                MaxNumberOfObjects = 50000,
			                                                Vbo = -1
		                                                };

		bufferData.Buffer = new float[bufferData.MaxNumberOfObjects * _vertexDataLength];
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
			GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * _vertexDataLength * bufferData.MaxNumberOfObjects, bufferData.Buffer, BufferUsageHint.StaticDraw);

			GL.EnableVertexAttribArray(3);
			GL.EnableVertexAttribArray(4);
			GL.EnableVertexAttribArray(5);
			GL.EnableVertexAttribArray(6);
			GL.EnableVertexAttribArray(7);

			// https://stackoverflow.com/a/28597384
			//  _vertexDataLength * sizeof(float) = 4 bytes * 16 numbers =  64
			GL.VertexAttribPointer(3, size: sizeof(float), VertexAttribPointerType.Float, false, _vertexDataLength * sizeof(float), 0);
			GL.VertexAttribPointer(4, size: sizeof(float), VertexAttribPointerType.Float, false, _vertexDataLength * sizeof(float), 1 * 3 * sizeof(float));
			GL.VertexAttribPointer(5, size: sizeof(float), VertexAttribPointerType.Float, false, _vertexDataLength * sizeof(float), 2 * 3 * sizeof(float));
			GL.VertexAttribPointer(6, size: sizeof(float), VertexAttribPointerType.Float, false, _vertexDataLength * sizeof(float), 3 * 3 * sizeof(float));
			GL.VertexAttribPointer(7, size: sizeof(float), VertexAttribPointerType.Float, false, _vertexDataLength * sizeof(float), 4 * 3 * sizeof(float));


			GL.VertexAttribDivisor(3, divisor: 1);
			GL.VertexAttribDivisor(4, divisor: 1);
			GL.VertexAttribDivisor(5, divisor: 1);
			GL.VertexAttribDivisor(6, divisor: 1);
			GL.VertexAttribDivisor(7, divisor: 1);
		}
		else
		{
			GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float) * _vertexDataLength * bufferData.NumberOfObjects, bufferData.Buffer);
		}
	}
}