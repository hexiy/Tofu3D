namespace Tofu3D.Rendering;

public abstract class RenderPass : IComparable<RenderPass>
{
    // List<Action> _renderQueue = new List<Action>();
    private Action _renderAction;

    public bool Enabled = true;

    protected RenderPass(RenderPassType type)
    {
        RenderPassType = type;
        Tofu.RenderPassSystem.RegisterRenderPass(this);
    }

    public RenderPassType RenderPassType { get; }
    public Framebuffer FinalFramebuffer { get; protected set; }

    public int CompareTo(RenderPass comparePart)
    {
        if (comparePart == null)
        {
            return 1;
        }

        return RenderPassType.CompareTo(comparePart.RenderPassType);
    }

    public virtual bool CanRender() =>
        // return _renderQueue.Count > 0 && Enabled;
        _renderAction != null && Enabled;

    public virtual void Initialize()
    {
    }

    public virtual void Clear()
    {
        if (CanRender() == false)
        {
            return;
        }

        FinalFramebuffer.Clear();
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

    public void RenderThisAsFullscreenQuadToTargetFramebuffer()
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

    public virtual void RenderThisAsFullscreenQuadToTargetFramebuffer(Framebuffer target, FramebufferAttachment attachment)
    {
        if (FinalFramebuffer == null)
        {
            Debug.Log("PassRenderTexture == null");
            return;
        }

        // GL.ClearColor(Color.Orchid.ToOtherColor());
        // GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        target.Bind();
        // GL.Viewport(0, 0, (int) target.Size.X, (int) target.Size.Y);
        // GL.Viewport(0, 0, (int) target.Size.X*2, (int) target.Size.Y*2);
        // wtf, why does the viewport need to be target.Size.X * 2 ??????
        // its 1380,
        if (attachment == FramebufferAttachment.Color && FinalFramebuffer.ColorAttachmentID != -1)
        {
            target.RenderColorAttachmentToThis(FinalFramebuffer.ColorAttachmentID);
        }

        if (attachment == FramebufferAttachment.Depth && target.DepthAttachmentID != -1 &&
            FinalFramebuffer.DepthAttachmentID != -1)
        {
            target.RenderDepthAttachmentToThis(FinalFramebuffer.DepthAttachmentID);
        }

        target.Unbind();
    }

    internal void BindFrameBuffer()
    {
        FinalFramebuffer?.Bind();

        if (FinalFramebuffer != null)
        {
            GL.Viewport(0, 0, (int)FinalFramebuffer.Size.X, (int)FinalFramebuffer.Size.Y);
        }
    }

    internal void UnbindFrameBuffer()
    {
        FinalFramebuffer?.Unbind();
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