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
    public Camera? _camera => _renderTargetPipeline?.Camera;

    public override ImGuiWindowFlags AdditionalWindowFlags => ImGuiWindowFlags.NoScrollbar |
                                                              ImGuiWindowFlags.NoScrollWithMouse;

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

            BeginWindowDefault();

            if (oldIsVisible == false && IsVisible == true && _renderTargetPipeline == null)
            {
                _renderTargetPipeline = Tofu.RenderingSystem.CreatePipeline(RenderTargetPipelineType.GameView);
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


            if ((Vector2)ImGui.GetWindowSize() != _camera.Size)
            {
                _camera.SetSize(ImGui.GetWindowSize());
                // Debug.Log("SetSize");
            }


            // ImGui.SetCursorPosX(0);
            ImGui.SetCursorPos(new Vector2(0, 0));

            Tofu.Editor.GameViewPosition = new Vector2(ImGui.GetCursorPosX(),
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

            ImGui.SetCursorPos(System.Numerics.Vector2.Zero);

            ImGui.SetCursorPosX(_camera.Size.X / 2 - 200 * Screen.ScaleI);

            Vector4 activeColor = Color.ForestGreen.ToVector4(); //ImGui.GetStyle().Colors[(int) ImGuiCol.Text];
            Vector4 inactiveColor = ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled];


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

            ImGui.End();

            ImGui.PopStyleVar();

            ImGui.PopStyleVar();
        }

        else

        {
            ImGui.SetNextWindowSize(_camera.Size + new Vector2(0, 50), ImGuiCond.Always);
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
        }
    }


    public override void Update()
    {
    }

    public override void Init()
    {
        I = this;

        _renderTargetPipeline = Tofu.RenderingSystem.CreatePipeline(RenderTargetPipelineType.GameView);
    }
}