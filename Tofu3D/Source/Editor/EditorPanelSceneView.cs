using System.IO;
using System.Runtime.InteropServices;
using ImGuiNET;
using Tofu3D;
using TofuEngine.Rendering;

namespace TofuEngine;

public class EditorPanelSceneView : EditorPanel
{
    private static bool _renderCameraViews = true;

    private bool _renderModeWindowOpened;
    private bool _renderPassesWindowOpened;
    public override string Name => "Scene View";
    public static EditorPanelSceneView I { get; private set; }
    private RenderTargetPipeline _renderTargetPipeline;
    public Camera _camera => _renderTargetPipeline.Camera;


    public override void Draw()
    {
        // return;
        if (IsActive == false)
        {
            return;
        }

        if (Global.EditorAttached)
        {
            _renderCameraViews = true || /*Global.Debug &&*/
                                 Tofu.GameObjectSelectionManager.GetSelectedGameObject()
                                     ?.GetComponent<DirectionalLight>() !=
                                 null;

            // int tooltipsPanelHeight = 70;
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

            Tofu.Editor.SceneViewSize =
                _renderTargetPipeline.FinalFramebuffer.Size /
                Screen.Scale; // + new Vector2(0, tooltipsPanelHeight);

            ImGui.SetNextWindowSize(_renderTargetPipeline.FinalFramebuffer.Size,
                ImGuiCond.FirstUseEver);

            ImGui.SetNextWindowPos(new Vector2(0, 0), ImGuiCond.FirstUseEver, new Vector2(0, 0));
            ImGuiWindowFlags flags = Editor.ImGuiDefaultWindowFlags | ImGuiWindowFlags.NoScrollbar |
                                     ImGuiWindowFlags.NoScrollWithMouse;

            if (IsFullscreen)
            {
            }

            ImGui.Begin(Name, flags);

            if ((Vector2)ImGui.GetWindowSize() != _camera.Size)
            {
                _camera.SetSize(ImGui.GetWindowSize());
                // Debug.Log("SetSize");
            }


            // ImGui.SetCursorPosX(0);
            ImGui.SetCursorPos(new Vector2(0, 0));

            Tofu.Editor.SceneViewPosition = new Vector2(ImGui.GetCursorPosX(),
                ImGuiHelper.FlipYToGoodSpace(ImGui.GetCursorPosY()) -
                _renderTargetPipeline.FinalFramebuffer.Size.Y / Screen.Scale - 15);

            // Debug.StatSetValue("aaaa", $"scne size {Tofu.RenderPassSystem.FinalFramebuffer.Size.Y / Screen.Scale}");

            if (_renderTargetPipeline.CanRender)
            {
                TofuImGui.ImageTexture2D(_renderTargetPipeline.FinalFramebuffer.TextureId,
                    _renderTargetPipeline.FinalFramebuffer.Size,
                    new Vector4(0, 1, 1, 0));
            }
            else
            {
                ImGui.Dummy(_renderTargetPipeline.FinalFramebuffer.Size);
            }

            HandleModelDragDrop();


            Tofu.MouseInput.IsMouseInSceneView = ImGui.IsItemHovered();

            // ImGui.Image((IntPtr) RenderPassManager.FinalRenderTexture.ColorAttachment, RenderPassManager.FinalRenderTexture.Size * 0.9f,
            //             new Vector2(-0.5f, 0.5f), new Vector2(0.5f, -0.5f), Color.White.ToVector4(), Color.Aqua.ToVector4());
            // if (RenderPassOpaques.I != null)
            // {
            // 	ImGui.Image((IntPtr) RenderPassOpaques.I.PassRenderTexture.ColorAttachment, RenderPassOpaques.I.PassRenderTexture.Size,
            // 	           new Vector2(0, 1), new Vector2(1, 0));
            // }
            // if (_renderCameraViews && RenderPassDirectionalLightShadowDepth.I?.DebugDepthVisualisationTexture != null)
            // {
            //     var ratio = RenderPassDirectionalLightShadowDepth.I.DebugDepthVisualisationTexture.Size.Y /
            //                 RenderPassDirectionalLightShadowDepth.I.DebugDepthVisualisationTexture.Size.X;
            //     var sizeX = Mathf.ClampMax(RenderPassDirectionalLightShadowDepth.I.DebugDepthVisualisationTexture.Size.X, 400);
            //     var sizeY = sizeX * ratio;
            //
            //     ImGui.SetCursorPos(new Vector2(5, 75));
            //
            //     ImGui.Image(RenderPassDirectionalLightShadowDepth.I.DebugDepthVisualisationTexture.TextureId,
            //         new Vector2(sizeX, sizeY),
            //         new Vector2(0, 1), new Vector2(1, 0), Color.White.ToVector4(), Color.Red.ToVector4());
            // }

            bool showBloomTextures = false;
            if (showBloomTextures)
            {
                if (RenderPassBloomThreshold.I?.MainFramebuffer != null)
                {
                    float ratio = RenderPassBloomThreshold.I.MainFramebuffer.Size.Y /
                                  RenderPassBloomThreshold.I.MainFramebuffer.Size.X;
                    float sizeX = Mathf.ClampMax(RenderPassBloomThreshold.I.MainFramebuffer.Size.X, 400);
                    float sizeY = sizeX * ratio;

                    ImGui.SetCursorPos(new Vector2(5, 75));

                    Vector4 whitePixelAtlasBounds = Tofu.Editor.EditorTextures.WhitePixel.BoundingBoxInAtlas;
                    ImGui.Image(Tofu.Editor.EditorTextures.WhitePixel.AtlasGLTextureArrayId,
                        new Vector2(sizeX, sizeY),
                        whitePixelAtlasBounds.XY, whitePixelAtlasBounds.ZW, Color.BlanchedAlmond.ToVector4(),
                        Color.Red.ToVector4());

                    ImGui.SetCursorPos(new Vector2(5, 75));

                    ImGui.Image(RenderPassBloomThreshold.I.MainFramebuffer.TextureId,
                        new Vector2(sizeX, sizeY),
                        new Vector2(0, 1), new Vector2(1, 0), Color.White.ToVector4(), Color.Red.ToVector4());
                }

                if (RenderPassBloomPostProcess.I?.MainFramebuffer != null)
                {
                    float ratio = RenderPassBloomPostProcess.I.MainFramebuffer.Size.Y /
                                  RenderPassBloomPostProcess.I.MainFramebuffer.Size.X;
                    float sizeX = Mathf.ClampMax(RenderPassBloomPostProcess.I.MainFramebuffer.Size.X, 400);
                    float sizeY = sizeX * ratio;

                    ImGui.SetCursorPos(new Vector2(405, 75));

                    TofuImGui.ImageTexture2DArray(
                        runtimeTexture: Tofu.Editor.EditorTextures.WhitePixel,
                        size: new Vector2(sizeX, sizeY),
                        Color.Black.ToVector4(), Color.Red.ToVector4());

                    ImGui.SetCursorPos(new Vector2(405, 75));

                    TofuImGui.ImageTexture2D(RenderPassBloomPostProcess.I.BloomFramebufferVertical.TextureId,
                        size: new Vector2(sizeX, sizeY), new Vector4(0, 1, 1, 0), Color.White.ToVector4(),
                        Color.Red.ToVector4());
                }
            }

            ImGui.SetCursorPos(System.Numerics.Vector2.Zero);

            ImGui.SetCursorPosX(_camera.Size.X / 2 - 200 * Screen.ScaleI);

            Vector4 activeColor = Color.ForestGreen.ToVector4(); //ImGui.GetStyle().Colors[(int) ImGuiCol.Text];
            Vector4 inactiveColor = ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled];
            /*ImGui.PushStyleColor(ImGuiCol.Text, PhysicsController.Running ? activeColor : inactiveColor);
            bool physicsButtonClicked = ImGui.Button("physics");
            if (physicsButtonClicked)
            {
                if (PhysicsController.Running == false)
                {
                    PhysicsController.StartPhysics();
                }
                else if (PhysicsController.Running)
                {
                    PhysicsController.StopPhysics();
                }
            }

            ImGui.PopStyleColor();

            ImGui.SameLine();*/
//////////
            bool renderPassesButtonClicked = ImGui.Button("Render passes");

            if (renderPassesButtonClicked)
            {
                _renderPassesWindowOpened = !_renderPassesWindowOpened;
                if (_renderPassesWindowOpened)
                {
                    ImGui.OpenPopup("Render passes");
                }
            }

            if (_renderPassesWindowOpened)
            {
                if (ImGui.BeginPopupContextWindow("Render passes"))
                {
                    foreach (RenderPass renderPass in _renderTargetPipeline.RenderPasses)
                    {
                        bool isEnabled = renderPass.Enabled;
                        bool wasEnabled = isEnabled;
                        bool clicked = ImGui.Checkbox(renderPass.RenderPassType.ToString(), ref isEnabled);

                        if (clicked)
                        {
                            renderPass.Enabled = !renderPass.Enabled;
                        }
                    }

                    ImGui.EndPopup();
                }

                if (ImGui.IsPopupOpen("Render passes") == false && _renderPassesWindowOpened)
                    // clicked away
                {
                    _renderPassesWindowOpened = false;
                }
            }

            ImGui.SameLine();
            //////////
            bool renderModeButtonClicked = ImGui.Button("Render mode");

            if (renderModeButtonClicked)
            {
                _renderModeWindowOpened = !_renderModeWindowOpened;
                if (_renderModeWindowOpened)
                {
                    ImGui.OpenPopup("RenderMode");
                }
            }

            if (_renderModeWindowOpened)
            {
                if (ImGui.BeginPopupContextWindow("RenderMode"))
                {
                    foreach (ViewRenderMode mode in Enum.GetValues(typeof(ViewRenderMode)))
                    {
                        bool isEnabled = Tofu.RenderSettings.CurrentRenderModeSettings.CurrentRenderMode == mode;
                        bool wasEnabled = isEnabled;
                        bool clicked = ImGui.Checkbox(mode.ToString(), ref isEnabled);
                        bool hovered = ImGui.IsItemHovered();
                        if (hovered)
                        {
                            bool isNew = Tofu.RenderSettings.CurrentRenderModeSettings.CurrentRenderMode != mode;
                            Tofu.RenderSettings.CurrentRenderModeSettings.CurrentRenderMode = mode;
                            if (isNew)
                            {
                                Tofu.RenderSettings.SaveData();
                            }
                        }

                        if (clicked)
                        {
                            _renderModeWindowOpened = false;
                        }
                    }

                    ImGui.EndPopup();
                }

                if (ImGui.IsPopupOpen("RenderMode") == false && _renderModeWindowOpened)
                    // clicked away
                {
                    _renderModeWindowOpened = false;
                }
            }

            ImGui.SameLine();

            //////////
            /// 
            ImGui.PushStyleColor(ImGuiCol.Text,
                Tofu.RenderSettings.CurrentWireframeRenderSettings.WireframeVisible ? activeColor : inactiveColor);
            bool wireframeButtonClicked = ImGui.Button("Wireframe");
            if (wireframeButtonClicked)
            {
                Tofu.RenderSettings.CurrentWireframeRenderSettings.WireframeVisible =
                    !Tofu.RenderSettings.CurrentWireframeRenderSettings.WireframeVisible;
                Tofu.RenderSettings.SaveData();
            }

            ImGui.PopStyleColor();

            ImGui.SameLine();

            ImGui.PushStyleColor(ImGuiCol.Text, Global.GameRunning ? activeColor : inactiveColor);

            bool playButtonClicked = ImGui.Button("play");

            ImGui.PopStyleColor();

            if (playButtonClicked)
            {
                if (Global.GameRunning)
                {
                    Playmode.PlayMode_Stop();
                }
                else
                {
                    Playmode.PlayMode_Start();
                }
            }

            ImGui.SameLine();

            ImGui.SetNextItemWidth(200);

            string projectionModeButtonText =
                Tofu.SceneViewController.CurrentProjectionMode == ProjectionMode.Orthographic ? "2D" : "3D";
            bool projectionButtonClicked = ImGui.Button(projectionModeButtonText);
            if (projectionButtonClicked)
            {
                if (Tofu.SceneViewController.CurrentProjectionMode == ProjectionMode.Orthographic)
                {
                    Tofu.SceneViewController.SetProjectionMode(ProjectionMode.Perspective);
                }
                else
                {
                    Tofu.SceneViewController.SetProjectionMode(ProjectionMode.Orthographic);
                }
            }

            ImGui.End();

            ImGui.PopStyleVar();

            ImGui.PopStyleVar();
        }

        else

        {
            ImGui.SetNextWindowSize(_camera.Size + new Vector2(0, 50), ImGuiCond.Always);
            ImGui.SetNextWindowPos(new Vector2(0, 0), ImGuiCond.Always, new Vector2(0, 0));
            ImGui.Begin("Scene View",
                ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoDecoration);

            ImGui.SetCursorPosX(0);
            Tofu.Editor.SceneViewPosition = new Vector2(ImGui.GetCursorPosX(), ImGui.GetCursorPosY());

            TofuImGui.ImageTexture2D(_renderTargetPipeline.FinalFramebuffer.TextureId,
                _renderTargetPipeline.FinalFramebuffer.Size,
                new Vector4(0, 1, 1, 0));

            ImGui.End();
        }
    }

    private void HandleModelDragDrop()
    {
        if (ImGui.BeginDragDropTarget())
        {
            string? path = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);

            if (TofuImGui.PayloadHasBeenDropped(DragDropPayloadTypes.Model) ||
                TofuImGui.PayloadHasBeenDropped(DragDropPayloadTypes.Mesh))
            {
                if (path.Length > 0 && AssetPathExtensions.IsFileModel(path))
                {
                    Asset_Model modelAsset = Tofu.AssetLoadManager.Get<Asset_Model>(path);
                    SpawnModelIntoScene(model: modelAsset);
                }
                else if (path.Length > 0 && AssetPathExtensions.IsFileMesh(path))
                {
                    RuntimeMesh mesh = Tofu.AssetLoadManager.Get<RuntimeMesh>(path);
                    SpawnMeshIntoScene(mesh: mesh, 0, true);
                }
            }

            ImGui.EndDragDropTarget();
        }
    }

    private GameObject SpawnModelIntoScene(Asset_Model model)
    {
        string importParametersPath =
            AssetPathExtensions.GetPathOfImportParametersOfSourceAssetFile(model.PathInAssetsFolder);

        AssetImportParameters_Model importParameters =
            Serializer.ReadFileJSON<AssetImportParameters_Model>(importParametersPath);

        int countOfMeshes = importParameters.ImportAsSingleMesh ? 1 : model.PathsToMeshAssets.Count;
        GameObject[] meshGameObjects =
            new GameObject[countOfMeshes];

        GameObject parent = null;

        for (int i = 0; i < meshGameObjects.Length; i++)
        {
            RuntimeMesh mesh = Tofu.AssetLoadManager.Get<RuntimeMesh>(model.PathsToMeshAssets[i]);

            GameObject meshGameObject =
                SpawnMeshIntoScene(mesh, indexOfMesh: i, isSingleMeshInModel: countOfMeshes == 1);

            if (countOfMeshes == 1)
            {
                parent = meshGameObject; // so we can return just parent and be done
            }
            else
            {
                if (parent == null)
                {
                    Vector3 worldPosition =
                        _camera.Transform.TransformVectorToWorldSpaceVector(Vector3.Forward * 10);

                    string modelName =
                        Path.GetFileNameWithoutExtension(model.PathInAssetsFolder);

                    parent = GameObject.Create(position: worldPosition, name: modelName);
                }

                meshGameObject.Transform.SetParent(parent.Transform);
                meshGameObject.Transform.LocalPosition = Vector3.Zero;
            }
        }

        return parent;
    }

    private GameObject SpawnMeshIntoScene(RuntimeMesh mesh, int indexOfMesh, bool isSingleMeshInModel)
    {
        Vector3 worldPosition = _camera.Transform.TransformVectorToWorldSpaceVector(Vector3.Forward * 10);

        string name =
            Path.GetFileNameWithoutExtension(mesh.Mesh.Name);
        if (isSingleMeshInModel == false)
        {
            name = name + "_" + indexOfMesh;
        }

        GameObject go = GameObject.Create(name: name, position: worldPosition);
        BoxShape boxShape = go.AddComponent<BoxShape>();
        boxShape.Pivot = Vector3.Half;
        boxShape.Size = new Vector3(3, 3, 3);
        ModelRendererInstanced modelRendererInstanced = go.AddComponent<ModelRendererInstanced>();

        modelRendererInstanced.RuntimeMesh = mesh;

        go.Awake();

        Tofu.GameObjectSelectionManager.SelectGameObject(go);

        return go;
    }

    public override void Update()
    {
    }

    public override void Init()
    {
        I = this;

        _renderTargetPipeline = Tofu.RenderingSystem.CreatePipeline(RenderTargetPipelineType.SceneView);
    }
}