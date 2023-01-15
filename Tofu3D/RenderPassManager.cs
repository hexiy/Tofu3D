namespace Tofu3D;

public static class RenderPassManager
{
	static List<IRenderPassPre> _renderPassPre = new List<IRenderPassPre>();
	static List<IRenderPassDepth> _renderPassDepth = new List<IRenderPassDepth>();
	static List<IRenderPassOpaques> _renderPassOpaques = new List<IRenderPassOpaques>();
	static List<IRenderPassPost> _renderPassPost = new List<IRenderPassPost>();
	public static RenderPassType CurrentRenderPassType = RenderPassType.Pre;

	public static void RegisterRenderPassEvent(IRenderPass renderPass)
	{
		{
			if (renderPass is IRenderPassPre rp)
			{
				_renderPassPre.Add(rp);
			}
		}
		{
			if (renderPass is IRenderPassDepth rp)
			{
				_renderPassDepth.Add(rp);
			}
		}
		{
			if (renderPass is IRenderPassOpaques rp)
			{
				_renderPassOpaques.Add(rp);
			}
		}

		{
			if (renderPass is IRenderPassPost rp)
			{
				_renderPassPost.Add(rp);
			}
		}
	}

	public static void RenderPassPre()
	{
		CurrentRenderPassType = RenderPassType.Pre;
		for (int i = 0; i < _renderPassPre.Count; i++)
		{
			_renderPassPre[i].RenderPassPre();
		}
	}

	public static void RenderPassDepth()
	{
		CurrentRenderPassType = RenderPassType.Depth;
		for (int i = 0; i < _renderPassDepth.Count; i++)
		{
			_renderPassDepth[i].RenderPassDepth();
		}
	}

	public static void RenderPassOpaques()
	{
		CurrentRenderPassType = RenderPassType.Opaques;
		for (int i = 0; i < _renderPassOpaques.Count; i++)
		{
			_renderPassOpaques[i].RenderPassOpaques();
		}
	}

	public static void RenderPassPost()
	{
		CurrentRenderPassType = RenderPassType.Post;
		for (int i = 0; i < _renderPassPost.Count; i++)
		{
			_renderPassPost[i].RenderPassPost();
		}
	}
}