namespace Tofu3D;

public static class RenderPassSystem
{
	// we need to first render the whole scene for directional light
	// next pass-opaques we now have a shadowmap so we drav the scene normally
	// post process pass to post process scene
	// next pass-ui pass

	// should be reorderable and being able to add new pass easily...
	// every pass will have its own texture
	// in editor we will be able to visualise all the passes
	static List<RenderPass> _renderPasses = new List<RenderPass>();
	public static RenderPassType CurrentRenderPassType { get; private set; } = RenderPassType.DirectionalLightShadowDepth;
	public static RenderTexture FinalRenderTexture { get; private set; } //= new RenderTexture(new Vector2(100, 100), true, false);
	public static bool Initialized;

	public static void Initialize()
	{
		if (Camera.I == null)
		{
			throw new NullReferenceException("No camera in scene");
		}

		FinalRenderTexture = new RenderTexture(Camera.I.Size, true, false);

		foreach (RenderPass renderPass in _renderPasses)
		{
			renderPass.Initialize();
		}

		Initialized = true;
	}

	private static void CreatePasses()
	{
		RenderPassDirectionalLightShadowDepth renderPassDirectionalLightShadowDepth = new RenderPassDirectionalLightShadowDepth();
		RenderPassOpaques renderPassOpaques = new RenderPassOpaques();
	}

	public static void RegisterRenderPass(RenderPass renderPass)
	{
		_renderPasses.Add(renderPass);
		_renderPasses.Sort();
	}

	public static void RegisterRender(RenderPassType type, Action render)
	{
		if (_renderPasses.Count == 0)
		{
			CreatePasses();
		}

		foreach (RenderPass renderPass in _renderPasses)
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
		foreach (RenderPass renderPass in _renderPasses)
		{
			CurrentRenderPassType = renderPass.RenderPassType;
			
			
			renderPass.Render();
		}

		RenderFinalRenderTexture();
	}

	private static void RenderFinalRenderTexture()
	{
		FinalRenderTexture.Clear();
		if (Initialized == false)
		{
			return;
		}

		// RenderPassDirectionalLightShadowDepth.I.RenderToFramebuffer(FinalRenderTexture, FramebufferAttachment.Depth);
		RenderPassOpaques.I.RenderToFramebuffer(FinalRenderTexture, FramebufferAttachment.Color);
		// foreach (RenderPass renderPass in _renderPasses)
		// {
		// 	if (renderPass.RenderPassType == RenderPassType.DirectionalLightShadowDepth)
		// 	{
		// 		continue;
		// 	}
		// 	
		// 	renderPass.RenderToFramebuffer(FinalRenderTexture, FramebufferAttachment.Color);
		// }
	}
}