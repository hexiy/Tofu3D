using Tofu3D.Rendering;

namespace Tofu3D;

public class InstancedRenderingSystem
{
	Dictionary<InstancedRenderingObjectDefinition, InstancedRenderingObjectBufferData> _objectBufferDatas = new Dictionary<InstancedRenderingObjectDefinition, InstancedRenderingObjectBufferData>();
	readonly int _vertexDataLength = 3; // for now only 3 translations

	public InstancedRenderingSystem()
	{
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


			material.Shader.SetMatrix4X4("u_mvp", ModelViewProjectionMatrix);


			material.Shader.SetMatrix4X4("u_model", ModelMatrix);
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
			
			UploadBufferData(objectBufferPair);
			
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
		bufferData.Buffer[startingIndexForRenderer] = renderer.Transform.WorldPosition.X;
		bufferData.Buffer[startingIndexForRenderer + 1] = renderer.Transform.WorldPosition.Y;
		bufferData.Buffer[startingIndexForRenderer + 2] = renderer.Transform.WorldPosition.Z;


		// not good
		_objectBufferDatas[definition] = bufferData;
	}

	private InstancedRenderingObjectBufferData CreateBufferData(InstancedRenderingObjectDefinition objectDefinition)
	{
		Debug.Log("Creating Instanced Buffer Data");
		GL.BindVertexArray(objectDefinition.Model.Vao);

		InstancedRenderingObjectBufferData bufferData = new InstancedRenderingObjectBufferData();
		bufferData.MaxNumberOfObjects = 500;

		bufferData.Buffer = new float[bufferData.MaxNumberOfObjects * _vertexDataLength];

		int instanceVbo = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, instanceVbo);
		GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * _vertexDataLength * bufferData.MaxNumberOfObjects, bufferData.Buffer, BufferUsageHint.StaticDraw);


		int translationsAttribIndex = 3;
		GL.EnableVertexAttribArray(translationsAttribIndex);
		GL.BindBuffer(BufferTarget.ArrayBuffer, instanceVbo);

		GL.VertexAttribPointer(translationsAttribIndex, size: _vertexDataLength, VertexAttribPointerType.Float, false, _vertexDataLength * sizeof(float), 0);
		GL.VertexAttribDivisor(translationsAttribIndex, divisor: 1);

		GL.BindVertexArray(0);
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

		ImGuiController.CheckGlError("Instanced createBufferData");
		return bufferData;
	}

	private void UploadBufferData(KeyValuePair<InstancedRenderingObjectDefinition, InstancedRenderingObjectBufferData> objectBufferPair)
	{
		InstancedRenderingObjectBufferData bufferData = objectBufferPair.Value;

		int instanceVbo = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, instanceVbo);
		GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * _vertexDataLength * bufferData.MaxNumberOfObjects, bufferData.Buffer, BufferUsageHint.StaticDraw);


		int translationsAttribIndex = 3;
		GL.EnableVertexAttribArray(translationsAttribIndex);
		GL.BindBuffer(BufferTarget.ArrayBuffer, instanceVbo);

		GL.VertexAttribPointer(translationsAttribIndex, size: _vertexDataLength, VertexAttribPointerType.Float, false, _vertexDataLength * sizeof(float), 0);
		GL.VertexAttribDivisor(translationsAttribIndex, divisor: 1);
	}
}