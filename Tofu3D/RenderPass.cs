namespace Tofu3D.Rendering;

public abstract class RenderPass : IComparable<RenderPass>
{
	public RenderPassType RenderPassType { get; private set; }
	// List<Action> _renderQueue = new List<Action>();
	Action _renderAction;
	public RenderTexture PassRenderTexture { get; protected set; }

	public bool Enabled = true;

	public virtual bool CanRender()
	{
		// return _renderQueue.Count > 0 && Enabled;
		return _renderAction != null && Enabled;
	}

	protected RenderPass(RenderPassType type)
	{
		RenderPassType = type;
		Tofu.I.RenderPassSystem.RegisterRenderPass(this);
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
		// _renderQueue.Clear();
		// _renderQueue.Add(render);
		_renderAction = render;
	}

	public void RemoveRender(Action render)
	{
		// _renderQueue.Remove(render);
		_renderAction = null;
	}

	public void Render()
	{
		// if (CanRender() == false)
		// {
		// 	return;
		// }
		PreBindFrameBuffer();
		BindFrameBuffer();
		PreRender();
		// foreach (Action renderCall in _renderQueue)
		// {
		// renderCall.Invoke();
		// }
		_renderAction?.Invoke(); // post process doesnt have render action so ?. 

		PostRender();
		UnbindFrameBuffer();
		PostUnbindFrameBuffer();
	}

	public virtual void RenderToRenderTexture(RenderTexture target, FramebufferAttachment attachment)
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
		if (attachment == FramebufferAttachment.Color && PassRenderTexture.ColorAttachment != -1)
		{
			target.RenderColorAttachment(PassRenderTexture.ColorAttachment);
		}

		if (attachment == FramebufferAttachment.Depth && target.DepthAttachment != -1 && PassRenderTexture.DepthAttachment != -1)
		{
			target.RenderDepthAttachment(PassRenderTexture.DepthAttachment);
		}

		target.Unbind();
	}

	internal void BindFrameBuffer()
	{
		PassRenderTexture?.Bind();

		if (PassRenderTexture != null)
		{
			GL.Viewport(0, 0, (int) PassRenderTexture.Size.X, (int) PassRenderTexture.Size.Y);
		}
	}

	internal void UnbindFrameBuffer()
	{
		PassRenderTexture?.Unbind();
	}

	protected virtual void PreBindFrameBuffer()
	{
	}

	protected virtual void PostUnbindFrameBuffer()
	{
	}

	protected virtual void PreRender()
	{
	}

	protected virtual void PostRender()
	{
	}

	protected abstract void SetupRenderTexture();
}