using ImGuiNET;

namespace TofuEngine;

public class EditorPanelFloating : EditorPanel
{
    public static EditorPanelFloating I { get; private set; }

    public override void Init()
    {
        I = this;
    }

    protected override void ExecuteImGuiDrawCommands()
    {
        //ImGui.SetNextWindowBgAlpha (0);
        ImGui.Begin("Floating", ImGuiWindowFlags.NoCollapse);

        // ImGui.Image(Tofu.RenderPassSystem.FinalFramebuffer.TextureId, new Vector2(300, 300));

        ImGui.End();
    }

    public void Update()
    {
    }
}