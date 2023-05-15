namespace Tofu3D.Rendering;

public static class RenderPassSystem
{
	// we need to first render the whole scene for directional light
	// next pass-opaques we now have a shadowmap so we drav the scene normally
	// post process pass to post process scene
	// next pass-ui pass

	// should be reorderable and being able to add new pass easily...
	// every pass will have its own texture
	// in editor we will be able to visualise all the passes
	public static List<RenderPass> RenderPasses { get; private set; } = new List<RenderPass>();

	public static RenderPassType CurrentRenderPassType { get; private set; } = RenderPassType.DirectionalLightShadowDepth;
	public static RenderTexture FinalRenderTexture /*
	{
		get { return _renderPasses[^1].PassRenderTexture; }
	} //*/ { get; private set; } //= new RenderTexture(new Vector2(100, 100), true, false);
	private static bool _initialized;
	public static Vector2 ViewSize { get; private set; } = new Vector2(100, 100);
	public static bool CanRender
	{
		get { return Camera.MainCamera?.IsActive == true && _initialized; }
	}

	public static void Initialize()
	{
		CreatePasses();
		RebuildRenderTextures(ViewSize);
		Camera.CameraSizeChanged += RebuildRenderTextures;
	}

	public static void RebuildRenderTextures(Vector2 viewSize)
	{
		ViewSize = viewSize;
		FinalRenderTexture = new RenderTexture(ViewSize, colorAttachment: true, depthAttachment: false);

		foreach (RenderPass renderPass in RenderPasses)
		{
			renderPass.Initialize();
		}

		_initialized = true;
	}

	private static void CreatePasses()
	{
		RenderPassSkybox renderPassSkybox = new RenderPassSkybox();
		RenderPassDirectionalLightShadowDepth renderPassDirectionalLightShadowDepth = new RenderPassDirectionalLightShadowDepth();
		RenderPassZPrePass renderPassZPrePass = new RenderPassZPrePass();
		RenderPassOpaques renderPassOpaques = new RenderPassOpaques();
		RenderPassPostProcess renderPassPostProcess = new RenderPassPostProcess();
		RenderPassUI renderPassUI = new RenderPassUI();


		// RenderPassTransparency renderPassTransparency = new RenderPassTransparency();
		// RenderPassMousePicking renderPassMousePicking = new RenderPassMousePicking();
	}

	public static void RegisterRenderPass(RenderPass renderPass)
	{
		RenderPasses.Add(renderPass);
		// _renderPasses.Sort();
	}

	public static void RemoveRender(RenderPassType type, Action render)
	{
		foreach (RenderPass renderPass in RenderPasses)
		{
			if (renderPass.RenderPassType == type)
			{
				renderPass.RemoveRender(render);
				return;
			}
		}
	}

	public static void RegisterRender(RenderPassType type, Action render)
	{
		foreach (RenderPass renderPass in RenderPasses)
		{
			if (renderPass.RenderPassType == type)
			{
				renderPass.RegisterRender(render);
				return;
			}
		}
	}

	public static void RenderAllPasses()
	{
		if (CanRender == false)
		{
			return;
		}

		// GL.Enable(EnableCap.Blend);

		foreach (RenderPass renderPass in RenderPasses)
		{
			if (renderPass.CanRender() == false)
			{
				continue;
			}

			renderPass.Clear();
		}

		foreach (RenderPass renderPass in RenderPasses)
		{
			if (renderPass.CanRender() == false)
			{
				continue;
			}

			CurrentRenderPassType = renderPass.RenderPassType;


			renderPass.Render();
		}

		RenderFinalRenderTexture();
	}

	private static void RenderFinalRenderTexture()
	{
		FinalRenderTexture.Clear();
		
		if (CanRender == false)
		{
			return;
		}

		foreach (RenderPass renderPass in RenderPasses)
		{
			if (renderPass.RenderPassType == RenderPassType.DirectionalLightShadowDepth)
			{
				continue;
			}

			if (renderPass.CanRender() == false)
			{
				continue;
			}

			renderPass.RenderToRenderTexture(FinalRenderTexture, FramebufferAttachment.Color);
		}
	}
}