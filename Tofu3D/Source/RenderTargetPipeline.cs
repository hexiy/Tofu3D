using Tofu3D;
using TofuEngine;

namespace TofuEngine.Rendering;

public class RenderTargetPipeline
{
    public EditorPanelGenericView EditorPanelView;
    private bool _initialized;
    public RenderTargetPipelineType ViewType { get; init; }
    public List<RenderPass> RenderPasses { get; } = new List<RenderPass>();

    public RenderPassType CurrentRenderPassType { get; private set; } = RenderPassType.DirectionalLightShadowDepth;

    public Framebuffer FinalFramebuffer { get; private set; }

    public Vector2 FramebufferSize { get; private set; } = new Vector2(100, 100);
    public bool CanRender => Camera.IsActive == true && _initialized;
    public Camera Camera;
    public RenderPass ZPrePass;
    public RenderSettings RenderSettings;

    private SceneViewData _sceneViewData;

    public RenderPassDirectionalLightShadowDepth? DirectionalLightShadowDepthRenderPass
    {
        get;
        private set;
    }
    
    public RenderTargetPipeline(EditorPanelGenericView editorPanelView,RenderTargetPipelineType type)
    {
        EditorPanelView = editorPanelView;
        ViewType = type;
        RenderSettings = new RenderSettings();
    }

    public void Initialize(int id, Vector2? viewSize = null)
    {
        FramebufferSize = viewSize ?? FramebufferSize;
        if (id != -1)
        {
            _sceneViewData = PersistentData.Get<SceneViewData>(key: $"SceneViewData_{id}", () => new SceneViewData());


            Tofu.Window.Closing += c => SaveSceneViewData(id);
            Scene.SceneStartedDisposing += () => SaveSceneViewData(id);
        }

        // RenderSettings.LoadSavedData();
        CreatePasses();
        RebuildRenderTextures(FramebufferSize);
        Scene.SceneLoaded += SetupCamera;
        if (Tofu.SceneManager.IsSceneLoaded)
        {
            SetupCamera();
        }
    }

    private void SaveSceneViewData(int id)
    {
        _sceneViewData.CameraPosition = Camera.Transform.WorldPosition;
        _sceneViewData.CameraRotation = Camera.Transform.WorldRotation;
        PersistentData.Set(key: $"SceneViewData_{id}", _sceneViewData);
    }

    private void SetupCamera()
    {
        if (ViewType == RenderTargetPipelineType.GameView)
        {
            Camera = Tofu.SceneManager.CurrentScene.FindComponent<Camera>();
            if (Camera != null)
            {
                Camera.CameraSizeChanged += RebuildRenderTextures;
                return;
            }
        }

        GameObject camGo = GameObject.Create(name: "RenderTargetPipeline Camera", visibleInHierarchy: false,
            runtimeOnly: true);

        Camera = camGo.AddComponent<Camera>();

        Camera.Transform.WorldPosition = _sceneViewData.CameraPosition;
        Camera.Transform.Rotation = _sceneViewData.CameraRotation;

        Camera.SceneViewCamera = Camera;
        Camera.AllCameras.Add(Camera);
        Camera.CameraSizeChanged += RebuildRenderTextures;
        camGo.AddComponent<Skybox>();
        camGo.Awake();
    }

    public void RebuildRenderTextures(Vector2 viewSize)
    {
        FramebufferSize = viewSize;
        FinalFramebuffer = new Framebuffer(FramebufferSize, true);

        foreach (RenderPass renderPass in RenderPasses)
        {
            renderPass.Initialize();
        }

        _initialized = true;
    }

    private void CreatePasses()
    {
        // GL.Disable(EnableCap.FramebufferSrgb);
        RenderPassSkybox renderPassSkybox = new RenderPassSkybox(this);
        RenderPassDirectionalLightShadowDepth renderPassDirectionalLightShadowDepth =
            new RenderPassDirectionalLightShadowDepth(this);

        DirectionalLightShadowDepthRenderPass = renderPassDirectionalLightShadowDepth;
        RenderPassPointLightShadowDepth renderPassPointLightShadowDepth = new RenderPassPointLightShadowDepth(this);
        RenderPassZPrePass renderPassZPrePass = new RenderPassZPrePass(this);
        ZPrePass = renderPassZPrePass;

        RenderPassOpaques renderPassOpaques = new RenderPassOpaques(this);
        // mouse picking for now must come before transparency pass for it to work

        RenderPassTransparency renderPassTransparency = new RenderPassTransparency(this);
        RenderPassMousePicking renderPassMousePicking = new RenderPassMousePicking(this);
        // renderPassMousePicking.Enabled = false;

        RenderPassUI renderPassUI = new RenderPassUI(this);


        RenderPasses.AddRange([
            renderPassSkybox,
            // renderPassDirectionalLightShadowDepth,
            // renderPassPointLightShadowDepth,
            // renderPassPointLightShadowDepth,
            renderPassZPrePass,
            renderPassOpaques,
            renderPassTransparency,
            renderPassMousePicking,
            renderPassUI
        ]);
        // RenderPassBloomThreshold renderPassBloomThreshold = new();
        // RenderPassBloomPostProcess renderPassBloomPostProcess = new(renderPassBloomThreshold);
        // RenderPassPostProcess renderPassPostProcess = new();


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

        Debug.StatAddValue("RenderTargetPipelines rendering:", 1);
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
        if (CanRender == false)
        {
            return;
        }

        FinalFramebuffer.Clear();

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
    public T? GetRenderPass<T>(int? index = null) where T : RenderPass
    {
        int k = index == null ? 0 : (int)index;
        for (int i = 0; i < RenderPasses.Count; i++)
        {
            if (RenderPasses[i] is T)
            {
                if (k == 0)
                {
                    return (T)RenderPasses[i];
                }

                k--;
            }
        }

        return null;
    }
    public T? GetRenderPass<T>(out T renderPass, int? index = null) where T : RenderPass
    {
        int k = index == null ? 0 : (int)index;
        for (int i = 0; i < RenderPasses.Count; i++)
        {
            if (RenderPasses[i] is T)
            {
                if (k == 0)
                {
                    renderPass = (T)RenderPasses[i];
                    return (T)RenderPasses[i];
                }

                k--;
            }
        }

        renderPass = null;
        return null;
    }
}