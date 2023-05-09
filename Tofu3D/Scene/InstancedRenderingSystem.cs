using Tofu3D.Rendering;

namespace Tofu3D;

public class InstancedRenderingSystem
{
	Dictionary<InstancedRenderingObjectDefinition, InstancedRenderingObjectBufferData> _objectBufferDatas = new Dictionary<InstancedRenderingObjectDefinition, InstancedRenderingObjectBufferData>();
	readonly int _vertexDataLength = 16; // 4x4 matrix

	public InstancedRenderingSystem()
	{
	}

	public void ClearBuffer()
	{
		_objectBufferDatas = new Dictionary<InstancedRenderingObjectDefinition, InstancedRenderingObjectBufferData>();
	}
	public void RenderInstances()
	{
		foreach (KeyValuePair<InstancedRenderingObjectDefinition, InstancedRenderingObjectBufferData> objectDefinitionBufferPair in _objectBufferDatas)
		{
			RenderSpecific(objectDefinitionBufferPair);
		}
	}

	public Matrix4x4 ModelViewProjectionMatrix
	{
		get { return ModelMatrix * Camera.MainCamera.ViewMatrix * Camera.MainCamera.ProjectionMatrix; }
	}

	public Matrix4x4 ModelMatrix
	{
		get
		{
			Matrix4x4 translation = Matrix4x4.CreateTranslation(Vector3.Zero);
			return ScalePivotRotationMatrix * translation;
		}
	}

	private Matrix4x4 ScalePivotRotationMatrix
	{
		get
		{
			Matrix4x4 scale = Matrix4x4.CreateScale(Vector3.One);
			return scale * IdentityPivotRotationMatrix;
		}
	}
	private Matrix4x4 IdentityPivotRotationMatrix
	{
		get
		{
			Vector3 worldPositionPivotOffset = Vector3.One * (Vector3.One - new Vector3(0.5f, 0.5f, 0.5f) * 2);

			Matrix4x4 pivot = Matrix4x4.CreateTranslation(worldPositionPivotOffset);

			return Matrix4x4.Identity * pivot;
		}
	}

	private void RenderSpecific(KeyValuePair<InstancedRenderingObjectDefinition, InstancedRenderingObjectBufferData> objectBufferPair)
	{
		Material material = objectBufferPair.Key.Material;
		Model model = objectBufferPair.Key.Model;
		if (RenderPassSystem.CurrentRenderPassType is RenderPassType.Opaques or RenderPassType.UI)
		{
			ShaderCache.UseShader(material.Shader);


			// material.Shader.SetMatrix4X4("u_mvp", ModelViewProjectionMatrix);


			// material.Shader.SetMatrix4X4("u_model", ModelMatrix);
			material.Shader.SetColor("u_rendererColor", Color.White);
			// material.Shader.SetVector2("u_tiling", Tiling);
			// material.Shader.SetVector2("u_offset", Offset);

			// GL.ActiveTexture(TextureUnit.Texture0);
			// TextureHelper.BindTexture(Texture.TextureId);
			// if (RenderPassDirectionalLightShadowDepth.I?.DepthMapRenderTexture != null)
			// {
			// 	GL.ActiveTexture(TextureUnit.Texture1);
			// 	TextureHelper.BindTexture(RenderPassDirectionalLightShadowDepth.I.DepthMapRenderTexture.ColorAttachment);
			// }


			ShaderCache.BindVertexArray(model.Vao);

			UploadBufferData(objectBufferPair.Value);

			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

			GL.ActiveTexture(TextureUnit.Texture0);
			TextureHelper.BindTexture(AssetManager.Load<Texture>("Assets/2D/solidColor.png").TextureId);

			GL_DrawArraysInstanced(PrimitiveType.Triangles, 0, model.IndicesCount, instanceCount: objectBufferPair.Value.Count);
		}
	}

	void GL_DrawArraysInstanced(PrimitiveType primitiveType, int first, int count, int instanceCount)
	{
		GL.DrawArraysInstanced(primitiveType, first, count, instanceCount);
		DebugHelper.LogDrawCall();
		Debug.StatAddValue("Instanced objects count", instanceCount);
	}

	public void UpdateObjectData(Model model, Material material, Renderer renderer)
	{
		InstancedRenderingObjectDefinition definition = new InstancedRenderingObjectDefinition(model, material);
		InstancedRenderingObjectBufferData bufferData;
		if (_objectBufferDatas.ContainsKey(definition) == false)
		{
			// no buffer exists for this combination-create one
			bufferData = CreateBufferData(definition);
			_objectBufferDatas.Add(definition, bufferData);
		}
		else
		{
			bufferData = _objectBufferDatas[definition];
		}

		if (renderer.InstancedRenderingIndex == -1)
		{
			// assign new InstancedRenderingIndex
			renderer.InstancedRenderingIndex = bufferData.Count;
			bufferData.Count++;
		}

		int startingIndexForRenderer = _vertexDataLength * renderer.InstancedRenderingIndex;

		CopyMatrixToBuffer(renderer.LatestModelViewProjection, ref bufferData.Buffer, startingIndexForRenderer);

		_objectBufferDatas[definition] = bufferData;
	}

	void CopyMatrixToBuffer(Matrix4x4 m, ref float[] buffer, int startingIndex)
	{
		buffer[startingIndex + 0] = m.M11;
		buffer[startingIndex + 1] = m.M12;
		buffer[startingIndex + 2] = m.M13;
		buffer[startingIndex + 3] = m.M14;
		buffer[startingIndex + 4] = m.M21;
		buffer[startingIndex + 5] = m.M22;
		buffer[startingIndex + 6] = m.M23;
		buffer[startingIndex + 7] = m.M24;
		buffer[startingIndex + 8] = m.M31;
		buffer[startingIndex + 9] = m.M32;
		buffer[startingIndex + 10] = m.M33;
		buffer[startingIndex + 11] = m.M34;
		buffer[startingIndex + 12] = m.M41;
		buffer[startingIndex + 13] = m.M42;
		buffer[startingIndex + 14] = m.M43;
		buffer[startingIndex + 15] = m.M44;
	}

	private InstancedRenderingObjectBufferData CreateBufferData(InstancedRenderingObjectDefinition objectDefinition)
	{
		Debug.Log("Creating Instanced Buffer Data");
		GL.BindVertexArray(objectDefinition.Model.Vao);

		InstancedRenderingObjectBufferData bufferData = new InstancedRenderingObjectBufferData();
		bufferData.MaxNumberOfObjects = 50000;
		bufferData.Vbo = -1;

		bufferData.Buffer = new float[bufferData.MaxNumberOfObjects * _vertexDataLength];

		UploadBufferData(bufferData);

		return bufferData;
	}

	private void UploadBufferData(InstancedRenderingObjectBufferData bufferData)
	{
		if (bufferData.Vbo == -1)
		{
			bufferData.Vbo = GL.GenBuffer();
		}
		GL.BindBuffer(BufferTarget.ArrayBuffer, bufferData.Vbo);
		GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * _vertexDataLength * bufferData.MaxNumberOfObjects, bufferData.Buffer, BufferUsageHint.StaticDraw);


		GL.EnableVertexAttribArray(3);
		GL.EnableVertexAttribArray(4);
		GL.EnableVertexAttribArray(5);
		GL.EnableVertexAttribArray(6);
		// GL.BindBuffer(BufferTarget.ArrayBuffer, bufferData.Vbo);


		// https://stackoverflow.com/a/28597384
		//  _vertexDataLength * sizeof(float) = 4 bytes * 16 numbers =  64
		GL.VertexAttribPointer(3, size: sizeof(float), VertexAttribPointerType.Float, false, _vertexDataLength * sizeof(float), 0);
		GL.VertexAttribPointer(4, size: sizeof(float), VertexAttribPointerType.Float, false, _vertexDataLength * sizeof(float), 1 * 4 * sizeof(float));
		GL.VertexAttribPointer(5, size: sizeof(float), VertexAttribPointerType.Float, false, _vertexDataLength * sizeof(float), 2 * 4 * sizeof(float));
		GL.VertexAttribPointer(6, size: sizeof(float), VertexAttribPointerType.Float, false, _vertexDataLength * sizeof(float), 3 * 4 * sizeof(float));


		GL.VertexAttribDivisor(3, divisor: 1);
		GL.VertexAttribDivisor(4, divisor: 1);
		GL.VertexAttribDivisor(5, divisor: 1);
		GL.VertexAttribDivisor(6, divisor: 1);

		// GL.BindVertexArray(0);
		// GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
	}
}