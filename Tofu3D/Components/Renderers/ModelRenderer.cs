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
		bool isTransformHandle = GameObject == TransformHandle.I.GameObject;
		if (isTransformHandle && (RenderPassSystem.CurrentRenderPassType != RenderPassType.Opaques && RenderPassSystem.CurrentRenderPassType != RenderPassType.UI))
		{
			return;
		}

		if (CastShadow == false && RenderPassSystem.CurrentRenderPassType == RenderPassType.DirectionalLightShadowDepth)
		{
			return;
		}

		if (Transform.IsInCanvas && RenderPassSystem.CurrentRenderPassType != RenderPassType.UI || Transform.IsInCanvas == false && RenderPassSystem.CurrentRenderPassType == RenderPassType.UI)
		{
			return;
		}


		if (GameObject == TransformHandle.I?.GameObject)
		{
			GL.Disable(EnableCap.DepthTest);
		}
		else
		{
			GL.Enable(EnableCap.DepthTest);
		}

		bool drawOutline = GameObject.Selected && false;

		//GL.Enable(EnableCap.DepthTest);
		if (RenderPassSystem.CurrentRenderPassType == RenderPassType.DirectionalLightShadowDepth)
		{
			Material depthMaterial = AssetManager.Load<Material>("DepthModel");
			ShaderCache.UseShader(depthMaterial.Shader);
			Material.Shader.SetMatrix4X4("u_mvp", LatestModelViewProjection);
		}

		if (RenderPassSystem.CurrentRenderPassType is RenderPassType.Opaques or RenderPassType.UI)
		{
			// GL.Enable(EnableCap.DepthTest);

			if (drawOutline)
			{
				GL.Enable(EnableCap.StencilTest);

				GL.Clear(ClearBufferMask.StencilBufferBit);
				GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
				GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
				GL.StencilMask(0xFF);
			}
			else
			{
				GL.Clear(ClearBufferMask.StencilBufferBit);

				GL.Disable(EnableCap.StencilTest);
			}

			ShaderCache.UseShader(Material.Shader);
			Material.Shader.SetMatrix4X4("u_lightSpaceMatrix", DirectionalLight.LightSpaceMatrix);

			if (Transform.IsInCanvas)
			{
				Material.Shader.SetMatrix4X4("u_mvp", GetModelMatrixForCanvasObject());
			}
			else
			{
				Material.Shader.SetMatrix4X4("u_mvp", LatestModelViewProjection);
			}

			Material.Shader.SetMatrix4X4("u_model", GetModelMatrix());
			Material.Shader.SetColor("u_rendererColor", Color);
			Material.Shader.SetVector2("u_tiling", Tiling);
			Material.Shader.SetVector2("u_offset", Offset);


			Material.Shader.SetVector3("u_lightPos", SceneLightingManager.I.GetDirectionalLightPosition() * Units.OneWorldUnit); // moves with camera but rotated wrong
			Material.Shader.SetVector3("u_camPos", Camera.MainCamera.Transform.WorldPosition * Units.OneWorldUnit);
			// Material.Shader.SetVector3("u_lightPos", SceneLightingManager.I.GetDirectionalLightPosition());
			// Material.Shader.SetVector3("u_camPos", Camera.MainCamera.Transform.WorldPosition);

			Material.Shader.SetVector3("u_ambientLightsColor", SceneLightingManager.I.GetAmbientLightsColor().ToVector3());
			Material.Shader.SetFloat("u_ambientLightsIntensity", SceneLightingManager.I.GetAmbientLightsIntensity());

			Material.Shader.SetVector3("u_directionalLightColor", SceneLightingManager.I.GetDirectionalLightColor().ToVector3());
			Material.Shader.SetFloat("u_directionalLightIntensity", SceneLightingManager.I.GetDirectionalLightIntensity());

			// Vector3 adjustedLightDirection = Transform.RotateVectorByRotation(SceneLightingManager.I.GetDirectionalLightDirection(), -Transform.Rotation);
			// Vector3 adjustedLightDirection = SceneLightingManager.I.GetDirectionalLightDirection();
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


			if (Model != null)
			{
				ShaderCache.BindVertexArray(Model.Vao);
				GL.Enable(EnableCap.Blend);
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

				if (isTransformHandle == false)
				{
					RenderWireframe(Model.VertexBufferDataLength);
				}

				GL.DrawArrays(PrimitiveType.Triangles, 0, Model.VertexBufferDataLength);
			}
			else
			{
				ShaderCache.BindVertexArray(Material.Vao);
				GL.Enable(EnableCap.Blend);
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
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


		if (drawOutline)
		{
			GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
			GL.StencilMask(0x00);
			GL.Disable(EnableCap.DepthTest);
			GL.Disable(EnableCap.Blend);

			Material outlineMaterial = AssetManager.Load<Material>("ModelRendererUnlit");
			ShaderCache.UseShader(outlineMaterial.Shader);

			if (Transform.IsInCanvas)
			{
				outlineMaterial.Shader.SetMatrix4X4("u_mvp", GetCanvasMvpForOutline());
			}
			else
			{
				outlineMaterial.Shader.SetMatrix4X4("u_mvp", GetMvpForOutline());
			}

			outlineMaterial.Shader.SetMatrix4X4("u_model", GetModelMatrix());
			outlineMaterial.Shader.SetColor("u_rendererColor", new Vector4(1, 1, 1, 1f));

			ShaderCache.BindVertexArray(Model.Vao);

			GL.DrawArrays(PrimitiveType.Triangles, 0, Model.VertexBufferDataLength);


			GL.StencilMask(0xFF);
			GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
			GL.Enable(EnableCap.DepthTest);

			// GL.BindVertexArray(0);
			// GL.Disable(EnableCap.Blend);
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
			// GL.BindVertexArray(0);
		}


		DebugHelper.LogDrawCall();
		if (drawOutline)
		{
			DebugHelper.LogDrawCall();
		}
	}
}