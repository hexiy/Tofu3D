using ImGuiNET;

namespace Tofu3D;

public static class TofuImGui
{
    public static Vector2 GetCursorScreenPos()
    {
        return new System.Numerics.Vector2(ImGui.GetCursorScreenPos().X / Tofu.Window.MonitorScale,
            Tofu3D.Tofu.Window.WindowSize.Y -
            (ImGui.GetCursorScreenPos().Y / Tofu.Window.MonitorScale)); // * new Vector2(-1, 1);
    }
}