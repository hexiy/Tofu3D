namespace TofuEngine.Rendering;

public class RenderTargetPipeline
{
    private bool _initialized;

    public List<RenderPass> RenderPasses { get; } = new List<RenderPass>();

    public RenderPassType CurrentRenderPassType { get; private set; } = RenderPassType.DirectionalLightShadowDepth;

    public Framebuffer FinalFramebuffer { get; private set; }

    public Vector2 ViewSize { get; private set; } = new Vector2(100, 100);
    public bool CanRender => Camera.GameViewCamera?.IsActive == true && _initialized;
    public Camera Camera;

    public void Initialize()
    {
        CreatePasses();
        RebuildRenderTextures(ViewSize);
        Scene.SceneLoaded += SetupCamera;
    }

    private void SetupCamera()
    {
        GameObject camGo = GameObject.Create(name: "RenderTargetPipeline Camera", visibleInHierarchy: false,
            runtimeOnly: true);

        Camera = camGo.AddComponent<Camera>();
        Camera.CameraSizeChanged += RebuildRenderTextures;
        camGo.AddComponent<Skybox>();
        camGo.Awake();
    }

    public void RebuildRenderTextures(Vector2 viewSize)
    {
        ViewSize = viewSize;
        FinalFramebuffer = new Framebuffer(ViewSize, true);

        foreach (RenderPass renderPass in RenderPasses)
        {
            renderPass.Initialize();
        }

        _initialized = true;
    }

    private void CreatePasses()
    {
        // GL.Disable(EnableCap.FramebufferSrgb);
        RenderPassSkybox renderPassSkybox = new RenderPassSkybox();
        RenderPassDirectionalLightShadowDepth renderPassDirectionalLightShadowDepth =
            new RenderPassDirectionalLightShadowDepth();
        RenderPassPointLightShadowDepth renderPassPointLightShadowDepth = new RenderPassPointLightShadowDepth();
        RenderPassZPrePass renderPassZPrePass = new RenderPassZPrePass();
        RenderPassOpaques renderPassOpaques = new RenderPassOpaques();
        // mouse picking for now must come before transparency pass for it to work

        RenderPassTransparency renderPassTransparency = new RenderPassTransparency();
        RenderPassMousePicking renderPassMousePicking = new RenderPassMousePicking();


        RenderPasses.AddRange([
            renderPassSkybox,
            renderPassDirectionalLightShadowDepth,
            renderPassPointLightShadowDepth,
            renderPassPointLightShadowDepth,
            renderPassZPrePass,
            renderPassOpaques,
            renderPassTransparency,
            renderPassMousePicking
        ]);
        // RenderPassBloomThreshold renderPassBloomThreshold = new();
        // RenderPassBloomPostProcess renderPassBloomPostProcess = new(renderPassBloomThreshold);
        // RenderPassPostProcess renderPassPostProcess = new();
        // RenderPassUI renderPassUI = new();


        // RenderPassTransparency renderPassTransparency = new RenderPassTransparency();


        // renderPassSkybox.Enabled = true;
        // renderPassDirectionalLightShadowDepth.Enabled = false;
        // renderPassPointLightShadowDepth.Enabled = false;
        // renderPassZPrePass.Enabled = false;
        // renderPassOpaques.Enabled = true;
        // renderPassTransparency.Enabled = false;
        // renderPassMousePicking.Enabled = false;
    }

    // public void RegisterRenderPass(RenderPass renderPass)
    // {
    //     RenderPasses.Add(renderPass);
    //     // _renderPasses.Sort();
    // }

    public void RenderAllPasses()
    {
        if (CanRender == false)
        {
            return;
        }

        // GL.Enable(EnableCap.Blend);
        GL.Enable(EnableCap.DepthTest);


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


            renderPass.RenderToFramebuffer();
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

        foreach (RenderPass renderPass in RenderPasses)
        {
            if (renderPass.DrawsToTheFinalColorFramebuffer == false)
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