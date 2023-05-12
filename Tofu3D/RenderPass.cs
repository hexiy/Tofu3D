namespace Tofu3D.Rendering;

public abstract class RenderPass : IComparable<RenderPass>
{
	public RenderPassType RenderPassType { get; private set; }
	List<Action> _renderQueue = new List<Action>();
	public RenderTexture PassRenderTexture { get; protected set; }

	public bool Enabled = true;
	public virtual bool CanRender()
	{
		return _renderQueue.Count > 0 && Enabled;
	}

	protected RenderPass(RenderPassType type)
	{
		RenderPassType = type;
		RenderPassSystem.RegisterRenderPass(this);
	}

	public virtual void Initialize()
	{
	}

	public virtual void Clear()
	{
		if (CanRender() == false)
		{
			return;
		}
		PassRenderTexture.Clear();
	}

	public int CompareTo(RenderPass comparePart)
	{
		if (comparePart == null)
		{
			return 1;
		}

		return RenderPassType.CompareTo(comparePart.RenderPassType);
	}

	public void RegisterRender(Action render)
	{
		_renderQueue.Add(render);
	}

	public void RemoveRender(Action render)
	{
		_renderQueue.Remove(render);
	}

	public void Render()
	{
		if (CanRender() == false)
		{
			return;
		}

		PreRender();
		foreach (Action renderCall in _renderQueue)
		{
			renderCall.Invoke();
		}

		PostRender();
	}

	public virtual void RenderToFramebuffer(RenderTexture target, FramebufferAttachment attachment)
	{
		if (PassRenderTexture == null)
		{
			Debug.Log($"PassRenderTexture == null");
			return;
		}

		// GL.ClearColor(Color.Orchid.ToOtherColor());
		// GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

		target.Bind();
		// GL.Viewport(0, 0, (int) target.Size.X, (int) target.Size.Y);
		// GL.Viewport(0, 0, (int) target.Size.X*2, (int) target.Size.Y*2);
		// wtf, why does the viewport need to be target.Size.X * 2 ??????
		// its 1380,
		if (attachment == FramebufferAttachment.Color)
		{
			target.RenderColorAttachment(PassRenderTexture.ColorAttachment);
		}

		if (attachment == FramebufferAttachment.Depth)
		{
			//target.RenderDepthAttachment(PassRenderTexture.DepthAttachment);
		}

		target.Unbind();
	}

	protected virtual void PreRender()
	{
		PassRenderTexture?.Bind();

		if (PassRenderTexture != null)
		{
			GL.Viewport(0, 0, (int) PassRenderTexture.Size.X, (int) PassRenderTexture.Size.Y);
		}
	}

	protected virtual void PostRender()
	{
		PassRenderTexture?.Unbind();
	}

	protected virtual void SetupRenderTexture()
	{
	}
}