namespace TofuEngine.Rendering;

public abstract class RenderPass : IComparable<RenderPass>
{
    public virtual BlendMode BlendMode { get; } = BlendMode.Fade;
    public bool Enabled = true;

    protected RenderPass(RenderPassType type)
    {
        RenderPassType = type;
    }

    public RenderPassType RenderPassType { get; }
    public Framebuffer MainFramebuffer { get; protected set; }
    public abstract bool DrawsToTheFinalColorFramebuffer { get; }

    public int CompareTo(RenderPass comparePart)
    {
        if (comparePart == null)
        {
            return 1;
        }

        return RenderPassType.CompareTo(comparePart.RenderPassType);
    }

    public virtual bool CanRender() => Enabled;

    public virtual void Initialize()
    {
    }

    public virtual void Clear()
    {
        if (CanRender() == false)
        {
            return;
        }

        MainFramebuffer.Clear();
    }

    public void RenderToFramebuffer()
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
        Render_GL();

        PostRender();
        UnbindFrameBuffer();
        PostUnbindFrameBuffer();
    }

    public virtual void RenderThisAsFullscreenQuadToTargetFramebuffer(Framebuffer target,
        FramebufferAttachment attachment)
    {
        if (MainFramebuffer == null)
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
        if (attachment == FramebufferAttachment.Color && MainFramebuffer.TextureId != -1)
        {
            target.RenderColorAttachmentToThis(MainFramebuffer.TextureId, this.BlendMode);
        }

        if (attachment == FramebufferAttachment.Depth && target.DepthTextureId != -1 &&
            MainFramebuffer.DepthTextureId != -1)
        {
            target.RenderDepthAttachmentToThis(MainFramebuffer.DepthTextureId);
        }

        target.Unbind();
    }

    internal void BindFrameBuffer()
    {
        MainFramebuffer?.Bind();

        if (MainFramebuffer != null)
        {
            GL.Viewport(0, 0, (int)MainFramebuffer.Size.X, (int)MainFramebuffer.Size.Y);
        }
    }

    internal void UnbindFrameBuffer()
    {
        MainFramebuffer?.Unbind();
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

    protected abstract void Render_GL();
    protected abstract void SetupRenderTexture();
}