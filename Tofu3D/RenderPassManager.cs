namespace Tofu3D;

public static class RenderPassManager
{
	static List<IRenderPassDepth> _renderPassDepth = new List<IRenderPassDepth>();
	static List<IRenderPassOpaques> _renderPassOpaques = new List<IRenderPassOpaques>();
	static List<IRenderPassUI> _renderPassUI = new List<IRenderPassUI>();
	public static RenderPassType CurrentRenderPassType = RenderPassType.Depth;

	public static void ResetRenderPassEvents()
	{
		_renderPassDepth.Clear();
		_renderPassOpaques.Clear();
		_renderPassUI.Clear();
	}

	public static void RegisterRenderPassEvent(IRenderPass renderPass)
	{
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
			if (renderPass is IRenderPassUI rp)
			{
				_renderPassUI.Add(rp);
			}
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

	public static void RenderPassUI()
	{
		CurrentRenderPassType = RenderPassType.UI;
		for (int i = 0; i < _renderPassUI.Count; i++)
		{
			_renderPassUI[i].RenderPassUI();
		}
	}
}