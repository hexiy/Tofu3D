using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using ImGuiNET;
using TofuEngine;
using TofuEngine.Rendering;

namespace TofuEngine;

public class EditorPanelGameView : EditorPanelGenericView
{
    public override string Name => "Game View";
    private bool _resolutionsPopupOpened;


    public override ImGuiWindowFlags AdditionalWindowFlags => ImGuiWindowFlags.NoScrollbar |
                                                              ImGuiWindowFlags.NoScrollWithMouse;

    float _controlsBarHeight = 64;

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
        if (_currentResolutionPersistent.Value.X <= 0)
        {
            _currentResolutionPersistent.Value = Size;
        }

        _renderTargetPipeline =
            Tofu.RenderingSystem.CreatePipelineForView(this,RenderTargetPipelineType.GameView, -1, _currentResolutionPersistent);
        
        EditorPanelTextureViewer.AddTexture(new TextureViewerTextureData()
        {
            Name = "Gameview",
            Texture = this._renderTargetPipeline.FinalFramebuffer
        });
    }

    protected override void OnClosed()
    {
        if (_renderTargetPipeline != null)
        {
            Tofu.RenderingSystem.DestroyPipeline(ref _renderTargetPipeline);
        }

        base.OnClosed();
    }

    protected override void BeforeWindowCreated()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

        base.BeforeWindowCreated();
    }

    protected override void AfterWindowEnded()
    {
        ImGui.PopStyleVar();
        ImGui.PopStyleVar();

        base.AfterWindowEnded();
    }

    protected override void OnVisibilityChanged()
    {
        if (IsVisible && _renderTargetPipeline == null)
        {
            _renderTargetPipeline = Tofu.RenderingSystem.CreatePipelineForView(this,RenderTargetPipelineType.GameView, -1);
        }

        if (IsVisible == false && _renderTargetPipeline != null)
        {
            Tofu.RenderingSystem.DestroyPipeline(ref _renderTargetPipeline);
        }

        base.OnVisibilityChanged();
    }

    protected override void ExecuteImGuiDrawCommands()
    {
        if (_renderTargetPipeline == null)
        {
            return;
        }

        if (Global.EditorAttached)
        {
            Vector2 actualSpaceForGameView = Size - new Vector2(_controlsBarHeight / Screen.Scale);
            Vector2 controlsBarHeightVector = new Vector2(0, _controlsBarHeight);

            if (_currentResolutionPersistent != Camera.Size && _currentResolutionInPopup.Value.X <= 0)
            {
                _currentResolutionPersistent.Value = actualSpaceForGameView;
                Camera.SetSize(_currentResolutionPersistent);
                // Debug.Log("SetSize");
            }

            Size = _renderTargetPipeline.FinalFramebuffer.Size + controlsBarHeightVector;


            // ImGui.SetCursorPosX(0);
            // ImGui.SetCursorPos(new Vector2(0, _controlsBarHeight));

            ActualViewSize = new Vector2(_renderTargetPipeline.FinalFramebuffer.Size.X,
                _renderTargetPipeline.FinalFramebuffer.Size.Y);

            
            Vector2 spaceAvailable = ImGui.GetContentRegionAvail();
            float ratio = ActualViewSize.Y /
                          ActualViewSize.X;
            ActualViewSize.X = Mathf.ClampMax(ActualViewSize.X, spaceAvailable.X);
            ActualViewSize.Y = ActualViewSize.X * ratio;
            ActualViewSize.Y = Mathf.ClampMax(ActualViewSize.Y, spaceAvailable.Y);
            ActualViewSize.X = ActualViewSize.Y / ratio;
            
            
            Vector2 offset = (spaceAvailable - ActualViewSize) * 0.5f + new Vector2(0,  _controlsBarHeight);

            ImGui.SetCursorPos(offset);

            ActualViewPosition = Position+offset;
            
            // if (ActualViewSize.X > Size.X)
            // {
            //     ActualViewSize = ActualViewSize / (ActualViewSize.X / Size.X);
            // }
            //
            // if (ActualViewSize.Y > actualSpaceForGameView.Y)
            // {
            //     ActualViewSize = ActualViewSize / (ActualViewSize.Y / actualSpaceForGameView.Y);
            // }
            //
            // ActualViewSize *= Screen.Scale;

            // Tofu.Editor.GameViewPosition = new Vector2(ImGui.GetCursorPosX(),
            // ImGuiHelper.FlipYToGoodSpace(ImGui.GetCursorPosY()) -
            // _renderTargetPipeline.FinalFramebuffer.Size.Y / Screen.Scale - 15);

            // Debug.StatSetValue("aaaa", $"scne size {Tofu.RenderPassSystem.FinalFramebuffer.Size.Y / Screen.Scale}");

            if (_renderTargetPipeline.CanRender)
            {
                TofuImGui.ImageTexture2D(_renderTargetPipeline.FinalFramebuffer.TextureId,
                    ActualViewSize,
                    new Vector4(0, 1, 1, 0));
            }
            else
            {
                ImGui.Dummy(ActualViewSize);
            }

            // Tofu.MouseInput.AnyViewIsHovered = ImGui.IsItemHovered() || Tofu.MouseInput.AnyViewIsHovered;

            ImGui.SetCursorPos(System.Numerics.Vector2.Zero);
            // ImGui.SetCursorPosX(0);
            ImGui.SetCursorPos(new Vector2(0, 0));
            ImGui.Dummy(new System.Numerics.Vector2(ImGui.GetContentRegionAvail().X, 50));
            // ImGui.SameLine();
            ImGui.SetCursorPos(new Vector2(0, _controlsBarHeight / 2));

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
        }
        else
        {
            if (Camera.Size != Tofu.Window.ClientSize.ToVector2())
            {
                _currentResolutionPersistent.Value = Tofu.Window.ClientSize.ToVector2();
                Camera.SetSize(_currentResolutionPersistent);
                // Debug.Log("SetSize");
            }

            ImGui.SetCursorPosX(0);
            ActualViewPosition = new Vector2(ImGui.GetCursorPosX(), ImGui.GetCursorPosY());

            TofuImGui.ImageTexture2D(_renderTargetPipeline.FinalFramebuffer.TextureId,
                _renderTargetPipeline.FinalFramebuffer.Size,
                new Vector4(0, 1, 1, 0));

            // IsPanelHovered=
            // Tofu.MouseInput.AnyViewIsHovered = ImGui.IsItemHovered() || Tofu.MouseInput.AnyViewIsHovered;
        }
    }


    public override void Update()
    {
    }
}