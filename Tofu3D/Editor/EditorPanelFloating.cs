using ImGuiNET;

namespace Tofu3D;

public class EditorPanelFloating : EditorPanel
{
    public static EditorPanelFloating I { get; private set; }

    public override void Init()
    {
        I = this;
    }

    public override void Draw()
    {
        if (Active == false) return;

        //ImGui.SetNextWindowBgAlpha (0);
        ImGui.Begin("Floating", ImGuiWindowFlags.NoCollapse);

        ImGui.Image((IntPtr)Tofu.RenderPassSystem.FinalRenderTexture.ColorAttachment, new Vector2(300, 300));

        ImGui.End();
    }

    public override void Update()
    {
    }
}