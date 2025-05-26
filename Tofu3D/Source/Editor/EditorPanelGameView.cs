using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using ImGuiNET;
using TofuEngine;
using TofuEngine.Rendering;

namespace TofuEngine;

public class EditorPanelGameView : EditorPanel
{
    public override string Name => "Game View";
    public static EditorPanelGameView I { get; private set; }
    private RenderTargetPipeline? _renderTargetPipeline;
    private bool _resolutionsPopupOpened;
    public Camera? _camera => _renderTargetPipeline?.Camera;

    public override ImGuiWindowFlags AdditionalWindowFlags => ImGuiWindowFlags.NoScrollbar |
                                                              ImGuiWindowFlags.NoScrollWithMouse;

    private Vector2[] _resolutions = new[]
    {
        new Vector2(-1, -1),
        new Vector2(1920, 1080),
        new Vector2(2560, 1000),
    };

    private PersistentObject<Vector2> _currentResolutionPersistent =
        new PersistentObject<Vector2>("GameViewCurrentResolution", new Vector2(1920, 1080));

    // need this because -1,-1
    private PersistentObject<Vector2> _currentResolutionInPopup =
        new PersistentObject<Vector2>("GameViewCurrentResolutionInPopup", new Vector2(1920, 1080));

    public override void Init()
    {
        I = this;
        if (_currentResolutionPersistent.Value.X <= 0)
        {
            _currentResolutionPersistent.Value = Size;
        }

        _renderTargetPipeline =
            Tofu.RenderingSystem.CreatePipeline(RenderTargetPipelineType.GameView, -1, _currentResolutionPersistent);
    }

    protected override void OnClosed()
    {
        if (_renderTargetPipeline != null)
        {
            Tofu.RenderingSystem.DestroyPipeline(ref _renderTargetPipeline);
        }

        base.OnClosed();
    }

    public override void Draw()
    {
        if (Global.EditorAttached)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

            bool oldIsVisible = IsVisible;


            ImGui.SetNextWindowSize(Size, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowPos(Position, ImGuiCond.FirstUseEver, Pivot);

            float controlsBarHeight = 64;
            Vector2 actualSpaceForGameView = Size - new Vector2(controlsBarHeight / Screen.Scale);
            Vector2 controlsBarHeightVector = new Vector2(0, controlsBarHeight);
            ImGui.SetNextWindowSize(_renderTargetPipeline.FinalFramebuffer.Size + controlsBarHeightVector,
                ImGuiCond.FirstUseEver);

            ImGui.SetNextWindowPos(new Vector2(0, 0), ImGuiCond.FirstUseEver, new Vector2(0, 0));
            ImGuiWindowFlags flags = Editor.ImGuiDefaultWindowFlags | ImGuiWindowFlags.NoScrollbar |
                                     ImGuiWindowFlags.NoScrollWithMouse;
            IsVisible = ImGui.Begin(Name, flags);

            DoPostWindowChecks();
            // BeginWindowDefault();

            if (oldIsVisible == false && IsVisible == true && _renderTargetPipeline == null)
            {
                _renderTargetPipeline = Tofu.RenderingSystem.CreatePipeline(RenderTargetPipelineType.GameView, -1);
            }

            if (oldIsVisible && IsVisible == false && _renderTargetPipeline != null)
            {
                Tofu.RenderingSystem.DestroyPipeline(ref _renderTargetPipeline);
            }


            if (IsVisible == false)
            {
                ImGui.End();
                return;
            }


            if (_currentResolutionPersistent != _camera.Size && _currentResolutionInPopup.Value.X <= 0)
            {
                _currentResolutionPersistent.Value = actualSpaceForGameView;
                _camera.SetSize(_currentResolutionPersistent);
                // Debug.Log("SetSize");
            }


            // ImGui.SetCursorPosX(0);
            ImGui.SetCursorPos(new Vector2(0, controlsBarHeight));

            Vector2 gameViewDisplaySize = new Vector2(_renderTargetPipeline.FinalFramebuffer.Size.X,
                _renderTargetPipeline.FinalFramebuffer.Size.Y);

            if (gameViewDisplaySize.X > Size.X)
            {
                gameViewDisplaySize = gameViewDisplaySize / (gameViewDisplaySize.X / Size.X);
            }

            if (gameViewDisplaySize.Y > actualSpaceForGameView.Y)
            {
                gameViewDisplaySize = gameViewDisplaySize / (gameViewDisplaySize.Y / actualSpaceForGameView.Y);
            }

            gameViewDisplaySize *= Screen.Scale;

            Tofu.Editor.GameViewPosition = new Vector2(ImGui.GetCursorPosX(),
                ImGuiHelper.FlipYToGoodSpace(ImGui.GetCursorPosY()) -
                _renderTargetPipeline.FinalFramebuffer.Size.Y / Screen.Scale - 15);

            // Debug.StatSetValue("aaaa", $"scne size {Tofu.RenderPassSystem.FinalFramebuffer.Size.Y / Screen.Scale}");

            if (_renderTargetPipeline.CanRender)
            {
                TofuImGui.ImageTexture2D(_renderTargetPipeline.FinalFramebuffer.TextureId,
                    gameViewDisplaySize,
                    new Vector4(0, 1, 1, 0));
            }
            else
            {
                ImGui.Dummy(gameViewDisplaySize);
            }

            ImGui.SetCursorPos(System.Numerics.Vector2.Zero);
            // ImGui.SetCursorPosX(0);
            ImGui.SetCursorPos(new Vector2(0, 0));
            ImGui.Dummy(new System.Numerics.Vector2(ImGui.GetContentRegionAvail().X, 50));
            // ImGui.SameLine();
            ImGui.SetCursorPos(new Vector2(0, controlsBarHeight / 2));

            ImGui.SetCursorPosX(10);

            bool resolutionsButtonClicked = ImGui.Button("Resolutions");

            if (resolutionsButtonClicked)
            {
                _resolutionsPopupOpened = !_resolutionsPopupOpened;
                if (_resolutionsPopupOpened)
                {
                    ImGui.OpenPopup("Resolutions");
                }
            }

            if (_resolutionsPopupOpened)
            {
                if (ImGui.BeginPopupContextWindow("Resolutions"))
                {
                    foreach (Vector2 res in _resolutions)
                    {
                        string label = res.X <= 0 ? "Free" : $"{res.X}x{res.Y}";
                        bool selected = _currentResolutionInPopup == res;
                        bool clicked = ImGui.Checkbox(label, ref selected);

                        Vector2 resForCamera = res.X <= 0 ? Size : res;
                        if (clicked)
                        {
                            _renderTargetPipeline.Camera.SetSize(resForCamera);
                            _currentResolutionPersistent.Value = resForCamera;
                            _currentResolutionInPopup.Value = res;
                        }
                    }

                    ImGui.EndPopup();
                }

                if (ImGui.IsPopupOpen("Resolutions") == false && _resolutionsPopupOpened)
                    // clicked away
                {
                    _resolutionsPopupOpened = false;
                }
            }


            ImGui.End();

            ImGui.PopStyleVar();
            ImGui.PopStyleVar();
        }

        else


        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, 0);
            if (_camera.Size != Tofu.Window.ClientSize.ToVector2())
            {
                _currentResolutionPersistent.Value = Tofu.Window.ClientSize.ToVector2();
                _camera.SetSize(_currentResolutionPersistent);
                // Debug.Log("SetSize");
            }

            ImGui.SetNextWindowSize(_camera.Size, ImGuiCond.Always);
            ImGui.SetNextWindowPos(new Vector2(0, 0), ImGuiCond.Always, new Vector2(0, 0));
            ImGui.Begin("Game View",
                ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoDecoration);

            ImGui.SetCursorPosX(0);
            Tofu.Editor.SceneViewPosition = new Vector2(ImGui.GetCursorPosX(), ImGui.GetCursorPosY());

            TofuImGui.ImageTexture2D(_renderTargetPipeline.FinalFramebuffer.TextureId,
                _renderTargetPipeline.FinalFramebuffer.Size,
                new Vector4(0, 1, 1, 0));

            ImGui.End();

            ImGui.PopStyleVar();
        }
    }


    public override void Update()
    {
    }
}