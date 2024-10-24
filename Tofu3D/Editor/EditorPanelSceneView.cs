using ImGuiNET;
using Tofu3D.Rendering;

namespace Tofu3D;

public class EditorPanelSceneView : EditorPanel
{
    private static bool _renderCameraViews = true;

    private bool _renderModeWindowOpened;
    private bool _renderPassesWindowOpened;
    public override string Name => "Scene View";
    public static EditorPanelSceneView I { get; private set; }

    public override void Draw()
    {
        if (Active == false)
        {
            return;
        }

        if (Global.EditorAttached)
        {
            _renderCameraViews = true || /*Global.Debug &&*/
                                 GameObjectSelectionManager.GetSelectedGameObject()?.GetComponent<DirectionalLight>() !=
                                 null;

            // int tooltipsPanelHeight = 70;
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

            Tofu.Editor.SceneViewSize =
                Tofu.RenderPassSystem.FinalRenderTexture.Size / Screen.Scale; // + new Vector2(0, tooltipsPanelHeight);

            ImGui.SetNextWindowSize(Tofu.RenderPassSystem.FinalRenderTexture.Size, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowPos(new Vector2(0, 0), ImGuiCond.FirstUseEver, new Vector2(0, 0));
            ImGuiWindowFlags flags = Editor.ImGuiDefaultWindowFlags | ImGuiWindowFlags.NoScrollbar |
                                     ImGuiWindowFlags.NoScrollWithMouse;
            if (IsFullscreen)
            {
            }

            ImGui.Begin(Name, flags);

            if ((Vector2)ImGui.GetWindowSize() != Camera.MainCamera.Size)
            {
                Camera.MainCamera.SetSize(ImGui.GetWindowSize());
                // Debug.Log("SetSize");
            }


            // ImGui.SetCursorPosX(0);
            ImGui.SetCursorPos(new Vector2(0, 0));

            Tofu.Editor.SceneViewPosition = new Vector2(ImGui.GetCursorPosX(),
                ImGuiHelper.FlipYToGoodSpace(ImGui.GetCursorPosY()) -
                Tofu.RenderPassSystem.FinalRenderTexture.Size.Y / Screen.Scale - 15);

            Debug.StatSetValue("aaaa", $"scne size {Tofu.RenderPassSystem.FinalRenderTexture.Size.Y / Screen.Scale}");

            if (Tofu.RenderPassSystem.CanRender)
            {
                ImGui.Image(Tofu.RenderPassSystem.FinalRenderTexture.ColorAttachmentID,
                    Tofu.RenderPassSystem.FinalRenderTexture.Size,
                    new Vector2(0, 1), new Vector2(1, 0));
            }
            else
            {
                ImGui.Dummy(Tofu.RenderPassSystem.FinalRenderTexture.Size);
            }

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
            //     ImGui.Image(RenderPassDirectionalLightShadowDepth.I.DebugDepthVisualisationTexture.ColorAttachmentID,
            //         new Vector2(sizeX, sizeY),
            //         new Vector2(0, 1), new Vector2(1, 0), Color.White.ToVector4(), Color.Red.ToVector4());
            // }
            if (RenderPassBloomThreshold.I?.PassRenderTexture != null)
            {
                var ratio = RenderPassBloomThreshold.I.PassRenderTexture.Size.Y /
                            RenderPassBloomThreshold.I.PassRenderTexture.Size.X;
                var sizeX = Mathf.ClampMax(RenderPassBloomThreshold.I.PassRenderTexture.Size.X, 400);
                var sizeY = sizeX * ratio;

                ImGui.SetCursorPos(new Vector2(5, 75));

                ImGui.Image(Tofu.Editor.EditorTextures.WhitePixel.TextureId,
                    new Vector2(sizeX, sizeY),
                    new Vector2(0, 1), new Vector2(1, 0), Color.Black.ToVector4(), Color.Red.ToVector4());

                ImGui.SetCursorPos(new Vector2(5, 75));

                ImGui.Image(RenderPassBloomThreshold.I.PassRenderTexture.ColorAttachmentID,
                    new Vector2(sizeX, sizeY),
                    new Vector2(0, 1), new Vector2(1, 0), Color.White.ToVector4(), Color.Red.ToVector4());
            }

            if (RenderPassBloomPostProcess.I?.PassRenderTexture != null)
            {
                var ratio = RenderPassBloomPostProcess.I.PassRenderTexture.Size.Y /
                            RenderPassBloomPostProcess.I.PassRenderTexture.Size.X;
                var sizeX = Mathf.ClampMax(RenderPassBloomPostProcess.I.PassRenderTexture.Size.X, 400);
                var sizeY = sizeX * ratio;

                ImGui.SetCursorPos(new Vector2(405, 75));

                ImGui.Image(Tofu.Editor.EditorTextures.WhitePixel.TextureId,
                    new Vector2(sizeX, sizeY),
                    new Vector2(0, 1), new Vector2(1, 0), Color.Black.ToVector4(), Color.Red.ToVector4());

                ImGui.SetCursorPos(new Vector2(405, 75));

                ImGui.Image(RenderPassBloomPostProcess.I.BloomRenderTextureVertical.ColorAttachmentID,
                    new Vector2(sizeX, sizeY),
                    new Vector2(0, 1), new Vector2(1, 0), Color.White.ToVector4(), Color.Red.ToVector4());
            }

            ImGui.SetCursorPos(System.Numerics.Vector2.Zero);

            ImGui.SetCursorPosX(Camera.MainCamera.Size.X / 2 - 400);

            var activeColor = Color.ForestGreen.ToVector4(); //ImGui.GetStyle().Colors[(int) ImGuiCol.Text];
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
            var renderPassesButtonClicked = ImGui.Button("Render passes");

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
                    foreach (var renderPass in Tofu.RenderPassSystem.RenderPasses)
                    {
                        var isEnabled = renderPass.Enabled;
                        var wasEnabled = isEnabled;
                        var clicked = ImGui.Checkbox(renderPass.RenderPassType.ToString(), ref isEnabled);

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
            var renderModeButtonClicked = ImGui.Button("Render mode");

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
                        var isEnabled = Tofu.RenderSettings.CurrentRenderModeSettings.CurrentRenderMode == mode;
                        var wasEnabled = isEnabled;
                        var clicked = ImGui.Checkbox(mode.ToString(), ref isEnabled);
                        var hovered = ImGui.IsItemHovered();
                        if (hovered)
                        {
                            var isNew = Tofu.RenderSettings.CurrentRenderModeSettings.CurrentRenderMode != mode;
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
            var wireframeButtonClicked = ImGui.Button("Wireframe");
            if (wireframeButtonClicked)
            {
                Tofu.RenderSettings.CurrentWireframeRenderSettings.WireframeVisible =
                    !Tofu.RenderSettings.CurrentWireframeRenderSettings.WireframeVisible;
                Tofu.RenderSettings.SaveData();
            }

            ImGui.PopStyleColor();

            ImGui.SameLine();

            ImGui.PushStyleColor(ImGuiCol.Text, Global.GameRunning ? activeColor : inactiveColor);

            var playButtonClicked = ImGui.Button("play");

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

            var projectionModeButtonText =
                Tofu.SceneViewController.CurrentProjectionMode == ProjectionMode.Orthographic ? "2D" : "3D";
            var projectionButtonClicked = ImGui.Button(projectionModeButtonText);
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
            ImGui.SetNextWindowSize(Camera.MainCamera.Size + new Vector2(0, 50), ImGuiCond.Always);
            ImGui.SetNextWindowPos(new Vector2(0, 0), ImGuiCond.Always, new Vector2(0, 0));
            ImGui.Begin("Scene View",
                ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoDecoration);

            ImGui.SetCursorPosX(0);
            Tofu.Editor.SceneViewPosition = new Vector2(ImGui.GetCursorPosX(), ImGui.GetCursorPosY());
            ImGui.Image(Tofu.RenderPassSystem.FinalRenderTexture.ColorAttachmentID, Camera.MainCamera.Size,
                new Vector2(0, 1), new Vector2(1, 0));

            ImGui.End();
        }
    }

    public override void Update()
    {
    }

    public override void Init()
    {
        I = this;
    }
}