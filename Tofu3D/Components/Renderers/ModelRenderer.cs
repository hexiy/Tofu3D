using Tofu3D.Rendering;

public class ModelRenderer : TextureRenderer
{
	public Model Model;
	public bool CastShadow = true;
	// Color _mousePickingColor;

	public override void Awake()
	{
		// _mousePickingColor = MousePickingSystem.RegisterObject(this);

		if (Texture == null)
		{
			Texture = new Texture();
		}
		else
		{
			LoadTexture(Texture.AssetPath);
		}

		base.Awake();
	}

	public override void SetDefaultMaterial()
	{
		if (Material?.AssetPath.Length == 0 || Material == null)
		{
			Material = AssetManager.Load<Material>("ModelRenderer");
		}
		else
		{
			Material = AssetManager.Load<Material>(Material.AssetPath);
		}

		if (Model)
		{
			Model = AssetManager.Load<Model>(Model.AssetPath);
		}
		// Material = AssetManager.Load<Material>("ModelRenderer");

		// base.CreateMaterial();
	}

	public override void OnDestroyed()
	{
		//BatchingManager.RemoveAttribs(texture.id, gameObjectID);
		base.OnDestroyed();
	}

	public override void Update()
	{
		if (GameObject == TransformHandle.I.GameObject)
		{
			return;
		}

		// if (RenderMode == RenderMode.Opaque && Material.RenderMode != RenderMode)
		// {
		// 	Material = AssetManager.Load<Material>("ModelRenderer");
		// }
		//
		// if (RenderMode == RenderMode.Transparent && Material.RenderMode != RenderMode)
		// {
		// 	Material = AssetManager.Load<Material>("ModelRendererUnlit");
		// }

		base.Update();
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

		bool isTransformHandle = GameObject == TransformHandle.I.GameObject;
		if (isTransformHandle && RenderPassSystem.CurrentRenderPassType != RenderPassType.Opaques)
		{
			return;
		}

		if (CastShadow == false && RenderPassSystem.CurrentRenderPassType == RenderPassType.DirectionalLightShadowDepth)
		{
			return;
		}


		bool drawOutline = GameObject.Selected && false;
		if (drawOutline)
		{
			{
				Material material = AssetManager.Load<Material>("ModelUnlit");
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


		if (GameObject == TransformHandle.I?.GameObject)
		{
			GL.Disable(EnableCap.DepthTest);
		}
		else
		{
			GL.Enable(EnableCap.DepthTest);
		}


		//GL.Enable(EnableCap.DepthTest);
		if (RenderPassSystem.CurrentRenderPassType == RenderPassType.DirectionalLightShadowDepth)
		{
			Material depthMaterial = AssetManager.Load<Material>("DepthModel");
			ShaderCache.UseShader(depthMaterial.Shader);
			Material.Shader.SetMatrix4X4("u_mvp", LatestModelViewProjection);
		}

		if (RenderPassSystem.CurrentRenderPassType == RenderPassType.Opaques)
		{
			ShaderCache.UseShader(Material.Shader);
			Material.Shader.SetMatrix4X4("u_lightSpaceMatrix", DirectionalLight.LightSpaceMatrix);
			Material.Shader.SetMatrix4X4("u_mvp", LatestModelViewProjection);
			Material.Shader.SetMatrix4X4("u_model", GetModelMatrix());
			Material.Shader.SetColor("u_rendererColor", Color);
			Material.Shader.SetVector2("u_tiling", Tiling);
			Material.Shader.SetVector2("u_offset", Offset);


			Material.Shader.SetVector3("u_lightPos", SceneLightingManager.I.GetDirectionalLightPosition());

			Material.Shader.SetVector3("u_ambientLightsColor", SceneLightingManager.I.GetAmbientLightsColor().ToVector3());
			Material.Shader.SetFloat("u_ambientLightsIntensity", SceneLightingManager.I.GetAmbientLightsIntensity());

			Material.Shader.SetVector3("u_directionalLightColor", SceneLightingManager.I.GetDirectionalLightColor().ToVector3());
			Material.Shader.SetFloat("u_directionalLightIntensity", SceneLightingManager.I.GetDirectionalLightIntensity());

			// Vector3 adjustedLightDirection = Transform.RotateVectorByRotation(SceneLightingManager.I.GetDirectionalLightDirection(), -Transform.Rotation);
			Vector3 adjustedLightDirection = SceneLightingManager.I.GetDirectionalLightDirection();
			// we can compute light direction 2 in relation to our rotation so we dont have to rotate normals in shader 
			Material.Shader.SetVector3("u_directionalLightDirection", adjustedLightDirection);


			GL.ActiveTexture(TextureUnit.Texture0);
			TextureHelper.BindTexture(Texture.TextureId);
			if (RenderPassDirectionalLightShadowDepth.I?.DepthMapRenderTexture != null)
			{
				GL.ActiveTexture(TextureUnit.Texture1);
				TextureHelper.BindTexture(RenderPassDirectionalLightShadowDepth.I.DepthMapRenderTexture.ColorAttachment);
			}

			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

			if (Model != null)
			{
				ShaderCache.BindVertexArray(Model.Vao);
				RenderWireframe(Model.VertexBufferDataLength);

				GL.DrawArrays(PrimitiveType.Triangles, 0, Model.VertexBufferDataLength);
			}
			else
			{
				ShaderCache.BindVertexArray(Material.Vao);
				GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
			}
		}
		else
		{
			if (Model != null)
			{
				ShaderCache.BindVertexArray(Model.Vao);

				GL.DrawArrays(PrimitiveType.Triangles, 0, Model.VertexBufferDataLength);
			}
			else
			{
				ShaderCache.BindVertexArray(Material.Vao);
				GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
			}
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

			Material mousePickingMaterial = AssetManager.Load<Material>("ModelMousePicking");
			ShaderCache.UseShader(mousePickingMaterial.Shader);

			mousePickingMaterial.Shader.SetMatrix4X4("u_mvp", LatestModelViewProjection);
			mousePickingMaterial.Shader.SetMatrix4X4("u_model", GetModelMatrixForLight());

			mousePickingMaterial.Shader.SetColor("u_rendererColor", _mousePickingColor);

			ShaderCache.BindVertexArray(Material.Vao);

			GL.DrawArrays(PrimitiveType.Triangles, 0, 6 * 2 * 3);
		}*/

		if (drawOutline)
		{
			GL.BindVertexArray(0);
		}

		DebugHelper.LogDrawCall();
		if (drawOutline)
		{
			DebugHelper.LogDrawCall();
		}
	}
}