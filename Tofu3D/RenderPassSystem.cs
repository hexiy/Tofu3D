namespace Tofu3D.Rendering;

public class RenderPassSystem
{
    private bool _initialized;
    // we need to first render the whole scene for directional light
    // next pass-opaques we now have a shadowmap so we drav the scene normally
    // post process pass to post process scene
    // next pass-ui pass

    // should be reorderable and being able to add new pass easily...
    // every pass will have its own texture
    // in editor we will be able to visualise all the passes
    public List<RenderPass> RenderPasses { get; } = new();

    public RenderPassType CurrentRenderPassType { get; private set; } = RenderPassType.DirectionalLightShadowDepth;

    public Framebuffer FinalFramebuffer /*
    {
        get { return _renderPasses[^1].PassRenderTexture; }
    } //*/ { get; private set; } //= new RenderTexture(new Vector2(100, 100), true, false);

    public Vector2 ViewSize { get; private set; } = new(100, 100);
    public bool CanRender => Camera.MainCamera?.IsActive == true && _initialized;

    public void Initialize()
    {
        CreatePasses();
        RebuildRenderTextures(ViewSize);
        Camera.CameraSizeChanged += RebuildRenderTextures;
    }

    public void RebuildRenderTextures(Vector2 viewSize)
    {
        ViewSize = viewSize;
        FinalFramebuffer = new Framebuffer(ViewSize, true);

        foreach (var renderPass in RenderPasses)
        {
            renderPass.Initialize();
        }

        _initialized = true;
    }

    private void CreatePasses()
    {
        // GL.Disable(EnableCap.FramebufferSrgb);
        RenderPassSkybox renderPassSkybox = new();
        RenderPassDirectionalLightShadowDepth renderPassDirectionalLightShadowDepth = new();
        RenderPassZPrePass renderPassZPrePass = new();
        RenderPassOpaques renderPassOpaques = new();
        RenderPassBloomThreshold renderPassBloomThreshold = new();
        RenderPassBloomPostProcess renderPassBloomPostProcess = new(renderPassBloomThreshold);
        // RenderPassPostProcess renderPassPostProcess = new();
        // RenderPassUI renderPassUI = new();


        // RenderPassTransparency renderPassTransparency = new RenderPassTransparency();
        // RenderPassMousePicking renderPassMousePicking = new RenderPassMousePicking();
    }

    public void RegisterRenderPass(RenderPass renderPass)
    {
        RenderPasses.Add(renderPass);
        // _renderPasses.Sort();
    }

    public void RemoveRender(RenderPassType type, Action render)
    {
        foreach (var renderPass in RenderPasses)
        {
            if (renderPass.RenderPassType == type)
            {
                renderPass.RemoveRender(render);
                return;
            }
        }
    }

    public void RegisterRender(RenderPassType type, Action render)
    {
        foreach (var renderPass in RenderPasses)
        {
            if (renderPass.RenderPassType == type)
            {
                renderPass.RegisterRender(render);
                return;
            }
        }
    }

    public void RenderAllPasses()
    {
        if (CanRender == false)
        {
            return;
        }

        // GL.Enable(EnableCap.Blend);

        foreach (var renderPass in RenderPasses)
        {
            if (renderPass.CanRender() == false)
            {
                continue;
            }

            renderPass.Clear();
        }

        foreach (var renderPass in RenderPasses)
        {
            if (renderPass.CanRender() == false)
            {
                continue;
            }

            CurrentRenderPassType = renderPass.RenderPassType;


            renderPass.RenderThisAsFullscreenQuadToTargetFramebuffer();
        }

        RenderFinalRenderTexture();
    }

    private void RenderFinalRenderTexture()
    {
        FinalFramebuffer.Clear();

        if (CanRender == false)
        {
            return;
        }

        foreach (var renderPass in RenderPasses)
        {
            if (renderPass.RenderPassType == RenderPassType.DirectionalLightShadowDepth)
            {
                continue;
            }

            if (renderPass.CanRender() == false)
            {
                continue;
            }

            renderPass.RenderThisAsFullscreenQuadToTargetFramebuffer(FinalFramebuffer, FramebufferAttachment.Color);
        }

    }
}