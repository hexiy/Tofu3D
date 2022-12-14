using Tofu3D.Components.Renderers;

public class ModelRenderer : TextureRenderer
{
	public Model Model;

	public override void Awake()
	{
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

	public override void CreateMaterial()
	{
		if (Material == null)
		{
			Material = MaterialCache.GetMaterial("ModelRenderer");
		}

		base.CreateMaterial();
	}

	public override void OnDestroyed()
	{
		//BatchingManager.RemoveAttribs(texture.id, gameObjectID);
		base.OnDestroyed();
	}

	public override void Render()
	{
		if (OnScreen == false)
		{
			return;
		}

		if (BoxShape == null)
		{
			return;
		}

		if (Texture.Loaded == false)
		{
			return;
		}

		bool drawOutline = GameObject.Selected && false;
		if (drawOutline)
		{
			{
				Material material = MaterialCache.GetMaterial("ModelUnlit");
				ShaderCache.UseShader(material.Shader);

				material.Shader.SetMatrix4X4("u_mvp", GetMvpForOutline());
				material.Shader.SetMatrix4X4("u_model", GetModelMatrix());
				material.Shader.SetColor("u_rendererColor", new Vector4(1, 1, 1, 0.8f));

				ShaderCache.BindVertexArray(material.Vao);

				//GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
				// TextureCache.BindTexture(Texture.Id);

				GL.DrawArrays(PrimitiveType.Triangles, 0, 6 * 2 * 3);
				// GL.DrawElements(PrimitiveType.Triangles, 6 * 2 * 3, DrawElementsType.UnsignedInt, (IntPtr) null);
			}


			//GL.BindVertexArray(0);
		}


		{
			//GL.Enable(EnableCap.DepthTest);

			ShaderCache.UseShader(Material.Shader);

			Material.Shader.SetMatrix4X4("u_mvp", LatestModelViewProjection);
			Material.Shader.SetMatrix4X4("u_model", GetModelMatrixForLight());
			Material.Shader.SetColor("u_rendererColor", Color);

			Material.Shader.SetVector3("u_ambientLightsColor", LightManager.I.GetAmbientLightsColor().ToVector3());
			Material.Shader.SetFloat("u_ambientLightsIntensity", LightManager.I.GetAmbientLightsIntensity());

			Material.Shader.SetVector3("u_directionalLightColor", LightManager.I.GetDirectionalLightColor().ToVector3());
			Material.Shader.SetFloat("u_directionalLightIntensity", LightManager.I.GetDirectionalLightIntensity());

			Vector3 adjustedLightDirection = Transform.RotateVectorByRotation(LightManager.I.GetDirectionalLightDirection(), -Transform.Rotation);
			// we can compute light direction 2 in relation to our rotation so we dont have to rotate normals in shader 
			Material.Shader.SetVector3("u_directionalLightDirection", adjustedLightDirection);
			
			
			// Material.Shader.SetVector3Array("u_pointLightLocations", LightManager.I.GetPointLightsPositions());
			// Material.Shader.SetVector3Array("u_pointLightColors", LightManager.I.GetPointLightsColors());
			// Material.Shader.SetFloatArray("u_pointLightIntensities", LightManager.I.GetPointLightsIntensities());
			//material.shader.SetMatrix4x4("u_model", GetModelMatrix());


			ShaderCache.BindVertexArray(Material.Vao);
			//GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			//GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			// TextureCache.BindTexture(Texture.Id);

			GL.DrawArrays(PrimitiveType.Triangles, 0, 6 * 2 * 3);
			//GL.DrawElements(PrimitiveType.Triangles, 6 * 2 * 3, DrawElementsType.UnsignedInt, (IntPtr) null);
		}

		if (drawOutline)
		{
			GL.BindVertexArray(0);
		}

		Debug.CountStat("Draw Calls", 1);
		if (drawOutline)
		{
			Debug.CountStat("Draw Calls", 1);
		}
	}
	// public override void Render()
	// {
	// 	if (onScreen == false)
	// 	{
	// 		return;
	// 	}
	//
	// 	if (boxShape == null)
	// 	{
	// 		return;
	// 	}
	//
	// 	if (texture.loaded == false)
	// 	{
	// 		return;
	// 	}
	//
	// 	transform.Rotation += Vector3.One * Time.deltaTime * 60;
	// 	ShaderCache.UseShader(material.shader);
	// 	material.shader.SetMatrix4x4("u_mvp", LatestModelViewProjection);
	// 	material.shader.SetColor("u_rendererColor", color);
	// 	//material.shader.SetMatrix4x4("u_model", GetModelMatrix());
	// 	if (material.additive)
	// 	{
	// 		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusConstantColor);
	// 	}
	// 	else
	// 	{
	// 		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
	// 	}
	//
	// 	ShaderCache.BindVAO(material.vao);
	//
	// 	//GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
	// 	TextureCache.BindTexture(texture.id);
	//
	// 	GL.DrawElements(PrimitiveType.Triangles, 6 * 2 * 3, DrawElementsType.UnsignedInt, (IntPtr) null);
	//
	// 	Debug.CountStat("Draw Calls", 1);
	// }
}

// STENCIL working

/*
public override void Render()
		{
			if (boxShape == null) return;
			if (texture.loaded == false) return;
			// stencil experiment
			GL.Enable(EnableCap.StencilTest);
			GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
			GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
			GL.StencilMask(0xFF);
			shader.Use();
			shader.SetMatrix4x4("u_mvp", GetModelViewProjection());
			shader.SetVector4("u_color", color.ToVector4());

			GL.BindVertexArray(vao);
			GL.Enable(EnableCap.Blend);

			if (additive)
			{
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusConstantColor);
			}
			else
			{
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			}
			texture.Use();
			GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
			// stencil after
			GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
			GL.StencilMask(0x00);
			GL.Disable(EnableCap.DepthTest);

			shader.Use();
			shader.SetMatrix4x4("u_mvp", GetModelViewProjectionForOutline(thickness));
			//shader.SetVector4("u_color", new Vector4(MathF.Abs(MathF.Sin(Time.elapsedTime * 0.3f)), MathF.Abs(MathF.Cos(Time.elapsedTime * 0.3f)), 1, 1));
			shader.SetVector4("u_color", Color.Black.ToVector4());

			texture.Use();
			GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

			GL.StencilMask(0xFF);
			GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
			GL.Enable(EnableCap.DepthTest);

			GL.BindVertexArray(0);
			GL.Disable(EnableCap.Blend);
		}
*/