using Tofu3D.Components.Renderers;

public class ModelRenderer : TextureRenderer
{
	public Model Model;
	public bool CastShadow = true;

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

		bool renderDepth = RenderPassSystem.CurrentRenderPassType == RenderPassType.DirectionalLightShadowDepth && GameObject.Silent == false && CastShadow;
		/*if (renderDepth && false)
		{
			// depth pass


			Material depthMaterial = MaterialCache.GetMaterial("DepthModel");
			ShaderCache.UseShader(depthMaterial.Shader);

			depthMaterial.Shader.SetMatrix4X4("u_mvp", LatestModelViewProjection);
			depthMaterial.Shader.SetMatrix4X4("u_model", GetModelMatrixForLight());
			depthMaterial.Shader.SetMatrix4X4("u_lightSpaceMatrix", DirectionalLight.LightSpaceMatrix);

			ShaderCache.BindVertexArray(depthMaterial.Vao);

			GL.DrawArrays(PrimitiveType.Triangles, 0, 6 * 2 * 3);
		}
		else if (RenderPassSystem.CurrentRenderPassType == RenderPassType.Opaques || true)*/
		if (RenderPassSystem.CurrentRenderPassType == RenderPassType.Opaques || (RenderPassSystem.CurrentRenderPassType == RenderPassType.DirectionalLightShadowDepth && renderDepth))
		{
			{
				//GL.Enable(EnableCap.DepthTest);

				ShaderCache.UseShader(Material.Shader);
				Material.Shader.SetMatrix4X4("u_lightSpaceMatrix", DirectionalLight.LightSpaceMatrix);

				Material.Shader.SetMatrix4X4("u_mvp", LatestModelViewProjection);
				Material.Shader.SetMatrix4X4("u_model", GetModelMatrixForLight());
				Material.Shader.SetColor("u_rendererColor", Color);


				Material.Shader.SetVector3("u_lightPos", LightManager.I.GetDirectionalLightPosition());

				Material.Shader.SetVector3("u_ambientLightsColor", LightManager.I.GetAmbientLightsColor().ToVector3());
				Material.Shader.SetFloat("u_ambientLightsIntensity", LightManager.I.GetAmbientLightsIntensity());

				Material.Shader.SetVector3("u_directionalLightColor", LightManager.I.GetDirectionalLightColor().ToVector3());
				Material.Shader.SetFloat("u_directionalLightIntensity", LightManager.I.GetDirectionalLightIntensity());

				Vector3 adjustedLightDirection = Transform.RotateVectorByRotation(LightManager.I.GetDirectionalLightDirection(), -Transform.Rotation);
				// we can compute light direction 2 in relation to our rotation so we dont have to rotate normals in shader 
				Material.Shader.SetVector3("u_directionalLightDirection", adjustedLightDirection);

				ShaderCache.BindVertexArray(Material.Vao);


				TextureCache.BindTexture(Texture.Id);
				if (RenderPassDirectionalLightShadowDepth.I != null)
				{
					TextureCache.BindTexture(RenderPassDirectionalLightShadowDepth.I.DepthMapRenderTexture.ColorAttachment);
				}
				// TextureCache.BindTexture(DirectionalLight.DisplayDepthRenderTexture.ColorAttachment);

				//TextureCache.BindTexture(DirectionalLight.DepthRenderTexture.DepthAttachment);

				GL.DrawArrays(PrimitiveType.Triangles, 0, 6 * 2 * 3);
			}
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
}